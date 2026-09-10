using Application.DTOs.Student;
using Domain.Enums;
using FluentValidation;
using System;

namespace Application.Validations.Student
{
    public class StudentFilterDtoValidator : AbstractValidator<StudentFilterDto>
    {
        public StudentFilterDtoValidator()
        {
            RuleFor(x => x.SpecializationId)
                .Must(id => id == null || id != Guid.Empty)
                .WithMessage("Specialization ID must be a valid GUID if provided.");

            RuleFor(x => x.EnrollmentYear)
                .GreaterThanOrEqualTo(1900).WithMessage("Enrollment year must be at least 1900.")
                .Must(year => year <= DateTime.UtcNow.Year)
                .WithMessage(_ => $"Enrollment year cannot be later than {DateTime.UtcNow.Year}.")
                .When(x => x.EnrollmentYear.HasValue);

            RuleFor(x => x.AcademicStatus)
                .IsEnumName(typeof(enAcademicStatus), caseSensitive: false)
                .WithMessage("Academic status must match a valid academic status value.")
                .When(x => !string.IsNullOrEmpty(x.AcademicStatus));

            RuleFor(x => x.MinGPA)
                .InclusiveBetween(0, 100.0m).WithMessage("Minimum GPA must be between 0 and 100.")
                .When(x => x.MinGPA.HasValue);

            RuleFor(x => x.MaxGPA)
                .InclusiveBetween(0, 100.0m).WithMessage("Maximum GPA must be between 0 and 100.")
                .When(x => x.MaxGPA.HasValue);

            RuleFor(x => x.MinGPA)
                .LessThanOrEqualTo(x => x.MaxGPA!.Value)
                .WithMessage("Minimum GPA cannot be greater than Maximum GPA.")
                .When(x => x.MinGPA.HasValue && x.MaxGPA.HasValue);

            RuleFor(x => x.SearchTerm)
                .MaximumLength(100).WithMessage("Search term must not exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.SearchTerm));
        }
    }
}