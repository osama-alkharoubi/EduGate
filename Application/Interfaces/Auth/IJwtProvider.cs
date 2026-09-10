using Application.DTOs.Auth;
using EduGate.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Auth
{
    public interface IJwtProvider
    {
        Task<string> GenerateTokenAsync(UserAuthDto user);
        string GenerateRefreshToken();
        Task<Guid?> GetUserIdFromExpiredTokenAsync(string accessToken);
    }
}
