using System.ComponentModel.DataAnnotations;

namespace TodoApi.Dtos;

public record TodoCreateRequest([Required, MaxLength(200)] string Title);

public record TodoUpdateRequest([Required, MaxLength(200)] string Title, bool IsDone);

public record TodoResponse(int Id, string Title, bool IsDone, DateTime CreatedAtUtc);
