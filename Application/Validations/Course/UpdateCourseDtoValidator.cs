using Application.DTOs.Course;
using FluentValidation;

namespace Application.Validations.Course
{
    public class UpdateCourseDtoValidator : AbstractValidator<UpdateCourseDto>
    {
        public UpdateCourseDtoValidator()
        {
            RuleFor(x => x.CourseName)
                .NotEmpty().WithMessage("Course name is required.")
                .MaximumLength(100).WithMessage("Course name must not exceed 100 characters.");

            RuleFor(x => x.CreditHours)
         .InclusiveBetween(0, 6).WithMessage("Credit hours must be between 0 and 6.");

            RuleFor(x => x.DepartmentId)
                .NotEmpty().WithMessage("Department ID cannot be empty.");
        }
    }
}
