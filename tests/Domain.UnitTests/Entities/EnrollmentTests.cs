using EduGate.Domain.Entities;
using FluentAssertions;
using System;
using Xunit;
using Domain.Enums;
namespace EduGate.Domain.UnitTests.Entities;

public class EnrollmentTests
{
    // 1. فحص القيم المرفوضة (أقل من 0 أو أكثر من 100)
    [Theory]
    [InlineData(-1)]
    [InlineData(-50.5)]
    [InlineData(150)]
    public void SetGrade_WhenGradeIsOutOfRange_ShouldThrowArgumentOutOfRangeException(decimal invalidGrade)
    {
        // Arrange
        var enrollment = new Enrollment();

        // Act
        Action act = () => enrollment.SetGrade(invalidGrade);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Grade must be between 0 and 100.");
    }

    // 2. فحص حالة النجاح
    [Theory]
    [InlineData(50)]
    [InlineData(75.5)]
    [InlineData(100)]
    public void SetGrade_WhenGradeIsPassable_ShouldSetGradeAndMarkAsCompleted(decimal validGrade)
    {
        // Arrange
        var enrollment = new Enrollment();

        // Act
        enrollment.SetGrade(validGrade);

        // Assert
        enrollment.Grade.Should().Be(validGrade);
        enrollment.Status.Should().Be(enEnrollmentStatus.Completed);
 
    }


    [Theory]
    [InlineData(0)]
    [InlineData(49.9)]
    public void SetGrade_WhenGradeIsFailing_ShouldSetGradeAndMarkAsFailed(decimal failingGrade)
    {
        // Arrange
        var enrollment = new Enrollment();

        // Act
        enrollment.SetGrade(failingGrade);

        // Assert
        enrollment.Grade.Should().Be(failingGrade);
        enrollment.Status.Should().Be(enEnrollmentStatus.Failed);
    }
}