using Application.DTOs.Grade;
using Application.Interfaces.Services;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/grading")]
[Authorize]
public class GradingController(IGradingService gradingService) : ControllerBase
{
    [HttpPost("sections/{sectionId:guid}/grades")]
    [Authorize(Roles = $"{AppRoles.Instructor},{AppRoles.Admin},{AppRoles.Registrar}")]
    public async Task<IActionResult> SubmitSectionGrades(
        Guid sectionId,
        [FromBody] List<StudentGradeDto> grades,
        CancellationToken cancellationToken)
    {
        if (sectionId == Guid.Empty || grades.Count == 0)
            return BadRequest("Invalid section ID or empty grades list.");

        await gradingService.SubmitSectionGradesAsync(sectionId, grades, cancellationToken);

        return Ok(new { Message = "Section grades submitted successfully." });
    }

    [HttpPut("sections/{sectionId:guid}/students/{studentId:guid}")]
    [Authorize(Roles = $"{AppRoles.Instructor},{AppRoles.Admin},{AppRoles.Registrar}")]
    public async Task<IActionResult> UpdateStudentGrade(
        Guid sectionId,
        Guid studentId,
        [FromBody] UpdateStudentGradeRequest request,
        CancellationToken cancellationToken)
    {
        if (sectionId == Guid.Empty || studentId == Guid.Empty)
            return BadRequest("Invalid section or student ID.");

        await gradingService.UpdateStudentGradeAsync(sectionId, studentId, request.NewGrade, cancellationToken);

        return Ok(new { Message = "Student grade updated successfully." });
    }

    [HttpPost("semesters/{semesterId:guid}/finalize")]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Registrar}")]
    public async Task<IActionResult> FinalizeSemesterGrades(
        Guid semesterId,
        CancellationToken cancellationToken)
    {
        if (semesterId == Guid.Empty)
            return BadRequest("Invalid semester ID.");

        var affected = await gradingService.FinalizeSemesterGradesAsync(semesterId, cancellationToken);

        return Ok(new { Message = "Semester grades finalized successfully.", AffectedStudents = affected });
    }
}

// Record منفصل ونظيف للـ Request
public record UpdateStudentGradeRequest(decimal NewGrade);