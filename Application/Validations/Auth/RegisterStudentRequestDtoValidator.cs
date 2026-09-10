using Application.DTOs.Auth;
using FluentValidation;
using FluentValidation.Validators;
using System;

namespace Application.Validations.Auth
{
    public class RegisterStudentRequestDtoValidator : AbstractValidator<RegisterStudentRequestDto>
    {
        public RegisterStudentRequestDtoValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username is required.")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters long.")
                .MaximumLength(50).WithMessage("Username must not exceed 50 characters.")
                .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores.");

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

            RuleFor(x => x.Email)
        .NotEmpty().WithMessage("Email is required.")
        .MaximumLength(255).WithMessage("Email must not exceed 255 characters.")
        .EmailAddress(EmailValidationMode.AspNetCoreCompatible)
        .WithMessage("Email must be a valid email address.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .MaximumLength(100).WithMessage("Password must not exceed 100 characters.")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)")
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, and one number.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
                .Matches(@"^\+?[0-9\s\-()]*$").WithMessage("Phone number contains invalid characters.");
      

            RuleFor(x => x.SpecializationId)
                .NotEmpty().WithMessage("Specialization ID is required.")
                .Must(id => id != Guid.Empty).WithMessage("Specialization ID cannot be empty.");
        }
    }
}