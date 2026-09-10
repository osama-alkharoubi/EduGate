using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Auth
{
    public class RefreshTokenWithUserDto
    {
        public RefreshToken RefreshToken { get; set; } = null!;
        public UserAuthDto User { get; set; } = null!;
    }
}
