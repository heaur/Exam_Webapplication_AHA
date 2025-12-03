namespace QuizApi.Domain
{
    // Én rad per spørsmål i et quiz-resultat
public class ResultAnswer
{
    public int Id { get; set; }

    public int ResultId { get; set; }
    public Result Result { get; set; } = null!;

    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public int OptionId { get; set; }
    public Option Option { get; set; } = null!;
}

}
