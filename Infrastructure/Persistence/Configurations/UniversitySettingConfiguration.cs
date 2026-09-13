using Domain.Entities;
using EduGate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduGate.Infrastructure.Persistence.Configurations;

public class UniversitySettingConfiguration : IEntityTypeConfiguration<UniversitySetting>
{
    public void Configure(EntityTypeBuilder<UniversitySetting> builder)
    {
        builder.HasKey(e => e.Id);

        // وضع قيم افتراضية لضمان عدم وجود Null
        builder.Property(e => e.MinCreditHours).HasDefaultValue(9);
        builder.Property(e => e.MaxCreditHours).HasDefaultValue(18);
        builder.Property(e => e.MaxCreditHoursForWarning).HasDefaultValue(12);
        builder.Property(e => e.MaxCreditHoursForGraduating).HasDefaultValue(21);

       
        builder.HasData(new UniversitySetting
        {
            Id = 1,
            MinCreditHours = 9,
            MaxCreditHours = 18,
            MaxCreditHoursForWarning = 12,
            MaxCreditHoursForGraduating = 21
        });
    }
}