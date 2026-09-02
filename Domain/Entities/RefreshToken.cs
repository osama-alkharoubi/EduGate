using EduGate.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresOnUtc { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime CreatedOnUtc { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
