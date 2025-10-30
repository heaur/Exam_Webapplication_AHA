using QuizApp.Models;

namespace QuizApp.DAL.Interfaces
{
    public interface IQuestionRepository : IGenericRepository<Question>
    {
        Task<IEnumerable<Question>> GetByQuizIdAsync(int quizId, bool includeOptions = true, CancellationToken ct = default);
    }
}