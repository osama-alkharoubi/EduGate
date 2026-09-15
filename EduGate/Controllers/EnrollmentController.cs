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

    [HttpPost("modify")]
    [Authorize(Roles = AppRoles.Student)]
    public async Task<IActionResult> ModifyMySchedule(
     [FromBody] ModifyScheduleRequestDto request,
     CancellationToken cancellationToken)
    {
        var tokenStudentId = _currentUserService.GetClaimAsGuid("StudentId");

        if (!tokenStudentId.HasValue || tokenStudentId.Value == Guid.Empty)
        {
            return Unauthorized(new { Error = "Student profile not found in token." });
        }

        await _enrollmentService.SyncStudentScheduleAsync(
            tokenStudentId.Value,
            request,
            cancellationToken);

        return Ok(new { Message = "Schedule modified successfully." });
    }
    [HttpPost("admin/{studentId:guid}/modify")]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Registrar}")]
    public async Task<IActionResult> ModifyStudentScheduleByAdmin(
    Guid studentId,
    [FromBody] ModifyScheduleRequestDto request,
    CancellationToken cancellationToken)
    {
        await _enrollmentService.SyncStudentScheduleAsync(
            studentId,
            request,
            cancellationToken);

        return Ok(new { Message = "Schedule modified successfully." });
    }
}
