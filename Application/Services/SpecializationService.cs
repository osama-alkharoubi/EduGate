using Application.DTOs.Curriculum;
using Application.Exceptions;
using Application.Interfaces.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using EduGate.Domain.Entities;

namespace Application.Services;

public class SpecializationService : ISpecializationService
{
    private readonly ISpecializationRepository _specializationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SpecializationService(
        ISpecializationRepository specializationRepository,
        IUnitOfWork unitOfWork)
    {
        _specializationRepository = specializationRepository;
        _unitOfWork = unitOfWork;
    }

    // ==========================================
    // 1. Queries (الاستعلامات)
    // ==========================================

    public async Task<SpecializationResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var spec = await _specializationRepository.GetSpecializationByIdAsync(id, cancellationToken);
        if (spec == null) return null;

        return new SpecializationResponseDto
        {
            SpecializationId = spec.SpecializationId,
            SpecializationName = spec.SpecializationName,
            TotalCredits = (byte)spec.TotalCredits,
            DepartmentId = spec.DepartmentId,
            IsActive = spec.IsActive
        };
    }

    public async Task<IEnumerable<SpecializationResponseDto>> GetAllAsync(Guid? departmentId = null, Guid? collegeId = null, CancellationToken cancellationToken = default)
    {
        var specializations = await _specializationRepository.GetAllSpecializationsAsync(departmentId, collegeId, cancellationToken);

        return specializations.Select(spec => new SpecializationResponseDto
        {
            SpecializationId = spec.SpecializationId,
            SpecializationName = spec.SpecializationName,
            TotalCredits = (byte)spec.TotalCredits,
            DepartmentId = spec.DepartmentId,
            IsActive = spec.IsActive
        });
    }

    public async Task<StudentStudyPlanDto?> GetStudentStudyPlanAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _specializationRepository.GetStudentStudyPlanAsync(studentId, cancellationToken);
    }

    public async Task<IEnumerable<CourseTreeNodeDto>> GetPrerequisiteTreeAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _specializationRepository.GetPrerequisiteTreeAsync(courseId, cancellationToken);
    }

    // ==========================================
    // 2. Commands (العمليات والتعديلات)
    // ==========================================

    public async Task<SpecializationResponseDto> CreateSpecializationAsync(CreateSpecializationDto dto, CancellationToken cancellationToken = default)
    {
        var specialization = new Specialization
        {
            SpecializationId = Guid.NewGuid(),
            SpecializationName = dto.SpecializationName,
            TotalCredits = dto.TotalCredits,
            DepartmentId = dto.DepartmentId,
            Code = dto.Code,
            IsActive = true // مفعل افتراضياً عند الإنشاء
        };
        if(await _specializationRepository.ExistsByCodeAsync(dto.Code, cancellationToken))
        {
            throw new ConflictException($"Specialization with code '{dto.Code}' already exists.");
        }
        _specializationRepository.AddSpecialization(specialization);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SpecializationResponseDto
        {
            SpecializationId = specialization.SpecializationId,
            SpecializationName = specialization.SpecializationName,
            TotalCredits = (byte)specialization.TotalCredits,
            DepartmentId = specialization.DepartmentId,
            Code = specialization.Code,
            IsActive = specialization.IsActive
        };
    }

    public async Task UpdateSpecializationAsync(Guid id, UpdateSpecializationDto dto, CancellationToken cancellationToken = default)
    {
        var spec = await _specializationRepository.GetSpecializationByIdAsync(id, cancellationToken);
        if (spec == null)
            throw new KeyNotFoundException($"Specialization with ID '{id}' was not found.");
        
        if(await _specializationRepository.ExistsByCodeExcludeIdAsync(dto.Code, id, cancellationToken))
        {
            throw new ConflictException($"Specialization with code '{dto.Code}' already exists.");
        }

        spec.SpecializationName = dto.SpecializationName;
        spec.TotalCredits = dto.TotalCredits;
        spec.DepartmentId = dto.DepartmentId;
        spec.IsActive = dto.IsActive;
        spec.Code = dto.Code;


        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateSpecializationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var spec = await _specializationRepository.GetSpecializationByIdAsync(id, cancellationToken);
        if (spec == null)
            throw new KeyNotFoundException($"Specialization with ID '{id}' was not found.");

        spec.IsActive = false;

  
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}