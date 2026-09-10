
using Application.DTOs.Course;

using Application.Interfaces.Repositories;
using EduGate.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly ApplicationDbContext _context;

    public CourseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Course?> GetByIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _context.Courses
            .FirstOrDefaultAsync(c => c.CourseId == courseId, cancellationToken);
    }

    public async Task<Course?> GetByIdWithPrerequisitesAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _context.Courses
            .Include(c => c.Prerequisites)
                .ThenInclude(p => p.PrerequisiteCourse)
            .FirstOrDefaultAsync(c => c.CourseId == courseId, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string courseCode, CancellationToken cancellationToken = default)
    {
        return await _context.Courses
            .AnyAsync(c => c.CourseCode == courseCode, cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _context.Courses
            .AnyAsync(c => c.CourseId == courseId, cancellationToken);
    }

    public async Task<IReadOnlyList<CourseDto>> GetAllActiveCoursesAsync( Guid? departmentId = null, Guid? collegeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Courses
        .AsNoTracking()
        .Where(c => c.IsActive);
        if (departmentId.HasValue)
        {
            query = query.Where(c => c.DepartmentId == departmentId);
        }
        if (collegeId.HasValue) {
            query = query.Where(c => c.Department.CollegeId == collegeId);
        }
        return await query
          .Select(c => new CourseDto
          {
              CourseId = c.CourseId,
              CourseCode = c.CourseCode,
              CourseName = c.CourseName,
              CreditHours = c.CreditHours,
              DepartmentId = c.DepartmentId,
              DepartmentName = c.Department!.DepartmentName,
              IsActive = c.IsActive
          })
          .ToListAsync(cancellationToken);
    }

    public void Add(Course course)
    {
        _context.Courses.Add(course);
    }
    
    public void Update(Course course)
    {
        _context.Courses.Update(course);
    }

    public void Remove(Course course)
    {
        _context.Courses.Remove(course);
    }
}