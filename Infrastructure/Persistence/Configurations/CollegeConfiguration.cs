using EduGate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CollegeConfiguration : IEntityTypeConfiguration<College>
{
    public void Configure(EntityTypeBuilder<College> builder)
    {
        builder.HasKey(c => c.CollegeId);

        builder.Property(c => c.CollegeName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(c => c.CollegeName)
            .IsUnique();

        builder.HasOne(c => c.Dean)
            .WithMany()
            .HasForeignKey(c => c.DeanId)
            .IsRequired(false);

   
    }
}