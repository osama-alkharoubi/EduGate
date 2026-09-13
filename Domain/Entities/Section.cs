using Domain.Entities;
using global::Domain.Enums;

namespace EduGate.Domain.Entities;

public class Section : BaseAuditableEntity
{
    public Guid SectionId { get; set; }
    public Guid CourseId { get; set; }
    public Guid SemesterId { get; set; }
    public Guid ProfessorId { get; set; }
    public int SectionNumber { get; set; }
    public int Capacity { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string DaysOfWeek { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public enSectionStatus Status { get; set; } = enSectionStatus.Open;
    public Course Course { get; set; } = null!;
    public Semester Semester { get; set; } = null!;
    public Professor Professor { get; set; } = null!;
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}