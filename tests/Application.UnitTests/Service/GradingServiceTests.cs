using Application.DTOs.Grade;
using Application.Interfaces.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Application.Services;
using EduGate.Domain.Entities;
using EduGate.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace EduGate.Application.UnitTests.Services;

public class GradingServiceTests
{
    private readonly IEnrollmentRepository _enrollmentRepoMock;
    private readonly ISectionRepository _sectionRepoMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly ICurrentUserService _currentUserServiceMock;
    private readonly GradingService _sut;
    private readonly Guid _validProfessorId = Guid.NewGuid();

    public GradingServiceTests()
    {
        _enrollmentRepoMock = Substitute.For<IEnrollmentRepository>();
        _sectionRepoMock = Substitute.For<ISectionRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _currentUserServiceMock = Substitute.For<ICurrentUserService>();


        _currentUserServiceMock.GetClaimAsGuid("ProfessorId").Returns(_validProfessorId);

        _sut = new GradingService(_enrollmentRepoMock, _sectionRepoMock, _unitOfWorkMock, _currentUserServiceMock);
    }

    [Fact]
    public void Property_ProfessorId_WhenClaimIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        _currentUserServiceMock.GetClaimAsGuid("ProfessorId").Returns((Guid?)null);

        // Act
        Action act = () => { var _ = _sut.ProfessorId; };

        // Assert
        act.Should().Throw<UnauthorizedAccessException>()
           .WithMessage("Current user is not a valid professor.");
    }

    [Fact]
    public async Task SubmitSectionGradesAsync_WhenProfessorNotAssigned_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var sectionId = Guid.NewGuid();
        _sectionRepoMock.IsProfessorAssignedToSectionAsync(sectionId, _validProfessorId).Returns(false);

        // Act
        var act = async () => await _sut.SubmitSectionGradesAsync(sectionId, new List<StudentGradeDto>());

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("The current professor is not assigned to this section.");
    }

    [Fact]
    public async Task SubmitSectionGradesAsync_WhenNoActiveEnrollments_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var sectionId = Guid.NewGuid();
        _sectionRepoMock.IsProfessorAssignedToSectionAsync(sectionId, _validProfessorId).Returns(true);
        _enrollmentRepoMock.GetEnrollmentsBySectionIdAsync(sectionId, Arg.Any<CancellationToken>())
            .Returns(new List<Enrollment>()); // قائمة فارغة

        // Act
        var act = async () => await _sut.SubmitSectionGradesAsync(sectionId, new List<StudentGradeDto>());

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("There is no active enrollment for this section.");
    }

    [Fact]
    public async Task SubmitSectionGradesAsync_WhenStudentNotEnrolled_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var sectionId = Guid.NewGuid();
        var enrolledStudentId = Guid.NewGuid();
        var fakeStudentId = Guid.NewGuid(); 

        _sectionRepoMock.IsProfessorAssignedToSectionAsync(sectionId, _validProfessorId).Returns(true);
        _enrollmentRepoMock.GetEnrollmentsBySectionIdAsync(sectionId, Arg.Any<CancellationToken>())
            .Returns(new List<Enrollment> { new Enrollment { StudentId = enrolledStudentId } });

        var grades = new List<StudentGradeDto>
        {
            new StudentGradeDto { StudentId = fakeStudentId, Grade = 90 }
        };

        // Act
        var act = async () => await _sut.SubmitSectionGradesAsync(sectionId, grades);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Student with ID '{fakeStudentId}' is not actively enrolled in this section.");
    }

    [Fact]
    public async Task SubmitSectionGradesAsync_WhenValid_ShouldSetGradesAndSaveChanges()
    {
        // Arrange
        var sectionId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var enrollment = Substitute.ForPartsOf<Enrollment>(); // نستخدم Substitute عشان نقدر نراقب استدعاء SetGrade
        enrollment.StudentId = studentId;

        _sectionRepoMock.IsProfessorAssignedToSectionAsync(sectionId, _validProfessorId).Returns(true);
        _enrollmentRepoMock.GetEnrollmentsBySectionIdAsync(sectionId, Arg.Any<CancellationToken>())
            .Returns(new List<Enrollment> { enrollment });

        var grades = new List<StudentGradeDto>
        {
            new StudentGradeDto { StudentId = studentId, Grade = 95 }
        };

        // Act
        await _sut.SubmitSectionGradesAsync(sectionId, grades);

        // Assert
        enrollment.Received(1).SetGrade(95);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateStudentGradeAsync_WhenEnrollmentNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var sectionId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        _sectionRepoMock.IsProfessorAssignedToSectionAsync(sectionId, _validProfessorId).Returns(true);
        _enrollmentRepoMock.GetBySectionAndStudentIdAsync(sectionId, studentId, Arg.Any<CancellationToken>())
            .Returns((Enrollment?)null);

        // Act
        var act = async () => await _sut.UpdateStudentGradeAsync(sectionId, studentId, 85);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateStudentGradeAsync_WhenValid_ShouldSetGradeAndSaveChanges()
    {
        // Arrange
        var sectionId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var enrollment = Substitute.ForPartsOf<Enrollment>();

        _sectionRepoMock.IsProfessorAssignedToSectionAsync(sectionId, _validProfessorId).Returns(true);
        _enrollmentRepoMock.GetBySectionAndStudentIdAsync(sectionId, studentId, Arg.Any<CancellationToken>())
            .Returns(enrollment);

        // Act
        await _sut.UpdateStudentGradeAsync(sectionId, studentId, 88);

        // Assert
        enrollment.Received(1).SetGrade(88);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FinalizeSemesterGradesAsync_WhenSemesterIdIsEmpty_ShouldThrowArgumentException()
    {
        // Act
        var act = async () => await _sut.FinalizeSemesterGradesAsync(Guid.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task FinalizeSemesterGradesAsync_WhenValid_ShouldExecuteBulkRecalculationAndReturnCount()
    {
        // Arrange
        var semesterId = Guid.NewGuid();
        var expectedAffectedStudents = 150;

        _enrollmentRepoMock.ExecuteBulkGpaRecalculationAsync(semesterId, Arg.Any<CancellationToken>())
            .Returns(expectedAffectedStudents);

        // Act
        var result = await _sut.FinalizeSemesterGradesAsync(semesterId);

        // Assert
        result.Should().Be(expectedAffectedStudents);
        await _enrollmentRepoMock.Received(1).ExecuteBulkGpaRecalculationAsync(semesterId, Arg.Any<CancellationToken>());
    }
}