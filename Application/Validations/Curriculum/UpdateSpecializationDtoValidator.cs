using Application.DTOs.Curriculum;
using FluentValidation;

namespace Application.Validations.Curriculum
{
    public class UpdateSpecializationDtoValidator : AbstractValidator<UpdateSpecializationDto>
    {
        public UpdateSpecializationDtoValidator()
        {
            RuleFor(x => x.SpecializationName)
                .NotEmpty().WithMessage("Specialization name is required.")
              
                .MaximumLength(100).WithMessage("Specialization name must not exceed 100 characters.");


            RuleFor(x => x.TotalCredits)
                .InclusiveBetween((byte)60, (byte)250)
    .WithMessage("Total credits must be between 60 and 250 credit hours.");

            RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Specialization code is required.")
            .GreaterThan(0).WithMessage("Specialization code must be greater than zero.")
            .LessThanOrEqualTo(999).WithMessage("Specialization code cannot exceed 3 digits (1-999).");

            RuleFor(x => x.DepartmentId)
                .NotEmpty().WithMessage("Department ID cannot be empty.");
        }
    }
}
