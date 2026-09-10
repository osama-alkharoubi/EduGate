using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Student
{
    public class StudentFilterDto
    {
        public Guid? SpecializationId { get; set; }
        public int? EnrollmentYear { get; set; }
        public string? AcademicStatus { get; set; }
        public decimal? MinGPA { get; set; }
        public decimal? MaxGPA { get; set; }
        public string? SearchTerm { get; set; } // للبحث بالاسم أو الرقم الجامعي
    }
}
