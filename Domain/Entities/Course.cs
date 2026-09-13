using Domain.Entities;

namespace EduGate.Domain.Entities;

public class Course : BaseAuditableEntity
{
    public Guid CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public int CreditHours { get; set; }
    public Guid DepartmentId { get; set; }
    public bool IsActive { get; set; } = true;
    public Department Department { get; set; } = null!;
    public ICollection<Section> Sections { get; set; } = new List<Section>();
    public ICollection<SpecializationCourse> SpecializationCourses { get; set; } = new List<SpecializationCourse>();
    public ICollection<CoursePrerequisite> Prerequisites { get; set; } = new List<CoursePrerequisite>();
    public ICollection<CoursePrerequisite> PrerequisiteFor { get; set; } = new List<CoursePrerequisite>();
}