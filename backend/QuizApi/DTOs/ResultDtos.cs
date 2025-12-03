using System;
using System.Collections.Generic;

namespace QuizApi.DTOs
{
    // DTO used when creating a quiz result after submission.
    public record ResultCreateDto
    {
        public string? UserId { get; init; }

        public int QuizId { get; init; }

        public int CorrectCount { get; init; }
        public int TotalQuestions { get; init; }

        // questionId -> optionId (user's chosen option)
        public Dictionary<int, int> Answers { get; init; } = new();
    }

    // DTO returned when reading results (summary)
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

    // COMPLETE RESULT DTO FOR FULL RESULT PAGE
    // This DTO contains:
    // 1. Summary (ResultReadDto)
    // 2. Full quiz structure (QuizTakeDto)
    // 3. All user's answers (questionId -> optionId)
    public class FullResultDto
    {
        public ResultReadDto Result { get; set; } = null!;

        // IMPORTANT: this matches your QuizTakeDtos.cs file
        public TakeQuizDto Quiz { get; set; } = null!;

        public Dictionary<int, int> Answers { get; set; } = new();
    }
}
