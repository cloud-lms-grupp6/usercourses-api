namespace UserCourses.Api.Domain;

public class UserCourse
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public Guid CourseId { get; set; }

    public UserCourseRole Role { get; set; }

    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
}
