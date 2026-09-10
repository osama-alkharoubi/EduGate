using Application.DTOs.Auth;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        void  Add(RefreshToken refreshToken);
        Task<RefreshTokenWithUserDto?> GetByTokenWithUserAsync(string token, CancellationToken cancellationToken = default);
        void Revoke(Guid refreshTokenId);

    }
}
