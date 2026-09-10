using Application.DTOs.Curriculum;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // تأمين الكنترولر بالكامل: ممنوع أي حدا مش مسجل دخول يفوت
public class SpecializationsController : ControllerBase
{
    private readonly ISpecializationService _specializationService;
    private readonly ICurrentUserService _currentUserService;

    public SpecializationsController(
        ISpecializationService specializationService,
        ICurrentUserService currentUserService)
    {
        _specializationService = specializationService;
        _currentUserService = currentUserService;
    }

    // ==========================================
    // 1. Student Endpoints (واجهات الطالب)
    // ==========================================

    [HttpGet("my-plan")]
    [Authorize(Roles = AppRoles.Student)] // فقط الطالب
    public async Task<ActionResult<StudentStudyPlanDto>> GetMyStudyPlan(CancellationToken cancellationToken)
    {
        var studentId = _currentUserService.GetClaimAsGuid("StudentId");

        if (studentId == null)
            return Forbid("User is not a valid student.");

        var plan = await _specializationService.GetStudentStudyPlanAsync(studentId.Value, cancellationToken);

        if (plan == null)
            return NotFound("Study plan not found for this student.");

        return Ok(plan);
    }

    // ==========================================
    // 2. Public / Shared Endpoints (استعلامات عامة)
    // ==========================================

    // ما حطيت Roles معينة هون، بس بتعتمد على [Authorize] تبعت الكنترولر
    // يعني: الطالب، الدكتور، المسجل، والأدمن بيقدروا يشوفوا التخصصات
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SpecializationResponseDto>>> GetAll(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? collegeId,
        CancellationToken cancellationToken)
    {
        var result = await _specializationService.GetAllAsync(departmentId, collegeId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SpecializationResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _specializationService.GetByIdAsync(id, cancellationToken);
        if (result == null) return NotFound();

        return Ok(result);
    }

    [HttpGet("courses/{courseId}/prerequisite-tree")]
    public async Task<ActionResult<IEnumerable<CourseTreeNodeDto>>> GetCoursePrerequisitesTree(Guid courseId, CancellationToken cancellationToken)
    {
        var result = await _specializationService.GetPrerequisiteTreeAsync(courseId, cancellationToken);
        return Ok(result);
    }

    // ==========================================
    // 3. Management Endpoints (إدارة التخصصات)
    // ==========================================

    [HttpPost]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Registrar}")] // الأدمن أو المسجل
    public async Task<ActionResult<SpecializationResponseDto>> Create([FromBody] CreateSpecializationDto dto, CancellationToken cancellationToken)
    {
        var result = await _specializationService.CreateSpecializationAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.SpecializationId }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Registrar}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSpecializationDto dto, CancellationToken cancellationToken)
    {
        await _specializationService.UpdateSpecializationAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}/deactivate")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _specializationService.DeactivateSpecializationAsync(id, cancellationToken);
        return NoContent();
    }
}