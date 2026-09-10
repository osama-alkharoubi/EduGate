using Application.DTOs.Auth;
using Application.Interfaces.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Authentication
{
    public class JwtProvider : IJwtProvider
    {
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryMinutes;
        private readonly SymmetricSecurityKey _securityKey;
        private readonly SigningCredentials _signingCredentials;
        private readonly JsonWebTokenHandler _tokenHandler;
        private readonly IEnumerable<IRoleClaimProvider> _roleClaimProviders;

        public JwtProvider(
            IConfiguration configuration,
            IEnumerable<IRoleClaimProvider> roleClaimProviders)
        {
            _roleClaimProviders = roleClaimProviders;

            var jwtSection = configuration.GetSection("JwtSettings");

            var secretKey = jwtSection["Secret"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT Secret is missing from configuration.");
            }

            _issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("JWT Issuer is missing.");
            _audience = jwtSection["Audience"] ?? throw new InvalidOperationException("JWT Audience is missing.");

            _expiryMinutes = int.TryParse(jwtSection["ExpiryMinutes"], out var minutes) ? minutes : 60;

            _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            _signingCredentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);
            _tokenHandler = new JsonWebTokenHandler();
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<string> GenerateTokenAsync(UserAuthDto user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName)
            };

            if (user.Roles != null && user.Roles.Count > 0)
            {
                foreach (var role in user.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));

                    // البحث عن المزود المطابق للدور الحالي دون if/else
                    var provider = _roleClaimProviders.FirstOrDefault(p => p.TargetRole == role);
                    if (provider != null)
                    {
                        var roleClaim = await provider.GetRoleClaimAsync(user.UserId);
                        claims.Add(roleClaim);
                    }
                }
            }

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _issuer,
                Audience = _audience,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_expiryMinutes),
                SigningCredentials = _signingCredentials
            };

            return _tokenHandler.CreateToken(descriptor);
        }

        public async Task<Guid?> GetUserIdFromExpiredTokenAsync(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                return null;

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _securityKey,
                ValidateLifetime = false
            };

            if (!_tokenHandler.CanReadToken(accessToken))
            {
                return null;
            }

            var validationResult = await _tokenHandler.ValidateTokenAsync(accessToken, tokenValidationParameters);

            if (!validationResult.IsValid)
            {
                return null;
            }

            if (validationResult.SecurityToken is not JsonWebToken jsonWebToken ||
                !jsonWebToken.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            var userIdClaim = validationResult.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                           ?? validationResult.ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}