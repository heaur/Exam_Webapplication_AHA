namespace QuizApi.DTOs
{
    // DTOs for Option entity
    // Create, Read, and Update operations
    public record OptionCreateDto(string Text, bool IsCorrect, int QuestionId);
    public record OptionReadDto(int Id, string Text, bool IsCorrect, int QuestionId);
    public record OptionUpdateDto(string Text, bool IsCorrect);
}