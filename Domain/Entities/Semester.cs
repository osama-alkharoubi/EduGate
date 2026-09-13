using Domain.Entities;

namespace EduGate.Domain.Entities;

public class Semester : BaseAuditableEntity
{
    public Guid SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public string SemesterCode { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; }

    public ICollection<Section> Sections { get; set; } = new List<Section>();
}