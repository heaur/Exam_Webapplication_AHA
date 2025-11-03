using QuizApi.Application.Interfaces;
using QuizApi.DAL.Interfaces;
using QuizApi.DTOs;
using QuizApi.Domain;

namespace QuizApi.Application.Services;

public class OptionService : IOptionService
{
    private readonly IGenericRepository<Option> _options;
    private readonly IUnitOfWork _uow; 

    public OptionService(IGenericRepository<Option> options)
    {
        _options = options;
    }

    public async Task<OptionReadDto?> GetByIdAsync(int optionId, CancellationToken ct = default)
    {
        var option = await _options.GetByIdAsync(optionId, ct);
        if (option == null) return null;

        return new OptionReadDto(option.OptionID, option.Text, option.IsCorrect, option.QuestionId);
    }

    public async Task<IEnumerable<OptionReadDto>> GetByQuestionIdAsync(int questionId, CancellationToken ct = default)
    {
        var all = await _options.GetAllAsync(ct);
        return all
            .Where(o => o.QuestionId == questionId)
            .Select(o => new OptionReadDto(o.OptionID, o.Text, o.IsCorrect, o.QuestionId))
            .ToList();
    }

    public async Task<OptionReadDto> CreateAsync(OptionCreateDto dto, CancellationToken ct = default)
    {
        var entity = new Option
        {
            Text = dto.Text,
            IsCorrect = dto.IsCorrect,
            QuestionId = dto.QuestionId
        };

        await _options.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return new OptionReadDto(entity.OptionID, entity.Text, entity.IsCorrect, entity.QuestionId);
    }

    public async Task<OptionReadDto> UpdateAsync(int id, OptionUpdateDto dto, CancellationToken ct = default)
    {
        var option = await _options.GetByIdAsync(id, ct);
        if (option == null)
            throw new KeyNotFoundException($"Option with id {id} not found");

        option.Text = dto.Text;
        option.IsCorrect = dto.IsCorrect;

        _options.Update(option);
        await _uow.SaveChangesAsync(ct);

        return new OptionReadDto(option.OptionID, option.Text, option.IsCorrect, option.QuestionId);
    }

    public async Task<bool> DeleteAsync(int optionId, CancellationToken ct = default)
    {
        var option = await _options.GetByIdAsync(optionId, ct);
        if (option == null) return false;

        _options.Remove(option);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
