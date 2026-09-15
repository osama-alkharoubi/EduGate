using Application.DTOs.Schedule;
using Application.Interfaces.Repositories;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ScheduleRepository : IScheduleRepository
{
    private readonly ApplicationDbContext _context;

    public ScheduleRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<IReadOnlyList<ProfessorScheduleDto>> GetProfessorTeachingScheduleAsync(
    Guid professorId,
    Guid? semesterId = null,
    CancellationToken cancellationToken = default)
    {
        var query = _context.Sections
            .AsNoTracking()
            .Where(s => s.ProfessorId == professorId);


        if (semesterId.HasValue)
            query = query.Where(s => s.SemesterId == semesterId.Value);
        else
            query = query.Where(s => s.Semester.IsActive);

        return await query
            .OrderBy(s => s.DaysOfWeek)
            .ThenBy(s => s.StartTime)
            .Select(s => new ProfessorScheduleDto(
                s.SectionId,
                s.Course.CourseCode,
                s.Course.CourseName,
                s.Course.CreditHours,
                s.SectionNumber,
                s.DaysOfWeek,
                s.StartTime,
                s.EndTime,
                s.RoomNumber,
                s.Capacity,
                s.Enrollments.Count(e => e.Status != enEnrollmentStatus.Dropped)
            ))
            .ToListAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<StudentScheduleDto>> GetActiveSemesterScheduleAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.StudentId == studentId
                     && e.Status != enEnrollmentStatus.Dropped
                     && e.Section.Semester.IsActive)
            .OrderBy(e => e.Section.DaysOfWeek)
            .ThenBy(e => e.Section.StartTime)
            .Select(e => new StudentScheduleDto(
                e.SectionId,
                e.Section.Course.CourseCode,
                e.Section.Course.CourseName,
                e.Section.Course.CreditHours,
                e.Section.SectionNumber,
                e.Section.DaysOfWeek,
                e.Section.StartTime,
                e.Section.EndTime,
                e.Section.RoomNumber,
                e.Section.Professor != null && e.Section.Professor.User != null
                    ? e.Section.Professor.User.UserName
                    : "TBA"
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StudentScheduleDto>> GetScheduleBySemesterIdAsync(Guid studentId, Guid semesterId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.StudentId == studentId
                     && e.Status != enEnrollmentStatus.Dropped
                     && e.Section.SemesterId == semesterId)
            .OrderBy(e => e.Section.DaysOfWeek)
            .ThenBy(e => e.Section.StartTime)
            .Select(e => new StudentScheduleDto(
                e.SectionId,
                e.Section.Course.CourseCode,
                e.Section.Course.CourseName,
                e.Section.Course.CreditHours,
                e.Section.SectionNumber,
                e.Section.DaysOfWeek,
                e.Section.StartTime,
                e.Section.EndTime,
                e.Section.RoomNumber,
                e.Section.Professor != null && e.Section.Professor.User != null
                    ? e.Section.Professor.User.UserName
                    : "TBA"
            ))
            .ToListAsync(cancellationToken);
    }
}