using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TodoApi.Data;
using TodoApi.Dtos;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    AppDbContext db,
    IConfiguration cfg,
    IPasswordHasher<Models.User> hasher
) : ControllerBase
{
    [HttpPost("login")]
    [EnableRateLimiting("login")] // extra skydd mot brute force
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest req)
    {
        var user = await db
            .Users.AsNoTracking()
            .SingleOrDefaultAsync(u => u.Username == req.Username);
        if (user is null)
            return Unauthorized();

        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized();

        var jwtSection = cfg.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("name", user.Username),
            new(ClaimTypes.Role, user.Role),
        };

        var expires = DateTime.UtcNow.AddHours(1);
        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(
            new LoginResponse(
                tokenString,
                user.Username,
                user.Role,
                new DateTimeOffset(expires).ToUnixTimeSeconds()
            )
        );
    }
}
