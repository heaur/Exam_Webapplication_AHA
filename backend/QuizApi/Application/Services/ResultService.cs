using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using QuizApi.Application.Interfaces;
using QuizApi.DAL.Interfaces;
using QuizApi.Domain;
using QuizApi.DTOs;

public class ResultService : IResultService
{
    // Dependencies
    private readonly IGenericRepository<Result> _results;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ResultService> _logger;
    private readonly IMapper _mapper; // AutoMapper for DTO <-> entity mapping

    // Constructor
    public ResultService(
        IGenericRepository<Result> resultRepository,
        IUnitOfWork uow,
        ILogger<ResultService> logger,
        IMapper mapper) // AutoMapper injected
    {
        _results = resultRepository; // Injected generic repository for Result entity
        _uow = uow; // Injected Unit of Work for transaction management
        _logger = logger; // Injected logger for logging operations
        _mapper = mapper; // Injected AutoMapper for DTO <-> entity mapping
    }

    // CREATE
    public async Task<ResultReadDto> CreateAsync(ResultCreateDto dto, CancellationToken ct = default)
    {
        ValidateCreate(dto);

        var entity = new Result
        {
            UserId         = dto.UserId,
            QuizId         = dto.QuizId,
            CorrectCount   = dto.CorrectCount,
            TotalQuestions = Math.Max(dto.CorrectCount, 1),
            CompletedAt    = DateTime.UtcNow
        };

        await _results.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Created result {ResultId} for user {UserId} on quiz {QuizId}",
            entity.ResultId, entity.UserId, entity.QuizId);

        return _mapper.Map<ResultReadDto>(entity); // Return the created result as ResultReadDto
    }

    // READ single object by id
    public async Task<ResultReadDto?> GetByIdAsync(int resultId, CancellationToken ct = default)
    {
        if (resultId <= 0) throw new ArgumentException("Invalid result id", nameof(resultId));
        var entity = await _results.GetByIdAsync(resultId, ct);
        return entity is null ? null : _mapper.Map<ResultReadDto>(entity);
        // Return the found result as ResultReadDto or null if not found
    }

    // LIST result(s) by user
    public async Task<IReadOnlyList<ResultReadDto>> ListByUserAsync(string userId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("Invalid user id", nameof(userId));
        if (page <= 0) throw new ArgumentOutOfRangeException(nameof(page), "Page must be >= 1");
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize), "PageSize must be >= 1");

        var all = await _results.GetAllAsync(ct);

        return all
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CompletedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => _mapper.Map<ResultReadDto>(r)) // Mapping via AutoMapper
            .ToList();
    }

    // COUNT result(s) by user
    public async Task<int> CountByUserAsync(string userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("Invalid user id", nameof(userId));
        var all = await _results.GetAllAsync(ct);
        return all.Count(r => r.UserId == userId);
    }

    // LIST result(s) by quiz
    public async Task<IReadOnlyList<ResultReadDto>> ListByQuizAsync(int quizId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        if (quizId <= 0) throw new ArgumentException("Invalid quiz id", nameof(quizId));
        if (page <= 0) throw new ArgumentOutOfRangeException(nameof(page), "Page must be >= 1");
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize), "PageSize must be >= 1");

        var all = await _results.GetAllAsync(ct);

        return all
            .Where(r => r.QuizId == quizId)
            .OrderByDescending(r => r.CompletedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => _mapper.Map<ResultReadDto>(r)) // <-- mapping via AutoMapper
            .ToList();
    }

    // COUNT result(s) by quiz
    public async Task<int> CountByQuizAsync(int quizId, CancellationToken ct = default)
    {
        if (quizId <= 0) throw new ArgumentException("Invalid quiz id", nameof(quizId));
        var all = await _results.GetAllAsync(ct);
        return all.Count(r => r.QuizId == quizId);
    }

    // DELETE, if needed
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

    // Validation for creating a result
    private static void ValidateCreate(ResultCreateDto dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        if (string.IsNullOrWhiteSpace(dto.UserId)) throw new ArgumentException("UserId must be provided", nameof(dto.UserId));
        if (dto.QuizId <= 0) throw new ArgumentException("QuizId must be > 0", nameof(dto.QuizId));
        if (dto.CorrectCount < 0) throw new ArgumentException("Score cannot be negative", nameof(dto.CorrectCount));
    }
}
