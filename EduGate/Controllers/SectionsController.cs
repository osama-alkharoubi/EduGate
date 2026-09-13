using Application.DTOs.Section;
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

    public SectionsController(ISectionService sectionService)
    {
        _sectionService = sectionService;
    }

    /// <summary>
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