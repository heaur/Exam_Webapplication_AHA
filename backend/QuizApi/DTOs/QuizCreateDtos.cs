using System;
using System.Collections.Generic;

namespace QuizApi.DTOs
{
    // ------------------- CREATE (inkl. spørsmål + alternativer) -------------------

    // Enkelt alternativ for et spørsmål når vi OPPRETTER quiz
    public record CreateOptionDto(
        string Text,
        bool IsCorrect
    );

    // Spørsmål når vi OPPRETTER quiz
    public record CreateQuestionDto(
        string Text,
        List<CreateOptionDto> Options
    );

    // DTO brukt når frontend OPPRETTER en ny quiz.
    // Frontend sender også med questions + options i body-en.
    public record QuizCreateDto(
        string Title,
        string SubjectCode,
        string? Description,
        string? ImageUrl,
        List<CreateQuestionDto> Questions
    );

    // ------------------- READ (liste / detaljer) -------------------

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

    // ------------------- UPDATE (kun metadata) -------------------

    public record QuizUpdateDto(
        string Title,
        string SubjectCode,
        string? Description,
        string? ImageUrl,
        bool? IsPublished = null
    );
}
