namespace EduGate.Domain.Entities;

public class Specialization
{
    public Guid SpecializationId { get; set; }
    public string SpecializationName { get; set; } = string.Empty;
    public int TotalCredits { get; set; }
    public Guid DepartmentId { get; set; }
    public bool IsActive { get; set; } = true;
    public int Code { get; set; }
    public Department Department { get; set; } = null!;
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<SpecializationCourse> SpecializationCourses { get; set; } = new List<SpecializationCourse>();
}