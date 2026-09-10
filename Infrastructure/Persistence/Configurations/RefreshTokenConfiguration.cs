using Domain.Entities;
using EduGate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration: IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(rt => rt.Id);
            builder.Property(rt => rt.Token)
                .IsRequired()
                .HasMaxLength(200);
            builder.HasIndex(rt => rt.Token)
                .IsUnique();
            builder.Property(rt => rt.ExpiresOnUtc)
                .IsRequired();
            builder.Property(rt => rt.IsRevoked)
                .IsRequired();
            builder.Property(rt => rt.IsRevoked)
                .HasDefaultValue(false);
            builder.Property(rt => rt.CreatedOnUtc)
                .IsRequired();
            builder.Property(rt => rt.CreatedOnUtc)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_RefreshTokens_ExpiresAfterCreated",
                "\"ExpiresOnUtc\" > \"CreatedOnUtc\""));
        }
    }
}
