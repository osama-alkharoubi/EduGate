using EduGate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduGate.Infrastructure.Persistence.Configurations;

public class StudentSemesterConfiguration : IEntityTypeConfiguration<StudentSemester>
{
    public void Configure(EntityTypeBuilder<StudentSemester> builder)
    {
        builder.HasKey(ss => ss.StudentSemesterId);

        // أهم سطر: منع التكرار. الطالب له سجل واحد فقط لكل فصل
        builder.HasIndex(ss => new { ss.StudentId, ss.SemesterId }).IsUnique();

        // تحديد دقة الأرقام العشرية للمعدلات
        builder.Property(ss => ss.SemesterGPA).HasColumnType("numeric(5,2)");
        builder.Property(ss => ss.CumulativeGPA).HasColumnType("numeric(5,2)");
        builder.Property(ss => ss.WarningIssued).HasDefaultValue(false).IsRequired();

        // العلاقات (Foreign Keys)
        builder.HasOne(ss => ss.Student)
            .WithMany(s => s.StudentSemesters)
            .HasForeignKey(ss => ss.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ss => ss.Semester)
            .WithMany(s => s.StudentSemesters)
            .HasForeignKey(ss => ss.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}