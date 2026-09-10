using Application.DTOs.Auth;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public RefreshTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Add(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Add(refreshToken);
        }
        public async Task<RefreshTokenWithUserDto?> GetByTokenWithUserAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _context.RefreshTokens
                .Where(rt => rt.Token == token)
                .Select(rt => new RefreshTokenWithUserDto
                {
                    RefreshToken = rt, // هذا الكيان رح يظل Tracked عشان تعدل عليه
                    User = new UserAuthDto
                    {
                        UserId = rt.User.UserId,
                        Email = rt.User.Email,
                        UserName = rt.User.UserName,
                        Roles = rt.User.UserRoles.Select(ur => ur.Role.RoleName).ToList()
                    }
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
        public void Revoke(Guid refreshTokenId)
        {
            var trackedToken = _context.RefreshTokens.Local.FirstOrDefault(r => r.Id == refreshTokenId);

            if (trackedToken != null)
            {
                trackedToken.IsRevoked = true;
            }
            else
            {
                var token = new RefreshToken
                {
                    Id = refreshTokenId,
                    IsRevoked = true
                };
                _context.RefreshTokens.Attach(token);
                _context.Entry(token).Property(r => r.IsRevoked).IsModified = true;
            }
        }
    }
}

