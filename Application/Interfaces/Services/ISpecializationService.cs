using Application.DTOs.Curriculum;
using Domain.Entities;

namespace Application.Interfaces.Services;

public interface ISpecializationService
{
    // Queries
    Task<SpecializationResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<SpecializationResponseDto>> GetAllAsync(Guid? departmentId = null, Guid? collegeId = null, CancellationToken cancellationToken = default);
    Task<StudentStudyPlanDto?> GetStudentStudyPlanAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CourseTreeNodeDto>> GetPrerequisiteTreeAsync(Guid courseId, CancellationToken cancellationToken = default);

    // Commands
    Task<SpecializationResponseDto> CreateSpecializationAsync(CreateSpecializationDto dto, CancellationToken cancellationToken = default);
    Task UpdateSpecializationAsync(Guid id, UpdateSpecializationDto dto, CancellationToken cancellationToken = default);
    Task DeactivateSpecializationAsync(Guid id, CancellationToken cancellationToken = default); // Soft Delete
}