using Application.Interfaces.Auth;
using Application.Interfaces.Repositories;
using System.Security.Claims;

namespace Infrastructure.Auth.Providers;

public class ProfessorClaimProvider : IRoleClaimProvider
{
    private readonly IProfessorRepository _professorRepository;

    public string TargetRole => "Professor";

    public ProfessorClaimProvider(IProfessorRepository professorRepository)
    {
        _professorRepository = professorRepository;
    }

    public async Task<Claim> GetRoleClaimAsync(Guid userId)
    {
        var professorId = await _professorRepository.GetProfessorIdByUserIdAsync(userId);

        if (professorId == null)
        {
            throw new InvalidOperationException($"No professor found for user ID '{userId}'.");
        }

        return new Claim("ProfessorId", professorId.Value.ToString());
    }
}