using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Dtos;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TodosController(AppDbContext db) : ControllerBase
{
    private int CurrentUserId
    {
        get
        {
            var idClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (idClaim is null)
                throw new UnauthorizedAccessException("No user id claim present");

            return int.Parse(idClaim.Value);
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> Get()
    {
        var items = await db
            .Todos.AsNoTracking()
            .Where(t => t.UserId == CurrentUserId)
            .OrderByDescending(t => t.CreatedAtUtc)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.IsDone,
                t.CreatedAtUtc,
            })
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create(TodoApi.Dtos.TodoCreateRequest req)
    {
        var todo = new TodoApi.Models.TodoItem { Title = req.Title, UserId = CurrentUserId };
        db.Todos.Add(todo);
        await db.SaveChangesAsync();
        return Created(
            $"/api/todos/{todo.Id}",
            new
            {
                todo.Id,
                todo.Title,
                todo.IsDone,
                todo.CreatedAtUtc,
            }
        );
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, TodoApi.Dtos.TodoUpdateRequest req)
    {
        var todo = await db.Todos.SingleOrDefaultAsync(t =>
            t.Id == id && t.UserId == CurrentUserId
        );
        if (todo is null)
            return NotFound();
        todo.Title = req.Title;
        todo.IsDone = req.IsDone;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var todo = await db.Todos.SingleOrDefaultAsync(t =>
            t.Id == id && t.UserId == CurrentUserId
        );
        if (todo is null)
            return NotFound();
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
