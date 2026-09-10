using Application.DTOs.Student;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet("me")]
    [Authorize(Roles = AppRoles.Student)]
    [ProducesResponseType(typeof(StudentProfileHeaderDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<StudentProfileHeaderDto>> GetCurrentStudentProfile(CancellationToken cancellationToken)
    {
        var profile = await _studentService.GetCurrentStudentProfileAsync(cancellationToken);
        return Ok(profile);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles =AppRoles.Admin + "," + AppRoles.Registrar)]
    [ProducesResponseType(typeof(StudentDetailsDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<StudentDetailsDto>> GetStudentById(Guid id, CancellationToken cancellationToken)
    {
        var student = await _studentService.GetStudentByIdAsync(id, cancellationToken);
        return Ok(student);
    }

    [HttpGet("by-number/{universityNumber}")]
    [Authorize(Roles =AppRoles.Admin + "," + AppRoles.Registrar)]
    [ProducesResponseType(typeof(StudentDetailsDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<StudentDetailsDto>> GetStudentByUniversityNumber(string universityNumber, CancellationToken cancellationToken)
    {
        var student = await _studentService.GetStudentByUniversityNumberAsync(universityNumber, cancellationToken);
        return Ok(student);
    }
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _studentService.DeleteStudentAsync(id, cancellationToken);

        return Ok(new { message = "Student has been successfully deleted." });
    }

    [HttpGet]
    [Authorize(Roles =AppRoles.Admin + "," + AppRoles.Registrar)]
    [ProducesResponseType(typeof(IEnumerable<StudentListDto>), 200)]
    public async Task<ActionResult<IEnumerable<StudentListDto>>> GetStudents([FromQuery] StudentFilterDto filter, CancellationToken cancellationToken)
    {
        var students = await _studentService.GetStudentsByFilterAsync(filter, cancellationToken);
        return Ok(students);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles =AppRoles.Admin + "," + AppRoles.Registrar)]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateStudentDetails(Guid id, [FromBody] UpdateStudentDto dto, CancellationToken cancellationToken)
    {
        await _studentService.UpdateStudentDetailsAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles =AppRoles.Admin + "," + AppRoles.Registrar)]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateAcademicStatus(Guid id, [FromBody] enAcademicStatus newStatus, CancellationToken cancellationToken)
    {
        await _studentService.UpdateAcademicStatusAsync(id, newStatus, cancellationToken);
        return NoContent();
    }
}