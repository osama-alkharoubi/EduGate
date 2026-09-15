using Application.DTOs.Semester;
using FluentValidation;

namespace Application.Validations.Semester;

public class CreateSemesterDtoValidator : AbstractValidator<CreateSemesterDto>
{
    public CreateSemesterDtoValidator()
    {
        RuleFor(x => x.SemesterName)
            .NotEmpty().WithMessage("Semester name is required.")
            .MaximumLength(100).WithMessage("Semester name must not exceed 100 characters.");

        RuleFor(x => x.SemesterCode)
            .NotEmpty().WithMessage("Semester code is required.")
            .MaximumLength(20).WithMessage("Semester code must not exceed 20 characters.")
            .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage("Semester code can only contain letters, numbers, hyphens, and underscores.");

        RuleFor(x => x.StartDate)
            .NotEqual(default(DateOnly)).WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .NotEqual(default(DateOnly)).WithMessage("End date is required.")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be later than start date.");
    }
}