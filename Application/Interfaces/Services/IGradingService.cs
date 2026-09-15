using Application.DTOs.Grade;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface IGradingService
    {
        Task UpdateStudentGradeAsync(Guid sectionId, Guid studentId, decimal newGrade, CancellationToken cancellationToken = default);
        Task<int> FinalizeSemesterGradesAsync(Guid semesterId, CancellationToken cancellationToken = default);
        Task SubmitSectionGradesAsync(Guid sectionId, List<StudentGradeDto> grades, CancellationToken cancellationToken = default);
    }
}
