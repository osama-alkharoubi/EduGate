using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Curriculum
{
    public class StudentStudyPlanDto
    {
        public string StudentName { get; set; } = string.Empty;
        public string StudentIdNumber { get; set; } = string.Empty;
        public string SpecializationName { get; set; } = string.Empty;
        public int PlanTotalHours { get; set; }
        public int CompletedHours { get; set; }

        // تصنيفات المواد كما في الشاشة
        public List<StudyPlanCategoryDto> Categories { get; set; } = new();
    }

    public class StudyPlanCategoryDto
    {
        public string CategoryName { get; set; } = string.Empty; // متطلبات التخصص الإجبارية، الكلية، ...
        public int RequiredHours { get; set; }
        public List<StudyPlanCourseItemDto> Courses { get; set; } = new();
    }

    public class StudyPlanCourseItemDto
    {
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public byte CreditHours { get; set; }
        public List<string> Prerequisites { get; set; } = new();

        // بيانات سجل الطالب
        public decimal? Grade { get; set; }
        public string Status { get; set; } = "NotTaken"; // Passed, Failed, InProgress, NotTaken
    }
}
