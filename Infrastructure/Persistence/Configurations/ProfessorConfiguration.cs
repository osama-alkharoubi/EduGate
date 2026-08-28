using EduGate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ProfessorConfiguration : IEntityTypeConfiguration<Professor>
{
    public void Configure(EntityTypeBuilder<Professor> builder)
    {
        builder.HasKey(p => p.ProfessorId);

        builder.Property(p => p.OfficeNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasOne(p => p.User)
            .WithOne(u => u.Professor)
            .HasForeignKey<Professor>(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
            
 
    }
}