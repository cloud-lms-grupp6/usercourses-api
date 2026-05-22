using UserCourses.Api.Domain;

namespace UserCourses.Api.Contracts;

public record UserCourseResponse(
    Guid Id,
    Guid UserId,
    Guid CourseId,
    UserCourseRole Role,
    EnrollmentStatus Status,
    DateTime EnrolledAt);
