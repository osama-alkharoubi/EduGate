using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Student
{
    public class UpdateStudentDto
    {
        // 1. التخصص: لتطبيق معاملة تحويل التخصص للطالب
        public Guid SpecializationId { get; set; }

        // 2. البيانات الشخصية (التابعة لجدول User): لتصحيح الأخطاء الإملائية الرسمية
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        // 3. وسيلة التواصل
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
