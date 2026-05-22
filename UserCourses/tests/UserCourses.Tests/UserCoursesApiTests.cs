using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using UserCourses.Api.Contracts;

namespace UserCourses.Tests;

public class UserCoursesApiTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private HttpClient ClientFor(Guid userId, string role)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwt.Create(userId, role));
        return client;
    }

    [Fact]
    public async Task Enroll_with_valid_token_returns_201()
    {
        var client = ClientFor(Guid.NewGuid(), "Student");

        var response = await client.PostAsJsonAsync(
            "/enrollments", new EnrollRequest(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Enroll_twice_in_same_course_returns_409()
    {
        var client = ClientFor(Guid.NewGuid(), "Student");
        var request = new EnrollRequest(Guid.NewGuid());

        await client.PostAsJsonAsync("/enrollments", request);
        var second = await client.PostAsJsonAsync("/enrollments", request);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task GetByUser_returns_only_that_users_enrollments()
    {
        var userId = Guid.NewGuid();
        var client = ClientFor(userId, "Student");
        await client.PostAsJsonAsync("/enrollments", new EnrollRequest(Guid.NewGuid()));
        await client.PostAsJsonAsync("/enrollments", new EnrollRequest(Guid.NewGuid()));

        // annan student anmäler sig, ska inte synas i förstas lista
        var otherClient = ClientFor(Guid.NewGuid(), "Student");
        await otherClient.PostAsJsonAsync("/enrollments", new EnrollRequest(Guid.NewGuid()));

        var response = await client.GetAsync($"/users/{userId}/enrollments");
        response.EnsureSuccessStatusCode();
        var enrollments = await response.Content
            .ReadFromJsonAsync<List<UserCourseResponse>>();

        Assert.NotNull(enrollments);
        Assert.Equal(2, enrollments.Count);
        Assert.All(enrollments, e => Assert.Equal(userId, e.UserId));
    }

    [Fact]
    public async Task Enroll_without_token_returns_401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/enrollments", new EnrollRequest(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Student_assigning_instructor_returns_403()
    {
        var client = ClientFor(Guid.NewGuid(), "Student");

        var response = await client.PostAsJsonAsync(
            $"/courses/{Guid.NewGuid()}/instructors",
            new AssignInstructorRequest(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Student_reading_other_users_enrollments_returns_403()
    {
        var client = ClientFor(Guid.NewGuid(), "Student");

        var response = await client.GetAsync($"/users/{Guid.NewGuid()}/enrollments");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
