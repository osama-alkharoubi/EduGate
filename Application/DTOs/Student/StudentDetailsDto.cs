using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Student
{
    public class StudentDetailsDto
    {
        public Guid StudentId { get; set; }
        public string UniversityNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SpecializationName { get; set; } = string.Empty;
        public decimal GPA { get; set; }
        public int CompletedCredits { get; set; }
        public DateOnly EnrollmentDate { get; set; }
        public string AcademicStatus { get; set; } = string.Empty;
    }
}
