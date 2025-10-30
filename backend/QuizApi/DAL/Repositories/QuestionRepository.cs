using Microsoft.EntityFrameworkCore;
using QuizApp.DAL.Interfaces;
using QuizApp.Models;

namespace QuizApp.DAL.Repositories
{
    public class QuestionRepository : GenericRepository<Question>, IQuestionRepository
    {
        public QuestionRepository(QuizDBContext db) : base(db) { }

        public async Task<IEnumerable<Question>> GetByQuizIdAsync(int quizId, bool includeOptions = true, CancellationToken ct = default)
        {
            IQueryable<Question> q = _db.Questions.Where(x => x.QuizId == quizId);

            if (includeOptions)
                q = q.Include(x => x.Options);

            return await q.AsNoTracking().ToListAsync(ct);
        }
    }
}