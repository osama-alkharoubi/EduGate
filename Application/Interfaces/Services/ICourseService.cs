using Application.DTOs.Course;
using EduGate.Domain.Entities;


namespace Application.Interfaces.Services;

public interface ICourseService
{
    // عمليات الـ CRUD الأساسية للكتابة
    Task<Guid> CreateCourseAsync(CreateCourseDto dto, CancellationToken cancellationToken = default);
    Task UpdateCourseAsync(Guid courseId, UpdateCourseDto dto, CancellationToken cancellationToken = default);
    Task DeleteCourseAsync(Guid courseId, CancellationToken cancellationToken = default);

    // عمليات القراءة
    Task<CourseDto> GetCourseByIdAsync(Guid courseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CourseDto>> GetAllActiveCoursesAsync(Guid? departmentId = null, Guid? collegeId = null, CancellationToken cancellationToken = default);


    // عمليات إدارة المتطلبات السابقة
    Task AddPrerequisiteAsync(Guid courseId, Guid prerequisiteId, CancellationToken cancellationToken = default);
    Task RemovePrerequisiteAsync(Guid courseId, Guid prerequisiteId, CancellationToken cancellationToken = default);
}