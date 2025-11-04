using QuizApi.Application.Interfaces;
using QuizApi.DAL.Interfaces;
using QuizApi.DTOs;
using QuizApi.Domain;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace QuizApi.Application.Services;

public class OptionService : IOptionService
{
    // Dependencies
    private readonly IGenericRepository<Option> _options;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<OptionService> _logger;
    private readonly IQuestionRepository _questions;

    // Constructor
    public OptionService(
        IGenericRepository<Option> options,
        IUnitOfWork uow,
        ILogger<OptionService> logger,
        IQuestionRepository questions
    )
    {
        _options = options;  // Injected generic repository for Option entity
        _uow = uow;  // Injected Unit of Work for transaction management
        _logger = logger; // Injected logger for logging operations
        _questions = questions; // Injected repository for Question entity
    }

    // Methods
    public async Task<OptionReadDto?> GetByIdAsync(int optionId, CancellationToken ct = default)
    {
        var entity = await _options.GetByIdAsync(optionId, ct);
        return entity is null
            ? null
            : new OptionReadDto(entity.OptionID, entity.Text, entity.IsCorrect, entity.QuestionId);
    }
    public async Task<IEnumerable<OptionReadDto>> GetByQuestionIdAsync(int questionId, CancellationToken ct = default)
    {
        if (questionId <= 0) throw new ArgumentException("Invalid question id");

        var all = await _options.GetAllAsync(ct);
        var list = all.Where(o => o.QuestionId == questionId);

        return list.Select(o => new OptionReadDto(o.OptionID, o.Text, o.IsCorrect, o.QuestionId));
    }


    public async Task<OptionReadDto> CreateAsync(OptionCreateDto dto, CancellationToken ct = default)
    {
        ValidateCreate(dto);


        var question = await _questions.GetByIdAsync(dto.QuestionId, ct);
        if (question is null) throw new KeyNotFoundException($"Question {dto.QuestionId} not found");

        var entity = new Option
        {
            Text = dto.Text.Trim(),
            IsCorrect = dto.IsCorrect,
            QuestionId = dto.QuestionId
        };

        await _options.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Created option {OptionId} for question {QuestionId}", entity.OptionID, entity.QuestionId);

        return new OptionReadDto(entity.OptionID, entity.Text, entity.IsCorrect, entity.QuestionId);
    }

    public async Task<OptionReadDto> UpdateAsync(int id, OptionUpdateDto dto, CancellationToken ct = default)
    {
        ValidateUpdate(dto);

        var entity = await _options.GetByIdAsync(id, ct)
                     ?? throw new KeyNotFoundException($"Option {id} not found");

        entity.Text = dto.Text.Trim();
        entity.IsCorrect = dto.IsCorrect;

        _options.Update(entity);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Updated option {OptionId}", id);

        return new OptionReadDto(entity.OptionID, entity.Text, entity.IsCorrect, entity.QuestionId);
    }

    public async Task<bool> DeleteAsync(int optionId, CancellationToken ct = default)
    {
        var entity = await _options.GetByIdAsync(optionId, ct);
        if (entity is null) return false;

        _options.Remove(entity);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted option {OptionId}", optionId);
        return true;
    }


    private static void ValidateCreate(OptionCreateDto dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        if (string.IsNullOrWhiteSpace(dto.Text)) throw new ArgumentException("Text is required");
        if (dto.QuestionId <= 0) throw new ArgumentException("QuestionId must be > 0");
    }

    private static void ValidateUpdate(OptionUpdateDto dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        if (string.IsNullOrWhiteSpace(dto.Text)) throw new ArgumentException("Text is required");
    }
}


