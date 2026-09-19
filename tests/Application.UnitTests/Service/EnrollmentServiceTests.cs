using Application.DTOs.Enrollment;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace EduGate.Application.UnitTests.Services;

public class EnrollmentServiceTests
{
    private readonly IEnrollmentRepository _enrollmentRepoMock;
    private readonly IAcademicValidationService _validationServiceMock;
    private readonly EnrollmentService _sut;

    public EnrollmentServiceTests()
    {
        _enrollmentRepoMock = Substitute.For<IEnrollmentRepository>();
        _validationServiceMock = Substitute.For<IAcademicValidationService>();
        _sut = new EnrollmentService(_enrollmentRepoMock, _validationServiceMock);
    }

    [Theory]
    [InlineData(false, false)] // كلاهما فارغ Count == 0
    [InlineData(true, true)]   // كلاهما null
    [InlineData(true, false)]  // الأولى null والثانية فارغة
    [InlineData(false, true)]  // الأولى فارغة والثانية null
    public async Task SyncStudentScheduleAsync_WhenNoSectionsProvided_ShouldReturnImmediately(
       bool isAddNull,
       bool isDropNull)
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var request = new ModifyScheduleRequestDto
        {
            SectionsToAdd = isAddNull ? null! : new List<Guid>(),
            SectionsToDrop = isDropNull ? null! : new List<Guid>()
        };

        // Act
        await _sut.SyncStudentScheduleAsync(studentId, request);

        // Assert
        await _validationServiceMock.DidNotReceiveWithAnyArgs().ValidatePrerequisitesAsync(default, default!, default);
        await _enrollmentRepoMock.DidNotReceiveWithAnyArgs().ModifyScheduleAtomicAsync(default, default!, default!, default);
    }

    [Fact]
    public async Task SyncStudentScheduleAsync_WhenConflictExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var request = new ModifyScheduleRequestDto
        {
            SectionsToAdd = new List<Guid> { Guid.NewGuid() }
        };

        _enrollmentRepoMock.GetActiveStudentSectionIdsAsync(studentId, Arg.Any<CancellationToken>())
            .Returns(new List<Guid>());

  
        _enrollmentRepoMock.HasBatchScheduleConflictAsync(studentId, request.SectionsToAdd, Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var act = async () => await _sut.SyncStudentScheduleAsync(studentId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("There is a schedule conflict in the proposed sections.");
        await _enrollmentRepoMock.DidNotReceiveWithAnyArgs().ModifyScheduleAtomicAsync(default, default!, default!, default);
    }

    [Fact]
    public async Task SyncStudentScheduleAsync_WhenValidRequest_ShouldExecuteAtomicTransaction()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var sectionToAdd = Guid.NewGuid();
        var sectionToDrop = Guid.NewGuid();

        var request = new ModifyScheduleRequestDto
        {
            SectionsToAdd = new List<Guid> { sectionToAdd },
            SectionsToDrop = new List<Guid> { sectionToDrop }
        };

        _enrollmentRepoMock.GetActiveStudentSectionIdsAsync(studentId, Arg.Any<CancellationToken>())
            .Returns(new List<Guid> { sectionToDrop });

        _enrollmentRepoMock.HasBatchScheduleConflictAsync(studentId, request.SectionsToAdd, request.SectionsToDrop, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _sut.SyncStudentScheduleAsync(studentId, request);

        // Assert

        await _validationServiceMock.Received(1).ValidatePrerequisitesAsync(studentId, request.SectionsToAdd, Arg.Any<CancellationToken>());
        await _validationServiceMock.Received(1).ValidateCreditHoursAsync(studentId, Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>());
        await _enrollmentRepoMock.Received(1).ModifyScheduleAtomicAsync(studentId, request.SectionsToAdd, request.SectionsToDrop, Arg.Any<CancellationToken>());
    }
}