using Application.DTOs.Semester;
using Application.Interfaces.Services;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduGate.Api.Controllers;

[ApiController]
[Route("api/semesters")]
[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Registrar)]
public class SemesterController : ControllerBase
{
    private readonly ISemesterService _semesterService;

    public SemesterController(ISemesterService semesterService)
    {
        _semesterService = semesterService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSemester(
        [FromBody] CreateSemesterDto dto,
        CancellationToken cancellationToken)
    {
        var semesterId = await _semesterService.CreateSemesterAsync(dto, cancellationToken);
        return Created(string.Empty, semesterId);
    }

    [HttpPut("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateSemester(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _semesterService.ActivateSemesterAsync(id, cancellationToken);
        return NoContent();
    }
}