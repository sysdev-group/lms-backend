using LMS.Application.DTOs.Grades;
using LMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

/// <summary>
/// Grade visibility and publishing. Students view their own published grades;
/// lecturers and admins view all grades and control publishing.
/// </summary>
[Route("api/v1/grades")]
[Authorize]
public class GradesController : BaseController
{
    private readonly IGradeService _gradeService;

    public GradesController(IGradeService gradeService)
    {
        _gradeService = gradeService;
    }

    /// <summary>
    /// Returns grades for a student. Students may only fetch their own published grades;
    /// lecturers and admins may fetch grades for any student regardless of publish state.
    /// </summary>
    /// <param name="studentId">The student whose grades are being requested.</param>
    /// <param name="courseId">Optional course filter.</param>
    [HttpGet("student/{studentId:guid}")]
    [ProducesResponseType(typeof(List<GradeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByStudent(Guid studentId, [FromQuery] Guid? courseId)
    {
        var result = await _gradeService.GetByStudentAsync(studentId, courseId);
        return ApiOk(result);
    }

    /// <summary>
    /// Returns all grades (published and unpublished) for every assignment in a course.
    /// Restricted to the owning lecturer and admins.
    /// </summary>
    /// <param name="courseId">The course whose grades are being retrieved.</param>
    [HttpGet("course/{courseId:guid}")]
    [Authorize(Roles = "Lecturer,Admin")]
    [ProducesResponseType(typeof(List<GradeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCourse(Guid courseId)
    {
        var result = await _gradeService.GetByCourseAsync(courseId);
        return ApiOk(result);
    }

    /// <summary>
    /// Publishes a single grade and sends an in-app notification to the student.
    /// Restricted to the owning lecturer and admins.
    /// </summary>
    /// <param name="id">The grade to publish.</param>
    [HttpPut("{id:guid}/publish")]
    [Authorize(Roles = "Lecturer,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Publish(Guid id)
    {
        await _gradeService.PublishGradeAsync(id);
        return ApiOk<object?>(null, "Grade published.");
    }

    /// <summary>
    /// Bulk-publishes all unpublished grades for a course and notifies each affected student.
    /// Restricted to the owning lecturer and admins.
    /// </summary>
    /// <param name="courseId">The course whose grades should all be published.</param>
    [HttpPut("course/{courseId:guid}/publish-all")]
    [Authorize(Roles = "Lecturer,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishAll(Guid courseId)
    {
        await _gradeService.PublishAllGradesForCourseAsync(courseId);
        return ApiOk<object?>(null, "All grades published.");
    }
}
