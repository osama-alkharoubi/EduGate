using Application.DTOs.Course;
using EduGate.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories
{
    public interface ICourseRepository
    {
        Task<Course?> GetByIdAsync(Guid courseId, CancellationToken cancellationToken = default);
        Task<Course?> GetByIdWithPrerequisitesAsync(Guid courseId, CancellationToken cancellationToken = default);
        Task<bool> ExistsByCodeAsync(string courseCode, CancellationToken cancellationToken = default);
        Task<bool> ExistsByIdAsync(Guid courseId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<CourseDto>> GetAllActiveCoursesAsync(Guid? departmentId = null, Guid? collegeId = null, CancellationToken cancellationToken = default);
        void Add(Course course);
        void Update(Course course);
         void Remove(Course course);
    }
}
