using Application.Interfaces.Repositories;
using EduGate.Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SemesterRepository : ISemesterRepository
{
    private readonly ApplicationDbContext _context;

    public SemesterRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task DeactivateAllExceptAsync(Guid excludedSemesterId, CancellationToken cancellationToken = default)
    {
        await _context.Semesters
            .Where(s => s.IsActive && s.SemesterId != excludedSemesterId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.IsActive, false), cancellationToken);
    }
    public async Task<Semester?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Semesters
            .FirstOrDefaultAsync(s => s.SemesterId == id, cancellationToken);
    }

    public async Task<List<Semester>> GetActiveSemestersAsync(CancellationToken cancellationToken)
    {
        return await _context.Semesters
            .Where(s => s.IsActive)
            .ToListAsync(cancellationToken);
    }

    public void  Add(Semester semester)
    {
         _context.Add(semester);
    }

  
}