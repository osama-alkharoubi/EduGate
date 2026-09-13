using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Section
{
    public class UpdateSectionDto
    {
        public Guid SectionId { get; set; }
        public Guid CourseId { get; set; }
        public Guid SemesterId { get; set; }
        public Guid ProfessorId { get; set; }
        public int SectionNumber { get; set; }
        public int Capacity { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string DaysOfWeek { get; set; } = string.Empty;
        public enSectionStatus Status { get; set; } 
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
