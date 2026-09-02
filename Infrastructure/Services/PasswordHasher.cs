using Application.Interfaces.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class PasswordHasher :IPasswordHasher
    {
        public  string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        public  bool Verify(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
