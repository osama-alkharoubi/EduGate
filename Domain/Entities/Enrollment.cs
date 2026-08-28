using Domain.Enums;

namespace EduGate.Domain.Entities;

public class Enrollment
{
    public Guid EnrollmentId { get; set; }
    public Guid StudentId { get; set; }
    public Guid SectionId { get; set; }
    public enEnrollmentStatus Status { get; set; }
    public decimal? Grade { get; set; }

    public Student Student { get; set; } = null!;
    public Section Section { get; set; } = null!;
}