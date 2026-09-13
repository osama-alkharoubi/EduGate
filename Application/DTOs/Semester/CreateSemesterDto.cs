namespace Application.DTOs.Semester;

public class CreateSemesterDto
{
    public string SemesterName { get; set; } = string.Empty;
    public string SemesterCode { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}