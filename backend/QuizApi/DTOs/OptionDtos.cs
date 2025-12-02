namespace QuizApi.DTOs
{
    // DTO used when creating an answer option for a question.
    public record OptionCreateDto(string Text, bool IsCorrect, int QuestionId);

    // DTO returned when reading options.
    public record OptionReadDto(int OptionId, string Text, bool IsCorrect, int QuestionId);

    // DTO used when updating an option.
    public record OptionUpdateDto(string Text, bool IsCorrect);
}
