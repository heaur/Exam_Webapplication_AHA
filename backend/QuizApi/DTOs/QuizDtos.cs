namespace QuizApi.DTOs
{
    // Create Quiz DTO
    public record QuizCreateDto(string Title, string? Description);

    // Read DTO includes QuestionCount
    public record QuizReadDto(
        int Id, string Title, string? Description,
        DateTime CreatedAt, DateTime? UpdatedAt,
        bool IsPublished, DateTime? PublishedAt,
        int? OwnerId, int? QuestionCount);
    
    // Update Quiz DTO
    public record QuizUpdateDto(string Title, string? Description, bool? IsPublished = null);
}