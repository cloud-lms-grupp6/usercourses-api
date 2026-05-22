using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserCourses.Api.Infrastructure;

namespace UserCourses.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // sista config-källan vinner, så test-JWT valideras mot dessa värden
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = TestJwt.Issuer,
                ["Jwt:Audience"] = TestJwt.Audience,
                ["Jwt:Key"] = TestJwt.Key
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // ta bort SQL Server-registreringen, annars krockar två databasprovidrar
            var descriptors = services
                .Where(d => d.ServiceType == typeof(UserCoursesDbContext)
                         || d.ServiceType == typeof(DbContextOptions)
                         || (d.ServiceType.IsGenericType
                             && d.ServiceType.GetGenericArguments()
                                 .Contains(typeof(UserCoursesDbContext))))
                .ToList();

            foreach (var descriptor in descriptors)
                services.Remove(descriptor);

            services.AddDbContext<UserCoursesDbContext>(options =>
                options.UseInMemoryDatabase("UserCoursesTests"));
        });
    }
}
