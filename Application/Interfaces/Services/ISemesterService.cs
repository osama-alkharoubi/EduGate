using Application.DTOs.Semester;

namespace Application.Interfaces.Services;

public interface ISemesterService
{
    Task<Guid> CreateSemesterAsync(CreateSemesterDto dto, CancellationToken cancellationToken);
    Task ActivateSemesterAsync(Guid semesterId, CancellationToken cancellationToken);
}