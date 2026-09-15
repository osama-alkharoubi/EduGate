using Application.DTOs.Enrollment;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IAcademicValidationService _academicValidationService;

    public EnrollmentService(
        IEnrollmentRepository enrollmentRepository,
        IAcademicValidationService academicValidationService)
    {
        _enrollmentRepository = enrollmentRepository;
        _academicValidationService = academicValidationService;
    }

    public async Task SyncStudentScheduleAsync(
        Guid studentId,
        ModifyScheduleRequestDto modifyScheduleRequestDto,
        CancellationToken cancellationToken = default)
    {

        var sectionsToAdd = modifyScheduleRequestDto.SectionsToAdd ?? new List<Guid>();
        var sectionsToDrop = modifyScheduleRequestDto.SectionsToDrop ?? new List<Guid>();

        // إذا الطلب فاضي، بننهي العملية
        if (!sectionsToAdd.Any() && !sectionsToDrop.Any())
            return;

        // 1. فحص المتطلبات السابقة للمواد اللي الطالب بده يضيفها
        if (sectionsToAdd.Any())
        {
            await _academicValidationService.ValidatePrerequisitesAsync(studentId, sectionsToAdd, cancellationToken);
        }

        var currentSectionIds = await _enrollmentRepository.GetActiveStudentSectionIdsAsync(studentId, cancellationToken);


        var proposedSectionIds = currentSectionIds
            .Except(sectionsToDrop)
            .Union(sectionsToAdd)
            .ToList();

        // 4. فحص الحد الأدنى والأقصى للساعات على الجدول المتوقع
        await _academicValidationService.ValidateCreditHoursAsync(studentId, proposedSectionIds, cancellationToken);
        var hasConflict = await _enrollmentRepository.HasBatchScheduleConflictAsync(studentId, sectionsToAdd, sectionsToDrop, cancellationToken);
        if (hasConflict)
        {
            throw new InvalidOperationException("There is a schedule conflict in the proposed sections.");
        }

        // 5. تمرير الطلب للـ Repository لتنفيذ المعاملة الذرية (Transaction)
        await _enrollmentRepository.ModifyScheduleAtomicAsync(studentId, sectionsToAdd, sectionsToDrop, cancellationToken);
    }
}

   

