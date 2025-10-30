using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using QuizApp.DAL.Interfaces;
using QuizApp.Models;

namespace QuizApp.DAL.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly QuizDBContext _db;
        protected readonly DbSet<T> _set;

        public GenericRepository(QuizDBContext db)
        {
            _db = db;
            _set = db.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _set.FindAsync(new object?[] { id }, ct);

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
            => await _set.AsNoTracking().ToListAsync(ct);

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
            => await _set.AsNoTracking().Where(predicate).ToListAsync(ct);

        public virtual async Task AddAsync(T entity, CancellationToken ct = default)
            => await _set.AddAsync(entity, ct);

        public virtual void Update(T entity) => _set.Update(entity);
        public virtual void Remove(T entity) => _set.Remove(entity);
    }
}