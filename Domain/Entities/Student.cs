using Domain.Enums;

namespace EduGate.Domain.Entities;

public class Student
{
    public Guid StudentId { get; set; }
    public string UniversityNumber { get; set; } = string.Empty;
    public decimal GPA { get; set; }
    public enAcademicStatus AcademicStatus { get; set; }
    public DateOnly EnrollmentDate { get; set; }
    public int CompletedCredits { get; set; }
    public Guid UserId { get; set; }
    public Guid SpecializationId { get; set; }

    public User User { get; set; } = null!;
    public Specialization Specialization { get; set; } = null!;
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}