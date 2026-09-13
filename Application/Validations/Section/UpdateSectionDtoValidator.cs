using Application.DTOs.Section;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Validations.Section
{
    public class UpdateSectionDtoValidator : AbstractValidator<UpdateSectionDto>
    {
        public UpdateSectionDtoValidator()
        {
            RuleFor(x => x.SectionId).NotEmpty();
            RuleFor(x => x.CourseId).NotEmpty();
            RuleFor(x => x.SemesterId).NotEmpty();
            RuleFor(x => x.ProfessorId).NotEmpty();
            RuleFor(x => x.SectionNumber).GreaterThan(0);
            RuleFor(x => x.Capacity).GreaterThan(0);
            RuleFor(x => x.RoomNumber).NotEmpty().MaximumLength(50);
            RuleFor(x => x.DaysOfWeek)
          .NotEmpty().WithMessage("DaysOfWeek is required.")
          .Must(BeAValidDaysPattern)
          .WithMessage("Invalid DaysOfWeek format. Use comma-separated standard days (e.g., 'Sun,Tue,Thu') with no duplicates.");
            RuleFor(x => x.StartTime).LessThan(x => x.EndTime)
                .WithMessage("StartTime must be earlier than EndTime.");
            RuleFor(x => x.Status).IsInEnum();
        }
        private static readonly HashSet<string> ValidDays = new(StringComparer.OrdinalIgnoreCase)
{
    "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"
};

        private bool BeAValidDaysPattern(string daysOfWeek)
        {
            if (string.IsNullOrWhiteSpace(daysOfWeek))
                return false;

            var days = daysOfWeek.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (days.Length == 0)
                return false;

            // التأكد من عدم تكرار نفس اليوم في السلسلة
            var uniqueDays = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var day in days)
            {
                
                if (!ValidDays.Contains(day))
                    return false;

  
                if (!uniqueDays.Add(day))
                    return false;
            }

            return true;
        }
    }
}
