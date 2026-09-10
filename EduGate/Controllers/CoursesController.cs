using Application.DTOs.Course;
using Application.Interfaces.Services;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduGate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // إغلاق الكنترولر بالكامل بحيث لا يدخله إلا من يملك Token
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin+","+AppRoles.Registrar)] // حصر الإضافة بموظفي التسجيل والإدارة
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateCourse(
        [FromBody] CreateCourseDto dto,
        CancellationToken cancellationToken)
    {
        var courseId = await _courseService.CreateCourseAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetCourseById), new { id = courseId }, courseId);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Registrar)] 
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCourse(
        Guid id,
        [FromBody] UpdateCourseDto dto,
        CancellationToken cancellationToken)
    {
        await _courseService.UpdateCourseAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Registrar)] 
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCourse(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _courseService.DeleteCourseAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourseById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var course = await _courseService.GetCourseByIdAsync(id, cancellationToken);
        return Ok(course);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CourseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllActiveCourses(
     [FromQuery] Guid? departmentId,
     [FromQuery] Guid? collegeId,
     CancellationToken cancellationToken)
    {
        var courses = await _courseService.GetAllActiveCoursesAsync(departmentId, collegeId, cancellationToken);
        return Ok(courses);
    }

    [HttpPost("{id:guid}/prerequisites/{prerequisiteId:guid}")]
    [Authorize(Roles = "Admin,Registrar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddPrerequisite(
        Guid id,
        Guid prerequisiteId,
        CancellationToken cancellationToken)
    {
        await _courseService.AddPrerequisiteAsync(id, prerequisiteId, cancellationToken);
        return Ok(new { message = "Prerequisite added successfully." });
    }

    [HttpDelete("{id:guid}/prerequisites/{prerequisiteId:guid}")]
    [Authorize(Roles = "Admin,Registrar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemovePrerequisite(
        Guid id,
        Guid prerequisiteId,
        CancellationToken cancellationToken)
    {
        await _courseService.RemovePrerequisiteAsync(id, prerequisiteId, cancellationToken);
        return NoContent();
    }
}