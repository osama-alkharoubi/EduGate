using Application.DTOs.Student;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface  IStudentService
    {
        Task<StudentProfileHeaderDto> GetCurrentStudentProfileAsync(CancellationToken cancellationToken = default);

        // عمليات القراءة للأدمن / التسجيل
        Task<StudentDetailsDto> GetStudentByIdAsync(Guid studentId, CancellationToken cancellationToken = default);
        Task<StudentDetailsDto> GetStudentByUniversityNumberAsync(string universityNumber, CancellationToken cancellationToken = default);
        Task<IEnumerable<StudentListDto>> GetStudentsByFilterAsync(StudentFilterDto filter, CancellationToken cancellationToken = default);

     
        public Task DeleteStudentAsync(Guid studentId, CancellationToken cancellationToken = default);
        Task UpdateStudentDetailsAsync(Guid studentId, UpdateStudentDto dto, CancellationToken cancellationToken = default);
        Task UpdateAcademicStatusAsync(Guid studentId, enAcademicStatus newStatus, CancellationToken cancellationToken = default);
    }
}

