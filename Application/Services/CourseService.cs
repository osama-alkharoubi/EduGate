using Application.DTOs.Course;
using Application.Interfaces.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using EduGate.Domain.Entities;
using Application.Exceptions;

namespace Application.Services {

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CourseService(
        ICourseRepository courseRepository,
        IUnitOfWork unitOfWork)
    {
        _courseRepository = courseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateCourseAsync(
        CreateCourseDto dto,
        CancellationToken cancellationToken = default)
    {
        var courseCode = dto.CourseCode.Trim().ToUpperInvariant();

        var exists = await _courseRepository
            .ExistsByCodeAsync(courseCode, cancellationToken);

        if (exists)
        {
            throw new ConflictException(
                $"Course with code '{courseCode}' already exists.");
        }

        var course = new Course
        {
            CourseId = Guid.NewGuid(),
            CourseCode = courseCode,
            CourseName = dto.CourseName.Trim(),
            CreditHours = dto.CreditHours,
            DepartmentId = dto.DepartmentId,
            IsActive = true
        };

        _courseRepository.Add(course);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return course.CourseId;
    }

    public async Task UpdateCourseAsync(
        Guid courseId,
        UpdateCourseDto dto,
        CancellationToken cancellationToken = default)
    {
        var course = await _courseRepository
            .GetByIdAsync(courseId, cancellationToken);

        if (course == null || !course.IsActive)
        {
            throw new NotFoundException(
                $"Course with ID '{courseId}' was not found.");
        }

        course.CourseName = dto.CourseName.Trim();
        course.CreditHours = dto.CreditHours;
        course.DepartmentId = dto.DepartmentId;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteCourseAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var course = await _courseRepository
            .GetByIdAsync(courseId, cancellationToken);

        if (course == null || !course.IsActive)
        {
            throw new NotFoundException(
                $"Course with ID '{courseId}' was not found.");
        }

        // Soft Delete
        course.IsActive = false;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<CourseDto> GetCourseByIdAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var course = await _courseRepository
            .GetByIdWithPrerequisitesAsync(
                courseId,
                cancellationToken);

        if (course == null || !course.IsActive)
        {
            throw new NotFoundException(
                $"Course with ID '{courseId}' was not found.");
        }

        return new CourseDto
        {
            CourseId = course.CourseId,
            CourseCode = course.CourseCode,
            CourseName = course.CourseName,
            CreditHours = course.CreditHours,
            DepartmentId = course.DepartmentId,
            DepartmentName =
                course.Department?.DepartmentName ?? string.Empty,
            IsActive = course.IsActive,

            Prerequisites = course.Prerequisites
                .Select(p => new PrerequisiteDto
                {
                    PrerequisiteId = p.PrerequisiteId,
                    CourseCode = p.PrerequisiteCourse.CourseCode,
                    CourseName = p.PrerequisiteCourse.CourseName
                })
                .ToList()
        };
    }

        public async Task<IReadOnlyList<CourseDto>> GetAllActiveCoursesAsync(Guid? departmentId = null, Guid? collegeId = null, CancellationToken cancellationToken = default)
        {
            return await _courseRepository.GetAllActiveCoursesAsync(departmentId, collegeId, cancellationToken);
        }

        public async Task AddPrerequisiteAsync(
        Guid courseId,
        Guid prerequisiteId,
        CancellationToken cancellationToken = default)
    {
        if (courseId == prerequisiteId)
        {
            throw new ConflictException(
                "A course cannot be a prerequisite to itself.");
        }

        var course = await _courseRepository
            .GetByIdWithPrerequisitesAsync(
                courseId,
                cancellationToken);

        if (course == null || !course.IsActive)
        {
            throw new NotFoundException(
                $"Course with ID '{courseId}' was not found.");
        }

        var prerequisiteCourse = await _courseRepository
            .GetByIdAsync(
                prerequisiteId,
                cancellationToken);

        if (prerequisiteCourse == null ||
            !prerequisiteCourse.IsActive)
        {
            throw new NotFoundException(
                $"Prerequisite course with ID '{prerequisiteId}' was not found.");
        }

        var alreadyExists = course.Prerequisites
            .Any(p => p.PrerequisiteId == prerequisiteId);

        if (alreadyExists)
        {
            throw new ConflictException(
                "This prerequisite is already assigned to the course.");
        }

        course.Prerequisites.Add(new CoursePrerequisite
        {
            CourseId = courseId,
            PrerequisiteId = prerequisiteId
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemovePrerequisiteAsync(
        Guid courseId,
        Guid prerequisiteId,
        CancellationToken cancellationToken = default)
    {
        var course = await _courseRepository
            .GetByIdWithPrerequisitesAsync(
                courseId,
                cancellationToken);

        if (course == null || !course.IsActive)
        {
            throw new NotFoundException(
                $"Course with ID '{courseId}' was not found.");
        }

        var prerequisite = course.Prerequisites
            .FirstOrDefault(
                p => p.PrerequisiteId == prerequisiteId);

        if (prerequisite == null)
        {
            throw new NotFoundException(
                $"Prerequisite relationship between course '{courseId}' " +
                $"and prerequisite '{prerequisiteId}' was not found.");
        }

        course.Prerequisites.Remove(prerequisite);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
    }
}