namespace QuizApi.DTOs
{
    // DTOs for Result entity
    // Create and Read operations
    public record ResultCreateDto(int UserId, int QuizId, int Score);
    public record ResultReadDto(int Id, int UserId, int QuizId, int Score, DateTime TakenAt);
}