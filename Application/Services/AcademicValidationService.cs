using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services;

public class AcademicValidationService : IAcademicValidationService
{
    private readonly IAcademicValidationRepository _repository;

    public AcademicValidationService(IAcademicValidationRepository repository)
    {
        _repository = repository;
    }

    public async Task ValidatePrerequisitesAsync(
        Guid studentId,
        List<Guid> sectionsToAdd,
        CancellationToken cancellationToken = default)
    {
        if (sectionsToAdd == null || !sectionsToAdd.Any())
            return;

        var hasUnmet = await _repository.HasUnmetPrerequisitesAsync(studentId, sectionsToAdd, cancellationToken);

        if (hasUnmet)
        {
            throw new InvalidOperationException("Registration denied: Unmet prerequisites for one or more selected courses.");
        }
    }

    public async Task ValidateCreditHoursAsync(
        Guid studentId,
        List<Guid> submittedSectionIds,
        CancellationToken cancellationToken = default)
    {
        if (submittedSectionIds == null || !submittedSectionIds.Any())
            return;

        // 1. Fetch raw data
        var settings = await _repository.GetUniversitySettingsAsync(cancellationToken);
        if (settings == null)
            throw new InvalidOperationException("University settings are not configured in the system.");

        var studentStatus = await _repository.GetStudentStatusAsync(studentId, cancellationToken);
        if (studentStatus == null)
            throw new KeyNotFoundException("Student data not found.");

        var totalCredits = await _repository.GetTotalCreditHoursAsync(submittedSectionIds, cancellationToken);

        // 2. Apply Business Logic for max allowed hours
        int maxAllowedHours = settings.MaxCreditHours;

        if (studentStatus.Value.HasAcademicWarning>0)
        {
            maxAllowedHours = settings.MaxCreditHoursForWarning;
        }
        else if (studentStatus.Value.IsGraduating)
        {
            maxAllowedHours = settings.MaxCreditHoursForGraduating;
        }

        // 3. Evaluate and throw standard English exceptions
        if (totalCredits < settings.MinCreditHours)
            throw new InvalidOperationException($"Registration denied: Minimum required credit hours is {settings.MinCreditHours}. Your requested total is {totalCredits}.");

        if (totalCredits > maxAllowedHours)
            throw new InvalidOperationException($"Registration denied: Maximum allowed credit hours is {maxAllowedHours}. Your requested total is {totalCredits}.");
    }
}