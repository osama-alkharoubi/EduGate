using FluentValidation;
using Application.DTOs.Grade;

namespace Application.Validations.Grade
{
    public class StudentGradeDtoValidator : AbstractValidator<StudentGradeDto>
    {
        public StudentGradeDtoValidator()
        {
            RuleFor(x => x.StudentId)
                .NotEmpty()
                .WithMessage("StudentId is required.");

            RuleFor(x => x.Grade)
                .InclusiveBetween(0m, 100m)
                .WithMessage("Grade must be between 0 and 100.");
        }
    }
}
