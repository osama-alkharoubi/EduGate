using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repositories
{
    public class AcademicValidationRepository : IAcademicValidationRepository
    {
        private readonly ApplicationDbContext _context;

        public AcademicValidationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UniversitySetting> GetUniversitySettingsAsync(CancellationToken cancellationToken = default)
        {
          var universitySettings = await _context.UniversitySettings.FirstOrDefaultAsync(cancellationToken);
            if (universitySettings == null)
                throw new InvalidOperationException("University settings not found.");

            return universitySettings;
        }

        public async Task<(bool IsGraduating, byte HasAcademicWarning)?> GetStudentStatusAsync(Guid studentId, CancellationToken cancellationToken = default)
        {
            var student = await _context.Students
                .Where(s => s.StudentId == studentId)
                .Select(s => new { s.IsGraduating, s.AcademicWarningsCount })
                .FirstOrDefaultAsync(cancellationToken);

            if (student == null) return null;

            return (student.IsGraduating, student.AcademicWarningsCount);
        }

        public async Task<int> GetTotalCreditHoursAsync(List<Guid> sectionIds, CancellationToken cancellationToken = default)
        {
            return await _context.Sections
                .Where(s => sectionIds.Contains(s.SectionId))
                .SumAsync(s => s.Course.CreditHours, cancellationToken);
        }

        public async Task<bool> HasUnmetPrerequisitesAsync(
    Guid studentId,
    List<Guid> sectionsToAdd,
    CancellationToken cancellationToken = default)
        {
            if (sectionsToAdd == null || !sectionsToAdd.Any())
                return false;

            // استعلام فرعي للمواد المراد تسجيلها (IQueryable بدون جلب داتا)
            var requestedCourseIds = _context.Sections
                .Where(s => sectionsToAdd.Contains(s.SectionId))
                .Select(s => s.CourseId);

            // استعلام فرعي للمواد التي اجتازها الطالب بنجاح (IQueryable بدون جلب داتا)
            var passedCourseIds = _context.Enrollments
                .Where(e => e.StudentId == studentId && (e.Status == enEnrollmentStatus.Completed || e.Status == enEnrollmentStatus.Failed))
                .Select(e => e.Section.CourseId);

            // استعلام واحد يترجم إلى EXISTS في الداتابيز: هل يوجد أي متطلب لم يجتزه الطالب؟
            return await _context.CoursePrerequisites
           .Where(cp => requestedCourseIds.Contains(cp.CourseId))
           .AnyAsync(cp => !passedCourseIds.Contains(cp.PrerequisiteId), cancellationToken);
        }

    }
}
