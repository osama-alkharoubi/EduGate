using Application.DTOs.Schedule;
using FluentValidation;

namespace Application.Validations.Enrollment;

public class GetScheduleBySemesterRequestValidator : AbstractValidator<GetScheduleBySemesterRequest>
{
    public GetScheduleBySemesterRequestValidator()
    {
        RuleFor(x => x.SemesterId)
            .NotEmpty().WithMessage("SemesterId is required.")
            .NotEqual(Guid.Empty).WithMessage("SemesterId must not be an empty GUID.");
    }
}
