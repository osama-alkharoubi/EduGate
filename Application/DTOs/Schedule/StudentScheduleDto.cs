namespace Application.DTOs.Schedule;

public record StudentScheduleDto(
    Guid SectionId,
    string CourseCode,
    string CourseName,
    int CreditHours,
    int SectionNumber,
    string DaysOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string RoomNumber,
    string ProfessorName);
