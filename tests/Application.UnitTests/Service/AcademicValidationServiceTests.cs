using Application.Interfaces.Repositories;
using Application.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;
using Domain.Entities;
namespace EduGate.Application.UnitTests.Services;

public class AcademicValidationServiceTests
{
    private readonly IAcademicValidationRepository _repoMock;
    private readonly AcademicValidationService _sut;

    public AcademicValidationServiceTests()
    {
        _repoMock = Substitute.For<IAcademicValidationRepository>();
        _sut = new AcademicValidationService(_repoMock);
    }

    [Theory]
    [InlineData(false)] 
    [InlineData(true)]  
    public async Task ValidatePrerequisitesAsync_WhenSectionsIsNullOrEmpty_ShouldReturnImmediately(bool isNull)
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var sectionsToAdd = isNull ? null : new List<Guid>();

        // Act
        await _sut.ValidatePrerequisitesAsync(studentId, sectionsToAdd!);

        // Assert
        await _repoMock.DidNotReceiveWithAnyArgs().HasUnmetPrerequisitesAsync(default, default!, default);
    }

    [Fact]
    public async Task ValidatePrerequisitesAsync_WhenHasUnmetPrerequisites_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var sectionsToAdd = new List<Guid> { Guid.NewGuid() };

        _repoMock.HasUnmetPrerequisitesAsync(studentId, sectionsToAdd, Arg.Any<CancellationToken>())
            .Returns(true); // يوجد متطلبات غير مجتازة

        // Act
        var act = async () => await _sut.ValidatePrerequisitesAsync(studentId, sectionsToAdd);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Registration denied: Unmet prerequisites for one or more selected courses.");
    }

    [Fact]
    public async Task ValidateCreditHoursAsync_WhenSettingsNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var sections = new List<Guid> { Guid.NewGuid() };

        _repoMock.GetUniversitySettingsAsync(Arg.Any<CancellationToken>())
            .Returns((UniversitySetting)null!); // إعدادات غير موجودة

        // Act
        var act = async () => await _sut.ValidateCreditHoursAsync(studentId, sections);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("University settings are not configured in the system.");
    }

    [Theory]
    [InlineData(0, false, 8)]  
    [InlineData(0, false, 19)] 
    [InlineData(1, false, 15)] 
    [InlineData(0, true, 22)]  
    public async Task ValidateCreditHoursAsync_WhenCreditsAreInvalid_ShouldThrowInvalidOperationException(
        byte warningCount,
        bool isGraduating,
        int requestedCredits)
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var sections = new List<Guid> { Guid.NewGuid() };

        var mockSettings = new UniversitySetting
        {
            MinCreditHours = 9,
            MaxCreditHours = 18,
            MaxCreditHoursForWarning = 12,
            MaxCreditHoursForGraduating = 21
        };
        _repoMock.GetUniversitySettingsAsync(Arg.Any<CancellationToken>()).Returns(mockSettings);

        (bool IsGraduating, byte HasAcademicWarning)? mockStatus = (isGraduating, warningCount);
        _repoMock.GetStudentStatusAsync(studentId, Arg.Any<CancellationToken>()).Returns(mockStatus);

        _repoMock.GetTotalCreditHoursAsync(sections, Arg.Any<CancellationToken>()).Returns(requestedCredits);

        // Act
        var act = async () => await _sut.ValidateCreditHoursAsync(studentId, sections);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }


    [Theory]
    [InlineData(0, false, 15)]  
    [InlineData(1, false, 12)]  
    [InlineData(0, true, 21)]  
    public async Task ValidateCreditHoursAsync_WhenCreditsAreValid_ShouldNotThrowException(
        byte warningCount,
        bool isGraduating,
        int requestedCredits)
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var sections = new List<Guid> { Guid.NewGuid() };

        var mockSettings = new UniversitySetting
        {
            MinCreditHours = 9,
            MaxCreditHours = 18,
            MaxCreditHoursForWarning = 12,
            MaxCreditHoursForGraduating = 21
        };
        _repoMock.GetUniversitySettingsAsync(Arg.Any<CancellationToken>()).Returns(mockSettings);

        (bool IsGraduating, byte HasAcademicWarning)? mockStatus = (isGraduating, warningCount);
        _repoMock.GetStudentStatusAsync(studentId, Arg.Any<CancellationToken>()).Returns(mockStatus);

        _repoMock.GetTotalCreditHoursAsync(sections, Arg.Any<CancellationToken>()).Returns(requestedCredits);

        // Act
        var act = async () => await _sut.ValidateCreditHoursAsync(studentId, sections);

        // Assert
        await act.Should().NotThrowAsync();
    }
}