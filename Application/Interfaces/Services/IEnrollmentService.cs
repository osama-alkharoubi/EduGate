using System;
using System.Collections.Generic;
using System.Text;
using Application.DTOs.Enrollment;
namespace Application.Interfaces.Services
{
    public interface IEnrollmentService
    {
        // الدالة الوحيدة التي سيتعامل معها الـ Controller
        Task SyncStudentScheduleAsync(
            Guid studentId,
            ModifyScheduleRequestDto request,
            CancellationToken cancellationToken = default);

        
    }
}
