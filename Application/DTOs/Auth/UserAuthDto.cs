using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Auth
{
    public class UserAuthDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string HashPassword { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}
