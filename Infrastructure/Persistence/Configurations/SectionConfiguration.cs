using Domain.Enums;
using EduGate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        builder.HasKey(s => s.SectionId);

        builder.HasIndex(s => new { s.CourseId, s.SemesterId, s.SectionNumber })
            .IsUnique();

        builder.Property(s => s.RoomNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.DaysOfWeek)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(s => s.Course)
            .WithMany(c => c.Sections)
            .HasForeignKey(s => s.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Semester)
            .WithMany(sem => sem.Sections)
            .HasForeignKey(s => s.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Professor)
            .WithMany(p => p.Sections)
            .HasForeignKey(s => s.ProfessorId)
            .OnDelete(DeleteBehavior.Restrict);

        var validStatusValues = string.Join(", ", Enum.GetValues<enSectionStatus>().Cast<short>());

   
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Sections_SectionNumber_Positive", "\"SectionNumber\" > 0");
            t.HasCheckConstraint("CK_Sections_Capacity_Positive", "\"Capacity\" > 0");
            t.HasCheckConstraint("CK_Sections_EndTime_After_StartTime", "\"EndTime\" > \"StartTime\"");
            t.HasCheckConstraint(
             "CK_Sections_Status",
             $"\"Status\" IN ({validStatusValues})"
         );
        });
    }
}