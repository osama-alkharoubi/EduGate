using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Section
{
    public class AvailableSectionDto
    {
        public Guid SectionId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int SectionNumber { get; set; }
        public string ProfessorName { get; set; } = string.Empty;
        public string DaysOfWeek { get; set; } = string.Empty;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int Capacity { get; set; }
        public int EnrolledCount { get; set; }
    }
}
