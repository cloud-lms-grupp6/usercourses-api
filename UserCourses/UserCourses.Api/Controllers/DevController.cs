using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using UserCourses.Api.Contracts;

namespace UserCourses.Api.Controllers;

[ApiController]
[Route("dev")]
public class DevController(IConfiguration config, IWebHostEnvironment env) : ControllerBase
{
    // genererar test-JWT bara i Development, tas bort när Auth-Api finns (KAN-24)
    [HttpPost("token")]
    public IActionResult CreateToken(TokenRequest request)
    {
        if (!env.IsDevelopment())
            return NotFound();

        var jwt = config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, request.Sub.ToString()),
            new Claim("role", request.Role)
        };

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}
