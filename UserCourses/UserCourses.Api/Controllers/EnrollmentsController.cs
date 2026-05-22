using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserCourses.Api.Contracts;
using UserCourses.Api.Domain;
using UserCourses.Api.Infrastructure;

namespace UserCourses.Api.Controllers;

[ApiController]
[Route("enrollments")]
public class EnrollmentsController(UserCoursesDbContext db) : ControllerBase
{
    // hårdkodad tills KAN-23 (JWT) ger riktig användare
    private static readonly Guid DevUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [HttpPost]
    public async Task<IActionResult> Enroll(EnrollRequest request)
    {
        bool alreadyEnrolled = await db.UserCourses.AnyAsync(uc =>
            uc.UserId == DevUserId &&
            uc.CourseId == request.CourseId &&
            uc.Role == UserCourseRole.Student);

        if (alreadyEnrolled)
            return Conflict("Already enrolled in this course.");

        var enrollment = new UserCourse
        {
            UserId = DevUserId,
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

    // mina kurser — userId från URL tills KAN-23 (JWT) sätter inloggad användare
    [HttpGet("/users/{userId:guid}/enrollments")]
    public async Task<IActionResult> GetByUser(Guid userId)
    {
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
}
