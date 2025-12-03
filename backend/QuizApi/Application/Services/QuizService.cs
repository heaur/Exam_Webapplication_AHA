using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuizApi.Application.Interfaces;
using QuizApi.DAL.Interfaces;
using QuizApi.Domain;
using QuizApi.DTOs;

namespace QuizApi.Application.Services
{
    public class QuizService : IQuizService
    {
        private readonly IGenericRepository<Quiz> _quizzes;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<QuizService> _logger;

        public QuizService(
            IGenericRepository<Quiz> quizzes,
            IUnitOfWork uow,
            ILogger<QuizService> logger)
        {
            _quizzes = quizzes;
            _uow = uow;
            _logger = logger;
        }

        // CREATE
        // Creates a new quiz from the incoming DTO and returns a read model.
        public async Task<QuizReadDto> CreateAsync(QuizCreateDto dto, CancellationToken ct = default)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Title cannot be empty", nameof(dto.Title));

            // Normalize subject code so grouping on the homepage is consistent.
            // If missing, we fall back to "OTHER".
            var subjectCode = string.IsNullOrWhiteSpace(dto.SubjectCode)
                ? "OTHER"
                : dto.SubjectCode.Trim().ToUpper();

            var now = DateTime.UtcNow;

            var entity = new Quiz
            {
                Title       = dto.Title.Trim(),
                SubjectCode = subjectCode,
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                ImageUrl    = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim(),
                CreatedAt   = now,
                UpdatedAt   = null,
                IsPublished = false,
                PublishedAt = null
                // OwnerId can be set higher up in the stack if you want to track owners here as well.
            };

            await _quizzes.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Created quiz {QuizId}", entity.QuizId);

            return ToDto(entity);
        }

        // READ: single quiz by id
        public async Task<QuizReadDto?> GetByIdAsync(int quizId, CancellationToken ct = default)
        {
            if (quizId <= 0) throw new ArgumentException("Invalid quiz id", nameof(quizId));

            var entity = await _quizzes.GetByIdAsync(quizId, ct);
            return entity is null ? null : ToDto(entity);
        }

        // LIST: with pagination and optional filters
        // Used by any consumer that needs a paged list of quizzes (e.g. admin views).
        public async Task<IReadOnlyList<QuizReadDto>> ListAsync(
            int page = 1,
            int pageSize = 20,
            string? search = null,
            string? ownerId = null,
            bool? isPublished = null,
            CancellationToken ct = default)
        {
            if (page <= 0) throw new ArgumentOutOfRangeException(nameof(page), "Page must be >= 1");
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize), "PageSize must be >= 1");

            var all = await _quizzes.GetAllAsync(ct);
            var query = all.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(q =>
                    q.Title != null &&
                    q.Title.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(ownerId))
                query = query.Where(q => q.OwnerId != null && q.OwnerId == ownerId);

            if (isPublished is not null)
                query = query.Where(q => q.IsPublished == isPublished);

            var items = query
                .OrderBy(q => q.QuizId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ToDto)
                .ToList();

            return items;
        }

        // COUNT: useful for paging metadata
        public async Task<int> CountAsync(
            string? search = null,
            string? ownerId = null,
            bool? isPublished = null,
            CancellationToken ct = default)
        {
            var all = await _quizzes.GetAllAsync(ct);
            var query = all.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(q =>
                    q.Title != null &&
                    q.Title.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(ownerId))
                query = query.Where(q => q.OwnerId != null && q.OwnerId == ownerId);

            if (isPublished is not null)
                query = query.Where(q => q.IsPublished == isPublished);

            return query.Count();
        }

        // UPDATE
        // Updates quiz metadata and returns the updated read model.
        public async Task<QuizReadDto?> UpdateAsync(int quizId, QuizUpdateDto dto, CancellationToken ct = default)
        {
            if (quizId <= 0) throw new ArgumentException("Invalid quiz id", nameof(quizId));
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Title cannot be empty", nameof(dto.Title));

            var entity = await _quizzes.GetByIdAsync(quizId, ct);
            if (entity is null) return null;

            var subjectCode = string.IsNullOrWhiteSpace(dto.SubjectCode)
                ? "OTHER"
                : dto.SubjectCode.Trim().ToUpper();

            entity.Title       = dto.Title.Trim();
            entity.SubjectCode = subjectCode;
            entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            entity.ImageUrl    = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim();

            if (dto.IsPublished is not null && entity.IsPublished != dto.IsPublished.Value)
            {
                entity.IsPublished = dto.IsPublished.Value;
                entity.PublishedAt = dto.IsPublished.Value ? DateTime.UtcNow : null;
            }

            entity.UpdatedAt = DateTime.UtcNow;

            _quizzes.Update(entity);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Updated quiz {QuizId}", entity.QuizId);
            return ToDto(entity);
        }

        // PUBLISH
        public async Task<bool> PublishAsync(int quizId, CancellationToken ct = default)
        {
            if (quizId <= 0) throw new ArgumentException("Invalid quiz id", nameof(quizId));

            var entity = await _quizzes.GetByIdAsync(quizId, ct);
            if (entity is null) return false;

            if (!entity.IsPublished)
            {
                entity.IsPublished = true;
                entity.PublishedAt = DateTime.UtcNow;
                entity.UpdatedAt   = DateTime.UtcNow;
                _quizzes.Update(entity);
                await _uow.SaveChangesAsync(ct);
            }

            _logger.LogInformation("Published quiz {QuizId}", quizId);
            return true;
        }

        // UNPUBLISH
        public async Task<bool> UnpublishAsync(int quizId, CancellationToken ct = default)
        {
            if (quizId <= 0) throw new ArgumentException("Invalid quiz id", nameof(quizId));

            var entity = await _quizzes.GetByIdAsync(quizId, ct);
            if (entity is null) return false;

            if (entity.IsPublished)
            {
                entity.IsPublished = false;
                entity.PublishedAt = null;
                entity.UpdatedAt   = DateTime.UtcNow;
                _quizzes.Update(entity);
                await _uow.SaveChangesAsync(ct);
            }

            _logger.LogInformation("Unpublished quiz {QuizId}", quizId);
            return true;
        }

        // DELETE
        public async Task<bool> DeleteAsync(int quizId, CancellationToken ct = default)
        {
            if (quizId <= 0) throw new ArgumentException("Invalid quiz id", nameof(quizId));

            var entity = await _quizzes.GetByIdAsync(quizId, ct);
            if (entity is null) return false;

            _quizzes.Remove(entity);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Deleted quiz {QuizId}", quizId);
            return true;
        }

        // Mapping helper: converts a Quiz entity into a QuizReadDto.
        // This is used from multiple places (create, update, list, get).
        private static QuizReadDto ToDto(Quiz q)
        {
            int questionCount = q.Questions?.Count ?? 0;

            return new QuizReadDto(
                q.QuizId,
                q.Title,
                q.SubjectCode,
                q.Description,
                q.ImageUrl,
                q.CreatedAt,
                q.UpdatedAt,
                q.IsPublished,
                q.PublishedAt,
                q.OwnerId,
                questionCount
            );
        }
    }
}
