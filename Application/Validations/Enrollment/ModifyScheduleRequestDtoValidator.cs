using Application.DTOs.Enrollment;
using FluentValidation;

namespace Application.Validations.Enrollment;

public class ModifyScheduleRequestDtoValidator : AbstractValidator<ModifyScheduleRequestDto>
{
    public ModifyScheduleRequestDtoValidator()
    {
        RuleFor(x => x)
            .Must(x => (x.SectionsToAdd?.Any() ?? false) || (x.SectionsToDrop?.Any() ?? false))
            .WithMessage("At least one section must be added or dropped.");

        RuleForEach(x => x.SectionsToAdd)
            .NotEmpty().WithMessage("SectionsToAdd cannot contain empty IDs.");

        RuleForEach(x => x.SectionsToDrop)
            .NotEmpty().WithMessage("SectionsToDrop cannot contain empty IDs.");

        RuleFor(x => x)
            .Must(x => x.SectionsToAdd == null || x.SectionsToAdd.Distinct().Count() == x.SectionsToAdd.Count)
            .WithMessage("SectionsToAdd cannot contain duplicate section IDs.");

        RuleFor(x => x)
            .Must(x => x.SectionsToDrop == null || x.SectionsToDrop.Distinct().Count() == x.SectionsToDrop.Count)
            .WithMessage("SectionsToDrop cannot contain duplicate section IDs.");

        RuleFor(x => x)
            .Must(x => x.SectionsToAdd == null || x.SectionsToDrop == null || !x.SectionsToAdd.Intersect(x.SectionsToDrop).Any())
            .WithMessage("The same section cannot be added and dropped in the same request.");
    }
}