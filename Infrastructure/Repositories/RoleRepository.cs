using Application.Interfaces.Repositories;
using EduGate.Domain.Entities;
using Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public RoleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid?> GetIdByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _context.Roles
        .Where(r => r.RoleName == name)
        .Select(r => (Guid?)r.RoleId)
        .FirstOrDefaultAsync(cancellationToken);
        }


    }
}
