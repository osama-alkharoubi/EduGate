using Application.DTOs.Semester;
using Application.Exceptions;
using Application.Interfaces.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using EduGate.Domain.Entities;

namespace Application.Services;

public class SemesterService : ISemesterService
{
    private readonly ISemesterRepository _semesterRepository;
    private readonly IUnitOfWork _unitOfWork;
    public SemesterService(ISemesterRepository semesterRepository, IUnitOfWork unitOfWork)
    {
        _semesterRepository = semesterRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateSemesterAsync(CreateSemesterDto dto, CancellationToken cancellationToken)
    {
        var semester = new Semester
        {
            SemesterId = Guid.NewGuid(),
            SemesterName = dto.SemesterName.Trim(),
            SemesterCode = dto.SemesterCode.Trim().ToUpperInvariant(),
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsActive = false
        };

         _semesterRepository.Add(semester);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return semester.SemesterId;
    }

    public async Task ActivateSemesterAsync(Guid semesterId, CancellationToken cancellationToken)
    {
        var targetSemester = await _semesterRepository.GetByIdAsync(semesterId, cancellationToken);

        if (targetSemester == null)
        {
            throw new KeyNotFoundException($"Semester with ID '{semesterId}' was not found.");
        }
        if(targetSemester.IsActive)
        {
            throw new ConflictException($"Semester with ID '{semesterId}' is already active.");
        }  
        await _semesterRepository.DeactivateAllExceptAsync(semesterId, cancellationToken);

       

        targetSemester.IsActive = true;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}