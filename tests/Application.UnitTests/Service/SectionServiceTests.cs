using Application.DTOs.Section;
using Application.Exceptions;
using Application.Interfaces.Common;
using EduGate.Application.Services;
using EduGate.Domain.Entities;
using EduGate.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EduGate.Application.UnitTests.Services;

public class SectionServiceTests
{
    private readonly ISectionRepository _repoMock;
    private readonly IUnitOfWork _uowMock;
    private readonly SectionService _sut;

    public SectionServiceTests()
    {
        _repoMock = Substitute.For<ISectionRepository>();
        _uowMock = Substitute.For<IUnitOfWork>();
        _sut = new SectionService(_repoMock, _uowMock);
    }

    // ==========================================
    // Tests for CreateSectionAsync
    // ==========================================

    [Fact]
    public async Task CreateSectionAsync_WhenSectionNumberExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var dto = new CreateSectionDto { CourseId = Guid.NewGuid(), SemesterId = Guid.NewGuid(), SectionNumber = 1 };

        _repoMock.IsSectionNumberExistsAsync(dto.CourseId, dto.SemesterId, dto.SectionNumber, null, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var act = async () => await _sut.CreateSectionAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Section number {dto.SectionNumber} already exists.");
    }

    [Fact]
    public async Task CreateSectionAsync_WhenProfessorHasConflict_ShouldThrowConflictException()
    {
        // Arrange
        var dto = new CreateSectionDto { CourseId = Guid.NewGuid(), SemesterId = Guid.NewGuid(), ProfessorId = Guid.NewGuid() };

        _repoMock.IsSectionNumberExistsAsync(dto.CourseId, dto.SemesterId, dto.SectionNumber, null, Arg.Any<CancellationToken>())
            .Returns(false);

        _repoMock.HasProfessorScheduleConflictAsync(dto.ProfessorId, dto.SemesterId, dto.DaysOfWeek, dto.StartTime, dto.EndTime, null, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var act = async () => await _sut.CreateSectionAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("The assigned professor has a schedule conflict with another section at this time.");
    }

    [Fact]
    public async Task CreateSectionAsync_WhenValid_ShouldAddSectionAndSaveChanges()
    {
        // Arrange
        var dto = new CreateSectionDto { CourseId = Guid.NewGuid(), SemesterId = Guid.NewGuid(), ProfessorId = Guid.NewGuid() };

        _repoMock.IsSectionNumberExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<int>(), null, Arg.Any<CancellationToken>())
            .Returns(false);

        _repoMock.HasProfessorScheduleConflictAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<TimeOnly>(), Arg.Any<TimeOnly>(), null, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.CreateSectionAsync(dto);

        // Assert
        result.Should().NotBeEmpty();
        _repoMock.Received(1).Add(Arg.Is<Section>(s => s.CourseId == dto.CourseId && s.ProfessorId == dto.ProfessorId));
        await _uowMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ==========================================
    // Tests for UpdateSectionAsync
    // ==========================================

    [Fact]
    public async Task UpdateSectionAsync_WhenSectionNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var dto = new UpdateSectionDto { SectionId = Guid.NewGuid() };

        _repoMock.GetByIdAsync(dto.SectionId, Arg.Any<CancellationToken>())
            .Returns((Section?)null);

        // Act
        var act = async () => await _sut.UpdateSectionAsync(dto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateSectionAsync_WhenCapacityIsLessThanActiveEnrollments_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var dto = new UpdateSectionDto { SectionId = Guid.NewGuid(), Capacity = 20 };
        var section = new Section { SectionId = dto.SectionId };

        _repoMock.GetByIdAsync(dto.SectionId, Arg.Any<CancellationToken>()).Returns(section);
        _repoMock.GetActiveEnrollmentCountAsync(dto.SectionId, Arg.Any<CancellationToken>()).Returns(25); // 25 طلاب مسجلين والسعة الجديدة 20

        // Act
        var act = async () => await _sut.UpdateSectionAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Cannot reduce capacity to {dto.Capacity}. There are already 25 active enrollments.");
    }

    [Fact]
    public async Task UpdateSectionAsync_WhenSectionNumberExists_ShouldThrowConflictException()
    {
        // Arrange
        var dto = new UpdateSectionDto { SectionId = Guid.NewGuid(), Capacity = 30, SectionNumber = 2 };
        var section = new Section { SectionId = dto.SectionId };

        _repoMock.GetByIdAsync(dto.SectionId, Arg.Any<CancellationToken>()).Returns(section);
        _repoMock.GetActiveEnrollmentCountAsync(dto.SectionId, Arg.Any<CancellationToken>()).Returns(10);

        _repoMock.IsSectionNumberExistsAsync(dto.CourseId, dto.SemesterId, dto.SectionNumber, dto.SectionId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var act = async () => await _sut.UpdateSectionAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task UpdateSectionAsync_WhenProfessorHasConflict_ShouldThrowConflictException()
    {
        // Arrange
        var dto = new UpdateSectionDto { SectionId = Guid.NewGuid(), Capacity = 30 };
        var section = new Section { SectionId = dto.SectionId };

        _repoMock.GetByIdAsync(dto.SectionId, Arg.Any<CancellationToken>()).Returns(section);
        _repoMock.GetActiveEnrollmentCountAsync(dto.SectionId, Arg.Any<CancellationToken>()).Returns(10);
        _repoMock.IsSectionNumberExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _repoMock.HasProfessorScheduleConflictAsync(dto.ProfessorId, dto.SemesterId, dto.DaysOfWeek, dto.StartTime, dto.EndTime, dto.SectionId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var act = async () => await _sut.UpdateSectionAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task UpdateSectionAsync_WhenValid_ShouldUpdatePropertiesAndSaveChanges()
    {
        // Arrange
        var dto = new UpdateSectionDto
        {
            SectionId = Guid.NewGuid(),
            Capacity = 30,
            RoomNumber = "A101"
        };
        var section = new Section { SectionId = dto.SectionId, RoomNumber = "OldRoom" };

        _repoMock.GetByIdAsync(dto.SectionId, Arg.Any<CancellationToken>()).Returns(section);
        _repoMock.GetActiveEnrollmentCountAsync(dto.SectionId, Arg.Any<CancellationToken>()).Returns(10);
        _repoMock.IsSectionNumberExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(false);
        _repoMock.HasProfessorScheduleConflictAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<TimeOnly>(), Arg.Any<TimeOnly>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(false);

        // Act
        await _sut.UpdateSectionAsync(dto);

        // Assert
        section.RoomNumber.Should().Be(dto.RoomNumber); 
        await _uowMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}