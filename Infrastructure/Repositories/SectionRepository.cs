using Application.DTOs.Section;
using Domain.Enums;
using EduGate.Domain.Entities;
using EduGate.Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduGate.Infrastructure.Persistence.Repositories;

public class SectionRepository : ISectionRepository
{
    private readonly ApplicationDbContext _context;

    public SectionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Section?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sections
            .FirstOrDefaultAsync(s => s.SectionId == id, cancellationToken);
    }

    public async Task<Section?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sections
            .Include(s => s.Course)
            .Include(s => s.Semester)
            .Include(s => s.Professor)
            .FirstOrDefaultAsync(s => s.SectionId == id, cancellationToken);
    }

    public async Task<IEnumerable<Section>> GetAvailableSectionsAsync(Guid courseId, Guid semesterId, CancellationToken cancellationToken = default)
    {
        return await _context.Sections
            .Include(s => s.Professor)
            .Where(s => s.CourseId == courseId && s.SemesterId == semesterId)
            .AsNoTracking() // الأداء هون أسرع لأننا بس بنقرا البيانات للعرض
            .ToListAsync(cancellationToken);
    }
    public async Task<bool> HasProfessorScheduleConflictAsync(
       Guid professorId,
       Guid semesterId,
       string daysOfWeek,
       TimeOnly startTime,
       TimeOnly endTime,
       Guid? excludeSectionId = null,
       CancellationToken cancellationToken = default)
    {
        var query = _context.Sections
            .Where(s => s.ProfessorId == professorId && s.SemesterId == semesterId);

        if (excludeSectionId.HasValue)
        {
            query = query.Where(s => s.SectionId != excludeSectionId.Value);
        }

        return await query.AnyAsync(s =>
            s.DaysOfWeek == daysOfWeek &&
            s.StartTime < endTime &&
            s.EndTime > startTime,
            cancellationToken);
    }

    public async Task<bool> IsSectionNumberExistsAsync(
      Guid courseId,
      Guid semesterId,
      int sectionNumber,
      Guid? excludeSectionId = null,
      CancellationToken cancellationToken = default)
    {
        var query = _context.Sections
            .Where(s => s.CourseId == courseId &&
                        s.SemesterId == semesterId &&
                        s.SectionNumber == sectionNumber);

        if (excludeSectionId.HasValue)
        {
            query = query.Where(s => s.SectionId != excludeSectionId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IEnumerable<AvailableSectionDto>> GetAvailableSectionsForStudentAsync(
    Guid studentId,
    Guid semesterId,
    CancellationToken cancellationToken = default)
    {
        return await _context.Sections
            // 1. تصفية شعب الفصل الحالي
            .Where(s => s.SemesterId == semesterId)

            // 2. استثناء المواد اللي ناجح فيها أو مسجلها
            .Where(s => !_context.Enrollments.Any(e =>
                e.StudentId == studentId &&
                e.Section.CourseId == s.CourseId &&
                (e.Status == enEnrollmentStatus.Completed || e.Status == enEnrollmentStatus.Enrolled)))

            // 3. فحص المتطلبات السابقة
            .Where(s => _context.CoursePrerequisites
                .Where(cp => cp.CourseId == s.CourseId)
                .All(cp => _context.Enrollments.Any(e =>
                    e.StudentId == studentId &&
                    e.Section.CourseId == cp.PrerequisiteId &&
                    e.Status == enEnrollmentStatus.Completed)))

            // 4. الـ Projection: سحب الداتا المطلوبة فقط وتشكيل الـ DTO
            .Select(s => new AvailableSectionDto
            {
                SectionId = s.SectionId,
                CourseName = s.Course.CourseName, // EF Core رح يعمل JOIN لحاله
                SectionNumber = s.SectionNumber,
                ProfessorName = s.Professor.User.FirstName + " " + s.Professor.User.LastName, // EF Core رح يعمل JOIN لحاله
                DaysOfWeek = s.DaysOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Capacity = s.Capacity,
                EnrolledCount = s.Enrollments.Count(e => e.Status == enEnrollmentStatus.Enrolled) // Sub-Query سريعة لمعرفة المسجلين
            })
            .ToListAsync(cancellationToken);
    }
    public  void Add(Section section)
    {
         _context.Sections.Add(section);
    }

    public void Update(Section section)
    {
        _context.Sections.Update(section);
    }
    public async Task<int> GetActiveEnrollmentCountAsync(
    Guid sectionId,
    CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .CountAsync(e => e.SectionId == sectionId && e.Status == enEnrollmentStatus.Enrolled, cancellationToken);
    }

    public void Delete(Section section)
    {
        _context.Sections.Remove(section);
    }
}