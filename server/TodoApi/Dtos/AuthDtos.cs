using System.ComponentModel.DataAnnotations;

namespace TodoApi.Dtos;

public record LoginRequest([Required] string Username, [Required] string Password);

public record LoginResponse(string Token, string Username, string Role, long ExpiresAtUtc);

public record CreateUserRequest(
    [Required] string Username,
    [Required] string Password,
    [Required] string Role
);
