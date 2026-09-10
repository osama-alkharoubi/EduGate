using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Curriculum
{
    public class UpdateSpecializationDto
    {
        public string SpecializationName { get; set; } = string.Empty;
        public byte TotalCredits { get; set; }
        public Guid DepartmentId { get; set; }
        public bool IsActive { get; set; }
        public int Code { get; set; }

    }
}
