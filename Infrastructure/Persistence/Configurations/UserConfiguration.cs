using EduGate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.UserId);

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasQueryFilter(u => u.IsActive);
        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.HashPassword)
            .IsRequired()
            .HasMaxLength(500);

        builder
    .HasIndex(u => u.Email)
    .IsUnique();
        builder.HasIndex(u => u.UserName);
        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired(false);
    }
}