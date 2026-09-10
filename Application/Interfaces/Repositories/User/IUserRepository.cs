

using Application.DTOs.Auth;
using EduGate.Domain.Entities;
namespace Application.Interfaces.Repositories.User;

public interface IUserRepository
{
    Task  <EduGate.Domain.Entities.User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EduGate.Domain.Entities.User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    void Add(EduGate.Domain.Entities.User user);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UserAuthDto?> GetForAuthByEmailAsync(string email, CancellationToken cancellationToken = default);

}