using Microsoft.EntityFrameworkCore;
using UserCourses.Api.Domain;

namespace UserCourses.Api.Infrastructure;

public class UserCoursesDbContext(DbContextOptions<UserCoursesDbContext> options)
    : DbContext(options)
{
    public DbSet<UserCourse> UserCourses => Set<UserCourse>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("usercourses");

        // en anmälan per (user, course, role)
        modelBuilder.Entity<UserCourse>()
            .HasIndex(uc => new { uc.UserId, uc.CourseId, uc.Role })
            .IsUnique();
    }
}
