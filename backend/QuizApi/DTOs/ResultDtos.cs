namespace QuizApi.DTOs
{
    // DTO used when creating a quiz result after submission.
    public record ResultCreateDto(string UserId, int QuizId, int CorrectCount, int TotalQuestions);

    // DTO returned when reading results, includes percentage for convenience.
    public record ResultReadDto(
        int ResultId,
        string UserId,
        int QuizId,
        int CorrectCount,
        int TotalQuestions,
        DateTime CompletedAt,
        double Percentage
    );
}
