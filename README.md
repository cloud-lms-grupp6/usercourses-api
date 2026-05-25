# UserCourses-Api

Microservice for LMS enrollments (user ↔ course links). Owns nothing about users (Auth-Api) or courses (Course-Api), only the relation.

**Live:** https://usercourses-api-ax-a2ebbpf5ejcwcyby.polandcentral-01.azurewebsites.net

## Stack

.NET 10 · ASP.NET Core Web API · EF Core · Azure SQL · JWT (HS256, stub)

## Endpoints

| Method | Route                              | Role           |
|--------|------------------------------------|----------------|
| POST   | `/enrollments`                     | instructor     |
| DELETE | `/enrollments/{id}`                | instructor     |
| GET    | `/users/{userId}/enrollments`      | own data / instructor |
| GET    | `/courses/{courseId}/participants` | instructor     |
| GET    | `/courses/{courseId}/instructor`   | any auth       |

All endpoints require `Authorization: Bearer <jwt>`. JWT uses short claim names `sub` + `role`.

## Local dev

```bash
cd UserCourses/UserCourses.Api
dotnet run --launch-profile http
```

- API: http://localhost:5107
- Scalar UI: http://localhost:5107/scalar/v1
- Dev token: `GET /dev/token?userId=<guid>&role=student` (Development only)

## Tests

```bash
cd UserCourses
dotnet test tests/UserCourses.Tests/UserCourses.Tests.csproj
```

## Deploy

GitHub Actions workflow `.github/workflows/ci.yml` runs build + test on every push/PR, deploys to Azure App Service on push to `main` (publish profile secret `AZURE_WEBAPP_PUBLISH_PROFILE`).
