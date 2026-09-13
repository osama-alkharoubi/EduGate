using Application.DTOs.Section;
using FluentValidation;

namespace EduGate.Application.Validators;

public class CreateSectionDtoValidator : AbstractValidator<CreateSectionDto>
{
    public CreateSectionDtoValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("CourseId is required.");

        RuleFor(x => x.SemesterId)
            .NotEmpty().WithMessage("SemesterId is required.");

        RuleFor(x => x.ProfessorId)
            .NotEmpty().WithMessage("ProfessorId is required.");

        RuleFor(x => x.SectionNumber)
            .GreaterThan(0).WithMessage("SectionNumber must be greater than zero.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than zero.");

        RuleFor(x => x.RoomNumber)
            .NotEmpty().WithMessage("RoomNumber is required.")
            .MaximumLength(50).WithMessage("RoomNumber must not exceed 50 characters.");

        RuleFor(x => x.DaysOfWeek)
            .NotEmpty().WithMessage("DaysOfWeek is required.")
            .MaximumLength(50).WithMessage("DaysOfWeek must not exceed 50 characters.");

        RuleFor(x => x.StartTime)
            .LessThan(x => x.EndTime)
            .WithMessage("StartTime must be earlier than EndTime.");
    }
}