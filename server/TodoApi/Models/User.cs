using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string Username { get; set; } = default!;

    [Required]
    public string PasswordHash { get; set; } = default!;

    [Required, MaxLength(16)]
    public string Role { get; set; } = "User"; // "Admin" eller "User"

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<TodoItem> Todos { get; set; } = new List<TodoItem>();
}
