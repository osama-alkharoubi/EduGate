using Application.DTOs.Enrollment;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/enrollments")]
[Authorize]
public class EnrollmentController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly ICurrentUserService _currentUserService;
    public EnrollmentController(IEnrollmentService enrollmentService, ICurrentUserService currentUserService)
    {
        _enrollmentService = enrollmentService;
        _currentUserService = currentUserService;
    }

    [HttpPost("{studentId}/modify")]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Registrar},{AppRoles.Student}")]
    public async Task<IActionResult> ModifySchedule(
     Guid studentId,
     [FromBody] ModifyScheduleRequestDto request,
     CancellationToken cancellationToken)
    {
        // 1. فحص الصلاحيات العليا (المايسترو)
        bool isSuperUser = _currentUserService.Roles.Contains(AppRoles.Admin) || _currentUserService.Roles.Contains(AppRoles.Registrar);

        // إذا لم يكن يملك صلاحيات إدارية، نطبق عليه قيود الطالب
        if (!isSuperUser)
        {
            var tokenStudentId = _currentUserService.GetClaimAsGuid("StudentId");

            // منعه من التعديل إذا كان الـ ID في التوكن لا يطابق الـ ID في الرابط
            if (tokenStudentId != studentId)
            {
                return StatusCode(403, new { Error = "Forbidden: You are not authorized to modify another student's schedule." });
            }
        }

        // 2. تنفيذ العملية (الـ Middleware سيتكفل بالتقاط أي Exception وارجاعه كـ 400 أو 500)
        await _enrollmentService.SyncStudentScheduleAsync(
            studentId,
            request,
            cancellationToken);

        return Ok(new { Message = "Schedule modified successfully." });
    }
}
