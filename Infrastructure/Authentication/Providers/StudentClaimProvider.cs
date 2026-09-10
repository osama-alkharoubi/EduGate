using Application.Interfaces.Auth;
using Application.Interfaces.Repositories; // مسار الـ IStudentRepository عندك
using System.Security.Claims;

namespace Infrastructure.Auth.Providers;

public class StudentClaimProvider : IRoleClaimProvider
{
    private readonly IStudentRepository _studentRepository;

    public string TargetRole => "Student";

    public StudentClaimProvider(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Claim> GetRoleClaimAsync(Guid userId)
    {
  
        var studentId = await _studentRepository.GetStudentIdByUserIdAsync(userId);
        if (studentId == null) { 
        throw new InvalidOperationException($"No student found for user ID '{userId}'.");
        }
        return new Claim("StudentId", studentId.Value.ToString());
    }
}