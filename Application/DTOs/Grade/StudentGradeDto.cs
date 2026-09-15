using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Grade
{
    public class StudentGradeDto
    {
        public Guid StudentId { get; set; }
        public decimal Grade { get; set; } = 0;
    }
}
