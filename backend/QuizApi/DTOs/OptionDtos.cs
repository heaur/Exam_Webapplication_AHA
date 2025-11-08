namespace QuizApi.DTOs
{
    // Create option DTO
    public record OptionCreateDto(string Text, bool IsCorrect, int QuestionId);

    // Read option DTO
    public record OptionReadDto(int OptionId, string Text, bool IsCorrect, int QuestionId);

    // Update option DTO
    public record OptionUpdateDto(string Text, bool IsCorrect);
}