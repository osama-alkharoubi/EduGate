namespace EduGate.Domain.Entities;

public class College
{
    public Guid CollegeId { get; set; }
    public string CollegeName { get; set; } = string.Empty;
    public Guid? DeanId { get; set; }

    public Professor? Dean { get; set; }
    public ICollection<Department> Departments { get; set; } = new List<Department>();
}