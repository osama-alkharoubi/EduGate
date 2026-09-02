using Domain.Entities;

namespace EduGate.Domain.Entities;

public class User
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string HashPassword { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public Student? Student { get; set; }
    public Professor? Professor { get; set; }
}