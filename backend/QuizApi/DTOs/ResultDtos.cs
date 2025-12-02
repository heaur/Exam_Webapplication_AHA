using System;

namespace QuizApi.DTOs
{
    // DTO used when creating a quiz result after submission.
    // Frontend trenger egentlig ikke å sende UserId, men vi beholder feltet
    // fordi noen interne tjenester fortsatt leser/skriv­er det.
    public record ResultCreateDto(
        string? UserId,
        int QuizId,
        int CorrectCount,
        int TotalQuestions
    );

    // DTO returned when reading results, includes percentage for convenience.
    public record ResultReadDto(
        int ResultId,
        string? UserId,
        int QuizId,
        string QuizTitle,
        string SubjectCode,
        int CorrectCount,
        int TotalQuestions,
        DateTime CompletedAt,
        double Percentage
    );
}
