namespace QuizApi.DAL.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IQuizRepository Quizzes { get; }
        IQuestionRepository Questions { get; }

        Task<int> SaveChangesAsync(CancellationToken ct = default);
        Task BeginTransactionAsync(CancellationToken ct = default);
        Task CommitAsync(CancellationToken ct = default);
        Task RollbackAsync(CancellationToken ct = default);
    }
}