using Application.Interfaces.Repositories.User;
using EduGate.Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

using Application.DTOs.Auth;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await  _context.Users.FindAsync( userId);
        }
        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
        public  void  Add(User user)
        {
            _context.Users.Add(user);
           
        }
        public async Task<bool> ExistsByEmailAsync(string email="",  CancellationToken cancellationToken = default)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
        public async Task<UserAuthDto?> GetForAuthByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users
         .Where(u => u.Email == email)
         .Select(u => new UserAuthDto
         {
             UserId = u.UserId,
             Email = u.Email,
             UserName = u.UserName,
             HashPassword = u.HashPassword,
             Roles = u.UserRoles
                 .Select(ur => ur.Role.RoleName)
                 .ToList()
         })
         .FirstOrDefaultAsync(cancellationToken);

        }
    }
}
