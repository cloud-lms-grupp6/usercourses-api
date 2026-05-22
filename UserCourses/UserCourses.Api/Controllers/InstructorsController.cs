using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserCourses.Api.Contracts;
using UserCourses.Api.Domain;
using UserCourses.Api.Infrastructure;

namespace UserCourses.Api.Controllers;

[ApiController]
public class InstructorsController(UserCoursesDbContext db) : ControllerBase
{
    [HttpPost("/courses/{courseId:guid}/instructors")]
    public async Task<IActionResult> Assign(Guid courseId, AssignInstructorRequest request)
    {
        bool alreadyInstructor = await db.UserCourses.AnyAsync(uc =>
            uc.UserId == request.UserId &&
            uc.CourseId == courseId &&
            uc.Role == UserCourseRole.Instructor);

        if (alreadyInstructor)
            return Conflict("Already an instructor for this course.");

        var instructor = new UserCourse
        {
            UserId = request.UserId,
            CourseId = courseId,
            Role = UserCourseRole.Instructor
        };

        db.UserCourses.Add(instructor);
        await db.SaveChangesAsync();

        var response = new UserCourseResponse(
            instructor.Id,
            instructor.UserId,
            instructor.CourseId,
            instructor.Role,
            instructor.Status,
            instructor.EnrolledAt);

        return Created($"/enrollments/{instructor.Id}", response);
    }

    [HttpDelete("/courses/{courseId:guid}/instructors/{userId:guid}")]
    public async Task<IActionResult> Remove(Guid courseId, Guid userId)
    {
        var instructor = await db.UserCourses.FirstOrDefaultAsync(uc =>
            uc.UserId == userId &&
            uc.CourseId == courseId &&
            uc.Role == UserCourseRole.Instructor);

        if (instructor is null)
            return NotFound();

        db.UserCourses.Remove(instructor);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
