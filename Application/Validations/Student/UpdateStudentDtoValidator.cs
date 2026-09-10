using Application.DTOs.Student;
using FluentValidation;
using System;

namespace Application.Validations.Student
{
    public class UpdateStudentDtoValidator : AbstractValidator<UpdateStudentDto>
    {
        public UpdateStudentDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100).WithMessage("First name must not exceed 100 characters.")
                .Matches(@"^[a-zA-Z\u0600-\u06FF\s\-]+$")
                .WithMessage("First name can only contain Arabic or English letters, spaces, and hyphens.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.")
                .Matches(@"^[a-zA-Z\u0600-\u06FF\s\-]+$")
                .WithMessage("Last name can only contain Arabic or English letters, spaces, and hyphens.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
                .Matches(@"^\+?[0-9\s\-()]*$")
                .WithMessage("Phone number must contain only digits, spaces, hyphens, parentheses, and optional leading +.")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.SpecializationId)
                .NotEmpty().WithMessage("Specialization ID is required.")
                .Must(id => id != Guid.Empty).WithMessage("Specialization ID cannot be empty.");
        }
    }
}