namespace QuizApi.DTOs
{
    // Create result DTO
    public record ResultCreateDto(int UserId, int QuizId, int CorrectCount, int TotalQuestions);

    // Read result DTO with Percentage
    public record ResultReadDto(
        int ResultId, int UserId, int QuizId,
        int CorrectCount, int TotalQuestions,
        DateTime CompletedAt, double Percentage);
}