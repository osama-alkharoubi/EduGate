using EduGate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace Infrastructure.Persistence.Configurations;

public class SpecializationConfiguration : IEntityTypeConfiguration<Specialization>
{
    public void Configure(EntityTypeBuilder<Specialization> builder)
    {
        builder.HasKey(s => s.SpecializationId);

        builder.Property(s => s.SpecializationName)
            .IsRequired()
            .HasMaxLength(100);
        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasIndex(x => new { x.DepartmentId, x.SpecializationName })
            .IsUnique();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Specializations_TotalCredits_Range", "\"TotalCredits\" BETWEEN 60 AND 250");
            t.HasCheckConstraint("CK_Specializations_Code_Range", "\"Code\" BETWEEN 1 AND 999");
        });

        builder.HasOne(s => s.Department)
            .WithMany(d => d.Specializations)
            .HasForeignKey(s => s.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(s => s.IsActive)
    .HasDefaultValue(true);

    }
}