namespace QuizApi.DTOs
{
    // Create question DTO
    public record QuestionCreateDto(string Text, int QuizId);

    // Read question DTO
    public record QuestionReadDto(int QuestionId, string Text, int QuizId);

    // Update question DTO
    public record QuestionUpdateDto(string Text);
}