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
    public class QuestionService : IQuestionService
    {
        // Dependencies
        private readonly IGenericRepository<Question> _questions;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<QuestionService> _logger;

        // Constructor
        public QuestionService(
            IGenericRepository<Question> questions,
            IUnitOfWork uow,
            ILogger<QuestionService> logger)
        {
            _questions = questions; // Injected generic repository for Question entity
            _uow = uow; // Injected Unit of Work for transaction management
            _logger = logger; // Injected logger for logging operations
        }

        // CREATE
        public async Task<QuestionReadDto> CreateAsync(QuestionCreateDto dto, CancellationToken ct = default)
        {
            ValidateCreate(dto);

            var entity = new Question
            {
                QuizId = dto.QuizId,     
                Text   = dto.Text.Trim()
            };

            await _questions.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Created question {QuestionId} for quiz {QuizId}", entity.QuestionId, entity.QuizId);

            return new QuestionReadDto(entity.QuestionId, entity.Text, entity.QuizId); 
            // Return the created question as QuestionReadDto
        }

        // READ single object by id
        public async Task<QuestionReadDto?> GetByIdAsync(int questionId, CancellationToken ct = default)
        {
            if (questionId <= 0) throw new ArgumentException("Invalid question id", nameof(questionId));

            var entity = await _questions.GetByIdAsync(questionId, ct);
            return entity is null
                ? null
                : new QuestionReadDto(entity.QuestionId, entity.Text, entity.QuizId);
                // Return the found question as QuestionReadDto
        }

        // READ list by quiz + pagination
        public async Task<IReadOnlyList<QuestionReadDto>> ListByQuizAsync(
            int quizId,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default)
        {
            if (quizId <= 0) throw new ArgumentException("Invalid quiz id", nameof(quizId));
            if (page <= 0) throw new ArgumentOutOfRangeException(nameof(page), "Page must be >= 1");
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize), "PageSize must be >= 1");

            var all = await _questions.GetAllAsync(ct);

            var items = all
                .Where(q => q.QuizId == quizId)
                .OrderBy(q => q.QuestionId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(q => new QuestionReadDto(q.QuestionId, q.Text, q.QuizId))
                .ToList();

            return items; // Return the paginated list of QuestionReadDto
        }

        public async Task<int> CountByQuizAsync(int quizId, CancellationToken ct = default)
        {
            if (quizId <= 0) throw new ArgumentException("Invalid quiz id", nameof(quizId));

            var all = await _questions.GetAllAsync(ct);
            return all.Count(q => q.QuizId == quizId);
        }

        // UPDATE 
        public async Task<bool> UpdateAsync(int questionId, QuestionUpdateDto dto, CancellationToken ct = default)
        {
            if (questionId <= 0) throw new ArgumentException("Invalid question id", nameof(questionId));
            ValidateUpdate(dto);

            var entity = await _questions.GetByIdAsync(questionId, ct);
            if (entity is null) return false;

            entity.Text = dto.Text.Trim();

            _questions.Update(entity);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Updated question {QuestionId}", questionId);
            return true; // Return true if update was successful
        }

        // DELETE
        public async Task<bool> DeleteAsync(int questionId, CancellationToken ct = default)
        {
            if (questionId <= 0) throw new ArgumentException("Invalid question id", nameof(questionId));

            var entity = await _questions.GetByIdAsync(questionId, ct);
            if (entity is null) return false;

            _questions.Remove(entity);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Deleted question {QuestionId}", questionId);
            return true; // Return true if deletion was successful
        }

        // Validation for create
        private static void ValidateCreate(QuestionCreateDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Text)) throw new ArgumentException("Text is required", nameof(dto.Text));
            if (dto.QuizId <= 0) throw new ArgumentException("QuizSetId must be > 0", nameof(dto.QuizId));
        }

        // Validation for update
        private static void ValidateUpdate(QuestionUpdateDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Text)) throw new ArgumentException("Text is required", nameof(dto.Text));
        }
    }
}
