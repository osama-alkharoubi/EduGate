using EduGate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.StudentId);

        builder.Property(s => s.UniversityNumber)
            .IsRequired()
            .HasMaxLength(20);
        builder.HasIndex(s => s.UniversityNumber).IsUnique();
        builder.Property(s => s.GPA)
            .HasPrecision(5, 2);

        builder.HasOne(s => s.User)
            .WithOne(u => u.Student)
            .HasForeignKey<Student>(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Specialization)
            .WithMany(sp => sp.Students)
            .HasForeignKey(s => s.SpecializationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Students_GPA_Range", "\"GPA\" BETWEEN 0 AND 100");
            t.HasCheckConstraint("CK_Students_AcademicStatus_Valid", "\"AcademicStatus\" BETWEEN 1 AND 4");
            t.HasCheckConstraint("CK_Students_CompletedCredits_NonNegative", "\"CompletedCredits\" >= 0");
        });
    }
}