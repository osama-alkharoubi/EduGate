using EduGate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduGate.Infrastructure.Persistence.Configurations;

public class CollegeConfiguration : IEntityTypeConfiguration<College>
{
    public void Configure(EntityTypeBuilder<College> builder)
    {
        // Primary Key
        builder.HasKey(c => c.CollegeId);

        // Properties
        builder.Property(c => c.CollegeName)
            .IsRequired()
            .HasMaxLength(100);

        // Relationships
        // 1-to-Many: College -> Department
        builder.HasMany(c => c.Departments)
            .WithOne(d => d.College)
            .HasForeignKey(d => d.CollegeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Optional Dean (Professor)
        builder.HasOne(c => c.Dean)
            .WithMany() // Assuming Professor entity does not have an ICollection<College> for Dean roles
            .HasForeignKey(c => c.DeanId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}