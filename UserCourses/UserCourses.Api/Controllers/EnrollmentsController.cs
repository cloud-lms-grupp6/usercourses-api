using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserCourses.Api.Contracts;
using UserCourses.Api.Domain;
using UserCourses.Api.Infrastructure;

namespace UserCourses.Api.Controllers;

[ApiController]
[Route("enrollments")]
[Authorize]
public class EnrollmentsController(UserCoursesDbContext db) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> Enroll(EnrollRequest request)
    {
        var userId = CurrentUserId;

        bool alreadyEnrolled = await db.UserCourses.AnyAsync(uc =>
            uc.UserId == userId &&
            uc.CourseId == request.CourseId &&
            uc.Role == UserCourseRole.Student);

        if (alreadyEnrolled)
            return Conflict("Already enrolled in this course.");

        var enrollment = new UserCourse
        {
            UserId = userId,
            CourseId = request.CourseId,
            Role = UserCourseRole.Student
        };

        db.UserCourses.Add(enrollment);
        await db.SaveChangesAsync();

        var response = new UserCourseResponse(
            enrollment.Id,
            enrollment.UserId,
            enrollment.CourseId,
            enrollment.Role,
            enrollment.Status,
            enrollment.EnrolledAt);

        return Created($"/enrollments/{enrollment.Id}", response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Unenroll(Guid id)
    {
        var enrollment = await db.UserCourses.FindAsync(id);
        if (enrollment is null)
            return NotFound();

        db.UserCourses.Remove(enrollment);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // mina kurser, student ser bara sina egna (KAN-23)
    [HttpGet("/users/{userId:guid}/enrollments")]
    public async Task<IActionResult> GetByUser(Guid userId)
    {
        bool privileged = User.IsInRole("Instructor") || User.IsInRole("Admin");
        if (!privileged && userId != CurrentUserId)
            return Forbid();

        var enrollments = await db.UserCourses
            .Where(uc => uc.UserId == userId)
            .Select(uc => new UserCourseResponse(
                uc.Id,
                uc.UserId,
                uc.CourseId,
                uc.Role,
                uc.Status,
                uc.EnrolledAt))
            .ToListAsync();

        return Ok(enrollments);
    }

    // deltagare på en kurs
    [HttpGet("/courses/{courseId:guid}/enrollments")]
    public async Task<IActionResult> GetByCourse(Guid courseId)
    {
        var enrollments = await db.UserCourses
            .Where(uc => uc.CourseId == courseId)
            .Select(uc => new UserCourseResponse(
                uc.Id,
                uc.UserId,
                uc.CourseId,
                uc.Role,
                uc.Status,
                uc.EnrolledAt))
            .ToListAsync();

        return Ok(enrollments);
    }
}
