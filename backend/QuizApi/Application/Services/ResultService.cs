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
    public class ResultService : IResultService
    {
        private readonly IResultRepository _results;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<ResultService> _logger;

        public ResultService(
            IResultRepository resultRepository,
            IUnitOfWork uow,
            ILogger<ResultService> logger)
        {
            _results = resultRepository;
            _uow = uow;
            _logger = logger;
        }

        // CREATE
        public async Task<ResultReadDto> CreateAsync(ResultCreateDto dto, CancellationToken ct = default)
        {
            ValidateCreate(dto);

            // Enkelt oppsett: Score -> CorrectCount, TotalQuestions settes minst 1
            var entity = new Result
            {
                UserId         = dto.UserId,
                QuizId         = dto.QuizId,
                CorrectCount   = dto.Score,
                TotalQuestions = Math.Max(dto.Score, 1),
                CompletedAt    = DateTime.UtcNow
            };

            await _results.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Created result {ResultId} for user {UserId} on quiz {QuizId}",
                entity.ResultId, entity.UserId, entity.QuizId);

            return ToReadDto(entity);
        }

        // READ single
        public async Task<ResultReadDto?> GetByIdAsync(int resultId, CancellationToken ct = default)
        {
            if (resultId <= 0) throw new ArgumentException("Invalid result id", nameof(resultId));

            var entity = await _results.GetByIdAsync(resultId, ct);
            return entity is null ? null : ToReadDto(entity);
        }

        // LIST by user
        public async Task<IReadOnlyList<ResultReadDto>> ListByUserAsync(
            int userId,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user id", nameof(userId));
            if (page <= 0) throw new ArgumentOutOfRangeException(nameof(page), "Page must be >= 1");
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize), "PageSize must be >= 1");

            var all = await _results.GetAllAsync(ct);

            var items = all
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CompletedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ToReadDto)
                .ToList();

            return items;
        }

        public async Task<int> CountByUserAsync(int userId, CancellationToken ct = default)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user id", nameof(userId));

            var all = await _results.GetAllAsync(ct);
            return all.Count(r => r.UserId == userId);
        }

        // LIST by quiz
        public async Task<IReadOnlyList<ResultReadDto>> ListByQuizAsync(
            int quizId,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default)
        {
            if (quizId <= 0) throw new ArgumentException("Invalid quiz id", nameof(quizId));
            if (page <= 0) throw new ArgumentOutOfRangeException(nameof(page), "Page must be >= 1");
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize), "PageSize must be >= 1");

            var all = await _results.GetAllAsync(ct);

            var items = all
                .Where(r => r.QuizId == quizId)
                .OrderByDescending(r => r.CompletedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ToReadDto)
                .ToList();

            return items;
        }

        public async Task<int> CountByQuizAsync(int quizId, CancellationToken ct = default)
        {
            if (quizId <= 0) throw new ArgumentException("Invalid quiz id", nameof(quizId));

            var all = await _results.GetAllAsync(ct);
            return all.Count(r => r.QuizId == quizId);
        }

        // DELETE
        public async Task<bool> DeleteAsync(int resultId, CancellationToken ct = default)
        {
            if (resultId <= 0) throw new ArgumentException("Invalid result id", nameof(resultId));

            var entity = await _results.GetByIdAsync(resultId, ct);
            if (entity is null) return false;

            _results.Remove(entity);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Deleted result {ResultId}", resultId);
            return true;
        }

        // --- Helpers ---

        private static void ValidateCreate(ResultCreateDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (dto.UserId <= 0) throw new ArgumentException("UserId must be > 0", nameof(dto.UserId));
            if (dto.QuizId <= 0) throw new ArgumentException("QuizId must be > 0", nameof(dto.QuizId));
            if (dto.Score < 0) throw new ArgumentException("Score cannot be negative", nameof(dto.Score));
        }

        private static ResultReadDto ToReadDto(Result r)
            => new ResultReadDto(r.ResultId, r.UserId, r.QuizId, r.CorrectCount, r.CompletedAt);
    }
}
