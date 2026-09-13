using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Enrollment
{
    public class ModifyScheduleRequestDto
    {
        public List<Guid> SectionsToAdd { get; set; } = new();
        public List<Guid> SectionsToDrop { get; set; } = new();
    }
}
