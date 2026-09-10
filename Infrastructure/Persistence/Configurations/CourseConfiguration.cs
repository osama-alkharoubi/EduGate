using EduGate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(c => c.CourseId);

        builder.Property(c => c.CourseCode)
            .IsRequired()
            .HasMaxLength(20);
        builder.HasIndex(c => c.CourseCode)
    .IsUnique();
        builder.Property(c => c.CourseName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);

        builder.HasOne(c => c.Department)
            .WithMany(d => d.Courses)
            .HasForeignKey(c => c.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Courses_CreditHours_Positive",
            "\"CreditHours\" > 0 AND \"CreditHours\" <= 12"));
    }
}