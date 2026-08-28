using Domain.Enums;

namespace EduGate.Domain.Entities;

public class SpecializationCourse
{
    public Guid PlanCourseId { get; set; }
    public Guid SpecializationId { get; set; }
    public Guid CourseId { get; set; }
    public enRequirementType RequirementType { get; set; }
    public byte SuggestedYear { get; set; }
    public byte SuggestedSemester { get; set; }

    public Specialization Specialization { get; set; } = null!;
    public Course Course { get; set; } = null!;
}