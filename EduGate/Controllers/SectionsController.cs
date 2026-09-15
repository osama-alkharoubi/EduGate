using Application.DTOs.Section;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduGate.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SectionsController : ControllerBase
{
    private readonly ISectionService _sectionService;
    private readonly ICurrentUserService _currentUserService;
    public SectionsController(ISectionService sectionService,ICurrentUserService currentUserService)
    {
        _sectionService = sectionService;
        _currentUserService = currentUserService;
    }
    [HttpPost]
    [Authorize(Roles = "Admin, Registrar")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSection(
        [FromBody] CreateSectionDto dto,
        CancellationToken cancellationToken)
    {
        var sectionId = await _sectionService.CreateSectionAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetSectionById), new { id = sectionId }, new { SectionId = sectionId });
    }
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSectionById(Guid id, CancellationToken cancellationToken)
    {

        var section = await _sectionService.GetSectionByIdAsync(id, cancellationToken);
        if (section == null)
            return NotFound(new { Message = $"Section with ID {id} not found." });

        return Ok(section);
    }
    [HttpGet("available/{semesterId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
 
    public async Task<IActionResult> GetAvailableSections(
    [FromRoute] Guid semesterId,
    CancellationToken cancellationToken)
    {
        var studentId = _currentUserService.GetClaimAsGuid("StudentId");

        if (studentId == null)
            return Forbid("User is not a valid student.");

        var sections = await _sectionService.GetAvailableSectionsForStudentAsync(studentId.Value, semesterId, cancellationToken);
        return Ok(sections);
    }
    /// Updates an existing section.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin, Registrar")] // حماية: فقط الإدارة والمُسجل
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // لمشاكل الفاليديشن أو قواعد العمل (تعارض، سعة)
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // مش عامل Login
    [ProducesResponseType(StatusCodes.Status403Forbidden)] // عامل Login بس مش Admin
    [ProducesResponseType(StatusCodes.Status404NotFound)] // الشعبة مش موجودة
    public async Task<IActionResult> UpdateSection(
        Guid id,
        [FromBody] UpdateSectionDto dto,
        CancellationToken cancellationToken)
    {
        if (id != dto.SectionId)
            return BadRequest(new { Message = "The ID in the route does not match the SectionId in the payload." });

        // الـ Exception Middleware عندك المفروض يحول 
        // KeyNotFoundException -> 404
        // InvalidOperationException -> 400
        await _sectionService.UpdateSectionAsync(dto, cancellationToken);

        return NoContent();
    }
}