using EduGate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CoursePrerequisiteConfiguration : IEntityTypeConfiguration<CoursePrerequisite>
{
    public void Configure(EntityTypeBuilder<CoursePrerequisite> builder)
    {
        builder.HasKey(cp => new { cp.CourseId, cp.PrerequisiteId });

        builder.HasOne(cp => cp.Course)
            .WithMany(c => c.Prerequisites)
            .HasForeignKey(cp => cp.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cp => cp.PrerequisiteCourse)
            .WithMany(c => c.PrerequisiteFor)
            .HasForeignKey(cp => cp.PrerequisiteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_CoursePrerequisites_DifferentCourses",
            "\"CourseId\" <> \"PrerequisiteId\""));
    }
}