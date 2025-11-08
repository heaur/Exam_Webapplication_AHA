using Microsoft.EntityFrameworkCore;
using QuizApi.DAL.Interfaces;
using QuizApi.Domain;

namespace QuizApi.DAL.Repositories
{
    public class QuizRepository : GenericRepository<Quiz>, IQuizRepository
    {
        public QuizRepository(QuizDbContext db) : base(db) { }

        public async Task<Quiz?> GetWithQuestionsAsync(int quizId, bool includeOptions = true, CancellationToken ct = default)
        {
            IQueryable<Quiz> q = _db.Quizzes
                .Include(z => z.Questions)
                .ThenInclude(q => q.Options);

            if (!includeOptions)
                q = _db.Quizzes.Include(z => z.Questions);

            return await q.FirstOrDefaultAsync(z => z.QuizId == quizId, ct);
        }

        public async Task<IEnumerable<Quiz>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
            => await _db.Quizzes.AsNoTracking()
                   .OrderBy(q => q.QuizId)
                   .Skip((page - 1) * pageSize)
                   .Take(pageSize)
                   .ToListAsync(ct);

        public Task<int> CountAsync(CancellationToken ct = default)
            => _db.Quizzes.CountAsync(ct);
    }
}