using System;

namespace QuizApi.DTOs
{

    // DTOs for Quiz entity
    // Create, Read, and Update operations
    public record QuizCreateDto(string Title, string? Description);
    public record QuizReadDto(int Id, string Title, string? Description, DateTime CreatedAt, DateTime? UpdatedAt = null, bool IsPublished = false, DateTime? PublishedAt = null, int? OwnerId = null, int? QuestionCount = null);
    public record QuizUpdateDto(string Title, string? Description, bool? IsPublished = null);
}
