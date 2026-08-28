namespace EduGate.Domain.Entities;

public class Department
{
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public Guid CollegeId { get; set; }
    public Guid? HeadOfDepartmentId { get; set; }

    public College College { get; set; } = null!;
    public Professor? HeadOfDepartment { get; set; }
    public ICollection<Specialization> Specializations { get; set; } = new List<Specialization>();
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}