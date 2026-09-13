using EduGate.Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface ISemesterRepository
{
    Task<Semester?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task DeactivateAllExceptAsync(Guid excludedSemesterId, CancellationToken cancellationToken = default);
    void Add(Semester semester);

}