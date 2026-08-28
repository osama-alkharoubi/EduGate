using Domain.Enums;

namespace EduGate.Domain.Entities;

public class Professor
{
    public Guid ProfessorId { get; set; }
    public DateOnly HireDate { get; set; }
    public enAcademicRank AcademicRank { get; set; }
    public string OfficeNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;
    public ICollection<Section> Sections { get; set; } = new List<Section>();
}