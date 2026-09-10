using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Student
{
    public class StudentListDto
    {
        public string UniversityNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public decimal GPA { get; set; }
        public string AcademicStatus { get; set; } = string.Empty;
    }
}
