using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface IAcademicValidationService
    {
        Task ValidateCreditHoursAsync(Guid studentId, List<Guid> submittedSectionIds, CancellationToken cancellationToken = default);

        // فحص اجتياز المتطلبات السابقة للمواد الجديدة
        Task ValidatePrerequisitesAsync(Guid studentId, List<Guid> sectionsToAdd, CancellationToken cancellationToken = default);
    }
}
