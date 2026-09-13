using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories
{
    public interface IEnrollmentRepository
    {
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
            List<Guid> sectionsToDrop,
            List<Guid> sectionsToAdd,
            CancellationToken cancellationToken = default);

       
    }
}
