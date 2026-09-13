namespace Domain.Entities
{
    public class UniversitySetting : BaseAuditableEntity
    {
        // دائماً سيكون هناك سجل واحد فقط الـ Id الخاص به = 1
        public int Id { get; set; }

        // إعدادات الساعات
        public int MinCreditHours { get; set; }
        public int MaxCreditHours { get; set; }
        public int MaxCreditHoursForWarning { get; set; } // للمنذر أكاديمياً
        public int MaxCreditHoursForGraduating { get; set; } // للخريج
    }
}
