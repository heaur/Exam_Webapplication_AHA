namespace QuizApi.DTOs
{
    // DTOs for Question entity
    // Create, Read, and Update operations
    public record QuestionCreateDto(string Text, int QuizSetId);
    public record QuestionReadDto(int Id, string Text, int QuizSetId);
    public record QuestionUpdateDto(string Text);
}