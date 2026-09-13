using Application.DTOs.Section;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface ISectionService
    {
        Task UpdateSectionAsync(UpdateSectionDto dto, CancellationToken cancellationToken = default);
        Task<Guid> CreateSectionAsync(CreateSectionDto dto, CancellationToken cancellationToken = default);
        Task<IEnumerable<AvailableSectionDto>> GetAvailableSectionsForStudentAsync(Guid studentId, Guid semesterId, CancellationToken cancellationToken = default);
        Task<SectionDetailsDto?> GetSectionByIdAsync(Guid sectionId, CancellationToken cancellationToken = default);
    }
}
