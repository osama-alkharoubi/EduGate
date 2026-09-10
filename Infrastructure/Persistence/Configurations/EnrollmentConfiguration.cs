using EduGate.Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.HasKey(e => e.EnrollmentId);

        builder.Property(e => e.Grade)
            .HasPrecision(5, 2)
            .IsRequired(false);

        builder.HasIndex(e => new { e.StudentId, e.SectionId })
            .IsUnique();

        builder.Property(e => e.Status)
            .HasDefaultValue(enEnrollmentStatus.Enrolled);

        builder.HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Section)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Enrollments_Grade_Range", "\"Grade\" IS NULL OR (\"Grade\" >= 0 AND \"Grade\" <= 100)");
            t.HasCheckConstraint("CK_Enrollments_Status_Valid", "\"Status\" BETWEEN 1 AND 5");
        });
    }
}