using Microsoft.EntityFrameworkCore.Storage;
using QuizApp.DAL.Interfaces;
using QuizApp.Models;

namespace QuizApp.DAL.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly QuizDBContext _db;
        private IDbContextTransaction? _tx;

        public IQuizRepository Quizzes { get; }
        public IQuestionRepository Questions { get; }

        public UnitOfWork(QuizDBContext db, IQuizRepository quizRepo, IQuestionRepository questionRepo)
        {
            _db = db;
            Quizzes = quizRepo;
            Questions = questionRepo;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);

        public async Task BeginTransactionAsync(CancellationToken ct = default)
        {
            if (_tx == null)
                _tx = await _db.Database.BeginTransactionAsync(ct);
        }

        public async Task CommitAsync(CancellationToken ct = default)
        {
            if (_tx != null)
            {
                await _tx.CommitAsync(ct);
                await _tx.DisposeAsync();
                _tx = null;
            }
        }

        public async Task RollbackAsync(CancellationToken ct = default)
        {
            if (_tx != null)
            {
                await _tx.RollbackAsync(ct);
                await _tx.DisposeAsync();
                _tx = null;
            }
        }

        public ValueTask DisposeAsync()
        {
            _tx?.Dispose();
            return _db.DisposeAsync();
        }
    }
}