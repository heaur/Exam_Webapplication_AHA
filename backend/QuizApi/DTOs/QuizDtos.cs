namespace QuizApi.DTOs
{
public record QuizSetCreateDto(string Title, string? Description);
public record QuizSetReadDto(int Id, string Title, string? Description, DateTime CreatedAt);
}

