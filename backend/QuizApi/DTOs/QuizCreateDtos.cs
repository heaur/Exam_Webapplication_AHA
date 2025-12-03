using System;
using System.Collections.Generic;

namespace QuizApi.DTOs
{
    // ------------------- CREATE (includes questions + options) -------------------

    // Single option for a question when creating a quiz
    public record CreateOptionDto(
        string Text,
        bool IsCorrect
    );

    // Question when creating a quiz
    public record CreateQuestionDto(
        string Text,
        List<CreateOptionDto> Options
    );

    // DTO used when the frontend creates a new quiz, including questions + options in the body
    public record QuizCreateDto(
        string Title,
        string SubjectCode,
        string? Description,
        string? ImageUrl,
        List<CreateQuestionDto> Questions
    );

    // ------------------- READ (list / details) -------------------

    public record QuizReadDto(
        int Id,
        string Title,
        string SubjectCode,
        string? Description,
        string? ImageUrl,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        bool IsPublished,
        DateTime? PublishedAt,
        string? OwnerId,
        int QuestionCount
    );

    // ------------------- UPDATE (metadata only) -------------------

    public record QuizUpdateDto(
        string Title,
        string SubjectCode,
        string? Description,
        string? ImageUrl,
        bool? IsPublished = null
    );
}
