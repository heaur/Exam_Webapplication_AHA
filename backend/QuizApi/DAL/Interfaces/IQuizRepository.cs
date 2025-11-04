using QuizApi.Domain;

namespace QuizApi.DAL.Interfaces
{
    public interface IQuestionRepository : IGenericRepository<Question>
    {
        Task<IEnumerable<Question>> GetByQuizIdAsync(int quizId, bool includeOptions = true, CancellationToken ct = default);
    }
}