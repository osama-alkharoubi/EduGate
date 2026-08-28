namespace EduGate.Domain.Entities;

public class CoursePrerequisite
{
    public Guid CourseId { get; set; }
    public Guid PrerequisiteId { get; set; }

    public Course Course { get; set; } = null!;
    public Course PrerequisiteCourse { get; set; } = null!;
}