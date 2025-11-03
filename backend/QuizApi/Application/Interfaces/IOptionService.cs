namespace QuizApi.Application.Interfaces;

using QuizApi.DTOs;

public interface IOptionService
{
    Task<OptionReadDto?> GetByIdAsync(int optionId, CancellationToken ct = default);
    Task<IEnumerable<OptionReadDto>> GetByQuestionIdAsync(int questionId, CancellationToken ct = default);
    Task<OptionReadDto> CreateAsync(OptionCreateDto dto, CancellationToken ct = default);
    Task<OptionReadDto> UpdateAsync(int id, OptionUpdateDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int optionId, CancellationToken ct = default);
}
