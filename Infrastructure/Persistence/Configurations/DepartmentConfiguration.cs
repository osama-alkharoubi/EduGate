using EduGate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(d => d.DepartmentId);

        builder.Property(d => d.DepartmentName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(d => new { d.CollegeId, d.DepartmentName })
            .IsUnique();

        builder.HasOne(d => d.College)
            .WithMany(c => c.Departments)
            .HasForeignKey(d => d.CollegeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.HeadOfDepartment)
     .WithMany()
     .HasForeignKey(d => d.HeadOfDepartmentId)
     .IsRequired(false)
     .OnDelete(DeleteBehavior.SetNull);


    }
}