namespace QuizApi.DTOs
{
    // DTO used when creating a new question under a quiz.
    public record QuestionCreateDto(string Text, int QuizId);

    // DTO returned when reading questions.
    public record QuestionReadDto(int QuestionId, string Text, int QuizId);

    // DTO used when updating an existing question.
    public record QuestionUpdateDto(string Text);
}
