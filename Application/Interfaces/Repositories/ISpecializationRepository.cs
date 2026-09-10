using Application.DTOs.Curriculum;
using EduGate.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories
{
    public interface ISpecializationRepository
    {
        // القسم الأول: استعلامات القراءة البسيطة (EF Core)
        Task<Specialization?> GetSpecializationByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Specialization>> GetAllSpecializationsAsync(Guid? departmentId = null, Guid? collegeId = null,CancellationToken cancellationToken = default);
        
        // عمليات الإدارة والتعديل (Commands - Void)
        // التخزين الفعلي بتكفل فيه IUnitOfWork
        void AddSpecialization(Specialization specialization);
        void UpdateSpecialization(Specialization specialization);
        void DeleteSpecialization(Specialization specialization);

        // القسم الثاني: استعلامات التقارير والخطة (Dapper - Complex & Fast Queries)
        Task<StudentStudyPlanDto?> GetStudentStudyPlanAsync(Guid studentId, CancellationToken cancellationToken = default);
        Task<bool> ExistsByCodeAsync(int code, CancellationToken cancellationToken = default);
        Task<bool> ExistsByCodeExcludeIdAsync(int code, Guid excludeId, CancellationToken cancellationToken = default);
        Task<int?> GetCodeById(Guid id, CancellationToken cancellationToken = default);
        // الاستعلام الشجري للمتطلبات السابقة
        Task<IEnumerable<CourseTreeNodeDto>> GetPrerequisiteTreeAsync(Guid courseId, CancellationToken cancellationToken = default);
    }
}
