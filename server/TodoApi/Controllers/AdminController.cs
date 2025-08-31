using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Dtos;
using TodoApi.Models;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(AppDbContext db, IPasswordHasher<User> hasher) : ControllerBase
{
    [HttpPost("users")] // skapa ny användare (Admin-only)
    public async Task<IActionResult> CreateUser(CreateUserRequest req)
    {
        var exists = await db.Users.AnyAsync(u => u.Username == req.Username);
        if (exists)
            return Conflict("Username already exists");

        if (req.Role is not ("Admin" or "User"))
            return BadRequest("Role must be 'Admin' or 'User'");

        var user = new User { Username = req.Username, Role = req.Role };
        user.PasswordHash = hasher.HashPassword(user, req.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return Created(
            $"/api/admin/users/{user.Id}",
            new
            {
                user.Id,
                user.Username,
                user.Role,
            }
        );
    }
}
