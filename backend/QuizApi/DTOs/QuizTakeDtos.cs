using System.Collections.Generic;

namespace QuizApi.DTOs
{
    // DTO sent from backend to frontend when a user takes a quiz.
    // Contains quiz metadata and a full list of questions + options.
    public sealed class TakeQuizDto
    {
        // Database id for the quiz.
        public int Id { get; set; }

        // Course / subject code (e.g. ITPE3200). Empty string if not set.
        public string SubjectCode { get; set; } = string.Empty;

        // Quiz title, shown in the UI.
        public string Title { get; set; } = string.Empty;

        // Short description of the quiz.
        public string Description { get; set; } = string.Empty;

        // Cover image URL for the quiz. Can be empty if you do not use it.
        public string ImageUrl { get; set; } = string.Empty;

        // Whether the quiz is published/visible to users.
        public bool IsPublished { get; set; }

        // All questions for this quiz in the order they should be shown.
        public List<TakeQuestionDto> Questions { get; set; } = new();
    }

    // DTO for a single question in the "take quiz" view.
    public sealed class TakeQuestionDto
    {
        // Database id for the question.
        public int Id { get; set; }

        // Question text shown to the user.
        public string Text { get; set; } = string.Empty;

        // Optional image URL for the question. Null if not used.
        public string? ImageUrl { get; set; }

        // Number of points this question is worth.
        public int Points { get; set; }

        // List of answer options for this question.
        public List<TakeOptionDto> Options { get; set; } = new();
    }

    // DTO for a single answer option.
    public sealed class TakeOptionDto
    {
        // Database id for the option.
        public int Id { get; set; }

        // Text shown next to the radio button.
        public string Text { get; set; } = string.Empty;

        // True if this is the correct option for the question.
        public bool IsCorrect { get; set; }
    }
}
