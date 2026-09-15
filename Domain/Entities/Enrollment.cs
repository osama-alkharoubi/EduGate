using Domain.Entities;
using Domain.Enums;

namespace EduGate.Domain.Entities;

public class Enrollment : BaseAuditableEntity
{
    public Guid EnrollmentId { get; set; }
    public Guid StudentId { get; set; }
    public Guid SectionId { get; set; }
    public enEnrollmentStatus Status { get; set; }
    public decimal? Grade { get; set; }

    public Student Student { get; set; } = null!;
    public Section Section { get; set; } = null!;

    public void SetGrade(decimal grade, decimal passingGrade = 50.0m)
    {
        if (grade < 0 || grade > 100)
            throw new ArgumentException("Grade must be between 0 and 100.");

        // 1. تعيين العلامة
        Grade = grade;

     
        Status = grade >= passingGrade ? enEnrollmentStatus.Completed : enEnrollmentStatus.Failed;
    }
}