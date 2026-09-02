using Application.Interfaces.Auth;
using EduGate.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using System.Security.Claims;
using System.Text;
namespace Infrastructure.Authentication
{
    public class JwtProvider:IJwtProvider
    {
        private readonly IConfiguration _configuration;
        public JwtProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GenerateToken(User user)
        {
            var secretKey = _configuration["Jwt:Secret"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // 1. استخدام List بدلاً من Array لدعم الإضافة الديناميكية
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName)
            };

            // 2. فحص وإضافة الرتب إن وجدت
            if (user.UserRoles != null)
            {
                foreach (var userRole in user.UserRoles)
                {
                    // تأكد أن الكيان Role يحتوي على خاصية Name أو غيرها حسب تصميمك
                    claims.Add(new Claim(ClaimTypes.Role, userRole.Role.RoleName));
                }
            }

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = credentials
            };

            var handler = new JsonWebTokenHandler();
            return handler.CreateToken(descriptor);
        }
    }
}
