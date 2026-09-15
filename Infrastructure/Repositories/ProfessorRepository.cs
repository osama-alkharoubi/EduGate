using Application.Interfaces.Repositories;
using Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repositories
{
    public class ProfessorRepository: IProfessorRepository
    {
       private readonly ApplicationDbContext _context;
        public ProfessorRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public   async Task<Guid?> GetProfessorIdByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Professors
                .Where(p => p.UserId == userId)
                .Select(p => (Guid?)p.ProfessorId)
                .FirstOrDefaultAsync(cancellationToken);

        }
    }
}
