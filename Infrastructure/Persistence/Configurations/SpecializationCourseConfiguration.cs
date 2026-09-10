using EduGate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SpecializationCourseConfiguration : IEntityTypeConfiguration<SpecializationCourse>
{
    public void Configure(EntityTypeBuilder<SpecializationCourse> builder)
    {
        builder.HasKey(sc => sc.PlanCourseId);

        builder.HasOne(sc => sc.Specialization)
            .WithMany(s => s.SpecializationCourses)
            .HasForeignKey(sc => sc.SpecializationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sc => sc.Course)
            .WithMany(c => c.SpecializationCourses)
            .HasForeignKey(sc => sc.CourseId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(sc => new { sc.SpecializationId, sc.CourseId })
            .IsUnique();

        builder.ToTable(t =>
        {
          
            t.HasCheckConstraint("CK_SpecializationCourses_SuggestedYear_Positive", "\"SuggestedYear\" > 0");
            t.HasCheckConstraint("CK_SpecializationCourses_SuggestedSemester_Range", "\"SuggestedSemester\" BETWEEN 1 AND 3");
        });

    }
}