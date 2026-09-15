using Application.DTOs.Grade;
using Application.Interfaces.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using EduGate.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class GradingService:IGradingService
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ISectionRepository _sectionRepository;
        public Guid ProfessorId =>
       _currentUserService.GetClaimAsGuid("ProfessorId")
       ?? throw new UnauthorizedAccessException("Current user is not a valid professor.");
        public GradingService(IEnrollmentRepository enrollmentRepository, ISectionRepository sectionRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _sectionRepository = sectionRepository;
            _enrollmentRepository = enrollmentRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }
        public async Task SubmitSectionGradesAsync(Guid sectionId, List<StudentGradeDto> grades, CancellationToken cancellationToken = default)
        {
           
            if(!await _sectionRepository.IsProfessorAssignedToSectionAsync(sectionId, ProfessorId))
                throw new UnauthorizedAccessException("The current professor is not assigned to this section.");

            var enrollments = await _enrollmentRepository.GetEnrollmentsBySectionIdAsync(sectionId, cancellationToken);

            if (!enrollments.Any())
                throw new KeyNotFoundException("There is no active enrollment for this section.");

            var enrollmentMap = enrollments.ToDictionary(e => e.StudentId);

            // 2. تحديث العلامة والحالة لكل طالب
            foreach (var item in grades)
            {
                if (!enrollmentMap.TryGetValue(item.StudentId, out var enrollment))
                {
                    throw new InvalidOperationException($"Student with ID '{item.StudentId}' is not actively enrolled in this section.");
                }

                enrollment.SetGrade(item.Grade);
            }

            // 3. حفظ التعديلات
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        public async Task UpdateStudentGradeAsync(Guid sectionId, Guid studentId, decimal newGrade, CancellationToken cancellationToken = default)
        {

            if (!await _sectionRepository.IsProfessorAssignedToSectionAsync(sectionId, ProfessorId))
                throw new UnauthorizedAccessException("The current professor is not assigned to this section.");
            // 1. جلب تسجيل الطالب في هذه الشعبة
            var enrollment = await _enrollmentRepository.GetBySectionAndStudentIdAsync(sectionId, studentId, cancellationToken);

            if (enrollment == null)
                throw new KeyNotFoundException($"No enrollment record found for student '{studentId}' in section '{sectionId}'.");

            // 2. تطبيق التعديل (SetGrade تفحص النطاق 0-100 وتغير الحالة Completed / Failed تلقائياً)
            enrollment.SetGrade(newGrade);

            // 3. حفظ التعديل
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        public async Task<int> FinalizeSemesterGradesAsync(Guid semesterId, CancellationToken cancellationToken = default)
        {
            if (semesterId == Guid.Empty)
                throw new ArgumentException("SemesterId cannot be empty.", nameof(semesterId));

            // استدعاء الكويري لحساب المعدلات والإنذارات
            var affectedStudents = await _enrollmentRepository.ExecuteBulkGpaRecalculationAsync(semesterId, cancellationToken);

            return affectedStudents;
        }
    }
}
