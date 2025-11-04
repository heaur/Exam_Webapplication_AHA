using QuizApi.Domain;

namespace QuizApi.DAL.Interfaces
{
    public interface IQuizRepository : IGenericRepository<Quiz>
    {
        Task<Quiz?> GetWithQuestionsAsync(int quizId, bool includeOptions = true, CancellationToken ct = default);
        Task<IEnumerable<Quiz>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
        Task<int> CountAsync(CancellationToken ct = default);
    }
}