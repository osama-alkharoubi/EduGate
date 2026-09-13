using Domain.Entities;

namespace EduGate.Domain.Entities;

public class Role : BaseAuditableEntity
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}