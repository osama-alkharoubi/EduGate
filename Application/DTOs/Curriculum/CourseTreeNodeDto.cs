using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Curriculum
{
    public class CourseTreeNodeDto
    {
        public Guid CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public Guid? ParentCourseId { get; set; }
        public int Depth { get; set; }
    }
}
