namespace QuizApi.DTOs
{

    // DTOs for Quiz entity
    // Create, Read, and Update operations
public record QuizCreateDto(string Title, string? Description);
public record QuizReadDto(int Id, string Title, string? Description, DateTime CreatedAt);
public record QuizUpdateDto(string Title, string? Description);
}

