using Microsoft.EntityFrameworkCore;
using QuizApi.DAL.Interfaces;
using QuizApi.Domain;

namespace QuizApi.DAL.Repositories
{
    public class QuestionRepository : GenericRepository<Question>, IQuestionRepository
    {
        public QuestionRepository(QuizDbContext db) : base(db) { }

        public async Task<IEnumerable<Question>> GetByQuizIdAsync(int quizId, bool includeOptions = true, CancellationToken ct = default)
        {
            IQueryable<Question> q = _db.Questions.Where(x => x.QuizId == quizId);

            if (includeOptions)
                q = q.Include(x => x.Options);

            return await q.AsNoTracking().ToListAsync(ct);
        }
    }
}