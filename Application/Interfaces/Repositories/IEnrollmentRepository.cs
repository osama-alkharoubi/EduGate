using Application.DTOs.Enrollment;
using EduGate.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories
{
    public interface IEnrollmentRepository
    {
        public Task<int> ExecuteBulkGpaRecalculationAsync(Guid semesterId, CancellationToken cancellationToken = default);
        Task<List<Guid>> GetActiveStudentSectionIdsAsync(Guid studentId, CancellationToken cancellationToken = default);

        // فحص التعارض مع استثناء المواد اللي رح تنحذف
        Task<bool> HasBatchScheduleConflictAsync(
            Guid studentId,
            List<Guid> sectionIdsToAdd,
            List<Guid> sectionIdsToDrop,
            CancellationToken cancellationToken = default);

        // تنفيذ الحذف والإضافة بضربة واحدة
        Task ModifyScheduleAtomicAsync(
            Guid studentId,
            List<Guid> sectionsToAdd, 
            List<Guid> sectionsToDrop,
            CancellationToken cancellationToken = default);
        Task<Enrollment?> GetBySectionAndStudentIdAsync(Guid sectionId, Guid studentId, CancellationToken cancellationToken = default);
        Task<List<Enrollment>> GetEnrollmentsBySectionIdAsync(Guid sectionId, CancellationToken cancellationToken = default);

       
    }
}
