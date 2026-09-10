using Application.DTOs;
using Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default);
        public Task<RegisterStudentResponseDto> RegisterStudentAsync(RegisterStudentRequestDto request, CancellationToken cancellationToken = default);
    }
}
