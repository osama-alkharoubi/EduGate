using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories
{
    public interface IAcademicValidationRepository
    {
        Task<UniversitySetting> GetUniversitySettingsAsync(CancellationToken cancellationToken = default);

        // إرجاع حالة الطالب الأكاديمية (Tuple)
        Task<(bool IsGraduating, byte HasAcademicWarning)?> GetStudentStatusAsync(Guid studentId, CancellationToken cancellationToken = default);
         Task<bool> HasUnmetPrerequisitesAsync(
   Guid studentId,
   List<Guid> sectionsToAdd,
   CancellationToken cancellationToken = default);
        Task<int> GetTotalCreditHoursAsync(List<Guid> sectionIds, CancellationToken cancellationToken = default);

      
    }
}
