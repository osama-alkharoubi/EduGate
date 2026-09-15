using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/schedule")]
[Authorize]
public class ScheduleController : ControllerBase
{
    private readonly IScheduleService _scheduleService;
    private readonly ICurrentUserService _currentUserService;

    public ScheduleController(
        IScheduleService scheduleService,
        ICurrentUserService currentUserService)
    {
        _scheduleService = scheduleService;
        _currentUserService = currentUserService;
    }

    [HttpGet("active")]
    [Authorize(Roles = AppRoles.Student)]
    public async Task<IActionResult> GetActiveSemesterSchedule(CancellationToken cancellationToken)
    {
        var studentId = _currentUserService.GetClaimAsGuid("StudentId");

        if (!studentId.HasValue || studentId.Value == Guid.Empty)
        {
            return Forbid();
        }

        var schedule = await _scheduleService.GetActiveSemesterScheduleAsync(
            studentId.Value,
            cancellationToken);

        return Ok(schedule);
    }
    [HttpGet("students/{studentId:guid}")]
    [Authorize(Roles = "Admin,Registrar")]
    public async Task<IActionResult> GetStudentSchedule(
    [FromRoute] Guid studentId,
    [FromQuery] Guid? semesterId,
    CancellationToken cancellationToken)
    {
        // إذا مرر الأدمن فصل محدد يجلب أرشيف ذلك الفصل، وإلا يجلب الفصل النشط حالياً
        var schedule = semesterId.HasValue
            ? await _scheduleService.GetScheduleBySemesterIdAsync(studentId, semesterId.Value, cancellationToken)
            : await _scheduleService.GetActiveSemesterScheduleAsync(studentId, cancellationToken);

        return Ok(schedule);
    }

    [HttpGet("{semesterId:guid}")]
    [Authorize(Roles = AppRoles.Student)]
    public async Task<IActionResult> GetScheduleBySemester(
        Guid semesterId,
        CancellationToken cancellationToken)
    {
        var studentId = _currentUserService.GetClaimAsGuid("StudentId");

        if (!studentId.HasValue || studentId.Value == Guid.Empty)
        {
            return Forbid();
        }

        var schedule = await _scheduleService.GetScheduleBySemesterIdAsync(
            studentId.Value,
            semesterId,
            cancellationToken);

        return Ok(schedule);
    }
    [HttpGet("teaching/active")]
    [Authorize(Roles = AppRoles.Instructor)]
    public async Task<IActionResult> GetActiveTeachingSchedule(CancellationToken cancellationToken)
    {
        var professorId = _currentUserService.GetClaimAsGuid("ProfessorId");

        if (!professorId.HasValue || professorId.Value == Guid.Empty)
        {
            return Forbid();
        }

        var schedule = await _scheduleService.GetProfessorScheduleAsync(
            professorId.Value,
            semesterId: null,
            cancellationToken);

        return Ok(schedule);
    }

    [HttpGet("teaching/{semesterId:guid}")]
    [Authorize(Roles = AppRoles.Instructor+","+ AppRoles.Admin)]
    public async Task<IActionResult> GetTeachingScheduleBySemester(
        [FromRoute] Guid semesterId,
        CancellationToken cancellationToken)
    {
        var professorId = _currentUserService.GetClaimAsGuid("ProfessorId");

        if (!professorId.HasValue || professorId.Value == Guid.Empty)
        {
            return Forbid();
        }

        var schedule = await _scheduleService.GetProfessorScheduleAsync(
            professorId.Value,
            semesterId: semesterId,
            cancellationToken);

        return Ok(schedule);
    }
}
