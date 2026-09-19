using Application.DTOs.Course;
using Application.Exceptions;
using Application.Interfaces.Common;
using Application.Interfaces.Repositories;
using Application.Services;
using EduGate.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace EduGate.Application.UnitTests.Services;

public class CourseServiceTests
{
    private readonly ICourseRepository _courseRepoMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly CourseService _sut;

    public CourseServiceTests()
    {
        _courseRepoMock = Substitute.For<ICourseRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _sut = new CourseService(_courseRepoMock, _unitOfWorkMock);
    }



    [Theory]
    [InlineData("CS101", "CS101")]
    [InlineData("cs101", "CS101")]
    [InlineData("  cs101  ", "CS101")]
    [InlineData("Cs101", "CS101")]
    [InlineData("math201", "MATH201")]
    [InlineData("  eng301  ", "ENG301")]
    public async Task CreateCourseAsync_WhenCourseCodeExists_ShouldThrowConflictException(
     string inputCode,
     string expectedCode)
    {
        // Arrange
        var dto = new CreateCourseDto
        {
            CourseCode = inputCode,
            CourseName = "Test",
            CreditHours = 3
        };


        _courseRepoMock.ExistsByCodeAsync(expectedCode, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var act = async () => await _sut.CreateCourseAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCourseAsync_WhenValidDto_ShouldAddCourseAndSaveChanges()
    {
        // Arrange
        var dto = new CreateCourseDto { CourseCode = "CS102", CourseName = "Data Structures", CreditHours = 3 };
        _courseRepoMock.ExistsByCodeAsync("CS102", Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await _sut.CreateCourseAsync(dto);

        // Assert
        result.Should().NotBeEmpty();
        _courseRepoMock.Received(1).Add(Arg.Is<Course>(c =>
            c.CourseCode == "CS102" && c.CourseName == "Data Structures" && c.IsActive));
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }





    [Fact]
    public async Task UpdateCourseAsync_WhenCourseDoesNotExistOrInactive_ShouldThrowNotFoundException()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var dto = new UpdateCourseDto { CourseName = "New Name", CreditHours = 4 };

        // الكورس غير موجود
        _courseRepoMock.GetByIdAsync(courseId, Arg.Any<CancellationToken>()).Returns((Course?)null);

        // Act
        var act = async () => await _sut.UpdateCourseAsync(courseId, dto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateCourseAsync_WhenValidRequest_ShouldUpdatePropertiesAndSaveChanges()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var dto = new UpdateCourseDto { CourseName = "Updated Name", CreditHours = 4, DepartmentId = Guid.NewGuid() };
        var existingCourse = new Course { CourseId = courseId, CourseName = "Old Name", CreditHours = 3, IsActive = true };

        _courseRepoMock.GetByIdAsync(courseId, Arg.Any<CancellationToken>()).Returns(existingCourse);

        // Act
        await _sut.UpdateCourseAsync(courseId, dto);

        // Assert
        existingCourse.CourseName.Should().Be("Updated Name");
        existingCourse.CreditHours.Should().Be(4);
        existingCourse.DepartmentId.Should().Be(dto.DepartmentId);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }





    [Fact]
    public async Task DeleteCourseAsync_WhenCourseExists_ShouldSetIsActiveToFalseAndSaveChanges()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var existingCourse = new Course { CourseId = courseId, IsActive = true };

        _courseRepoMock.GetByIdAsync(courseId, Arg.Any<CancellationToken>()).Returns(existingCourse);

        // Act
        await _sut.DeleteCourseAsync(courseId);

        // Assert
        existingCourse.IsActive.Should().BeFalse();
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }



    [Fact]
    public async Task AddPrerequisiteAsync_WhenCourseIsPrerequisiteToItself_ShouldThrowConflictException()
    {
        // Arrange
        var sameId = Guid.NewGuid();

        // Act
        var act = async () => await _sut.AddPrerequisiteAsync(sameId, sameId);

        // Assert
        await act.Should().ThrowAsync<ConflictException>().WithMessage("A course cannot be a prerequisite to itself.");
    }

    [Fact]
    public async Task AddPrerequisiteAsync_WhenPrerequisiteAlreadyAssigned_ShouldThrowConflictException()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var prerequisiteId = Guid.NewGuid();

        var course = new Course
        {
            CourseId = courseId,
            IsActive = true,
            Prerequisites = new List<CoursePrerequisite>
            {
                new CoursePrerequisite { PrerequisiteId = prerequisiteId }
            }
        };
        var prerequisiteCourse = new Course { CourseId = prerequisiteId, IsActive = true };

        _courseRepoMock.GetByIdWithPrerequisitesAsync(courseId, Arg.Any<CancellationToken>()).Returns(course);
        _courseRepoMock.GetByIdAsync(prerequisiteId, Arg.Any<CancellationToken>()).Returns(prerequisiteCourse);

        // Act
        var act = async () => await _sut.AddPrerequisiteAsync(courseId, prerequisiteId);

        // Assert
        await act.Should().ThrowAsync<ConflictException>().WithMessage("This prerequisite is already assigned to the course.");
    }

    [Fact]
    public async Task AddPrerequisiteAsync_WhenValid_ShouldAddPrerequisiteAndSaveChanges()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var prerequisiteId = Guid.NewGuid();

        var course = new Course { CourseId = courseId, IsActive = true, Prerequisites = new List<CoursePrerequisite>() };
        var prerequisiteCourse = new Course { CourseId = prerequisiteId, IsActive = true };

        _courseRepoMock.GetByIdWithPrerequisitesAsync(courseId, Arg.Any<CancellationToken>()).Returns(course);
        _courseRepoMock.GetByIdAsync(prerequisiteId, Arg.Any<CancellationToken>()).Returns(prerequisiteCourse);

        // Act
        await _sut.AddPrerequisiteAsync(courseId, prerequisiteId);

        // Assert
        course.Prerequisites.Should().ContainSingle(p => p.PrerequisiteId == prerequisiteId);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

}