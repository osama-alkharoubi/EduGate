using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Schedule
{
    public record ProfessorScheduleDto(
        Guid SectionId,
        string CourseCode,
        string CourseName,
        int CreditHours,
        int SectionNumber,
        string DaysOfWeek,
        TimeOnly StartTime,
    TimeOnly EndTime,
        string RoomNumber,
        int Capacity,
        int EnrolledCount
    );
}
