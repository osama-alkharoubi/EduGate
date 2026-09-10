using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Course
{
    public class CourseDto
    {
        public Guid CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int CreditHours { get; set; }
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<PrerequisiteDto>? Prerequisites { get; set; }
    }
    public class PrerequisiteDto
    {
        public Guid PrerequisiteId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
    }
}






                                                                                         
