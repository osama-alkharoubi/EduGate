using EduGate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SemesterConfiguration : IEntityTypeConfiguration<Semester>
{
    public void Configure(EntityTypeBuilder<Semester> builder)
    {
        builder.HasKey(s => s.SemesterId);

        builder.Property(s => s.SemesterName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.SemesterCode)
            .IsRequired()
            .HasMaxLength(20);
        builder.HasIndex(s => s.SemesterCode)
    .IsUnique();

    }
}