using Domain.Entities;
using Domain.Enums;

// تأكد من الـ namespace الخاص بـ BaseAuditableEntity

namespace EduGate.Domain.Entities;

public class StudentSemester : BaseAuditableEntity
{
    public Guid StudentSemesterId { get; set; }

    public Guid StudentId { get; set; }
    public Guid SemesterId { get; set; }

    // إحصائيات الفصل الحالي (فقط المواد اللي سجلها بهاد الفصل)
    public int SemesterCompletedCredits { get; set; }
    public decimal SemesterGPA { get; set; }

    // الإحصائيات التراكمية (الحصيلة النهائية لحد نهاية هاد الفصل)
    public int TotalCompletedCredits { get; set; }
    public decimal CumulativeGPA { get; set; }


    public enAcademicStatus AcademicStatus { get; set; }
    public bool WarningIssued { get; set; }  = false;

    // Navigation Properties
    public Student Student { get; set; } = null!;
    public Semester Semester { get; set; } = null!;
}