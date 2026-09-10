using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Course
{
    public class UpdateCourseDto
    {
        public string CourseName { get; set; } = string.Empty;
        public int CreditHours { get; set; }
        public Guid DepartmentId { get; set; }
    }
}
