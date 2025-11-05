using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuizApi.DTOs;

namespace QuizApi.Application.Interfaces
{
    public interface IOptionService
    {
        // READ single option by id
        Task<OptionReadDto?> GetByIdAsync(int optionId, CancellationToken ct = default);

        // READ list of options by question id
        Task<IReadOnlyList<OptionReadDto>> GetByQuestionIdAsync(int questionId, CancellationToken ct = default);

        // CREATE
        Task<OptionReadDto> CreateAsync(OptionCreateDto dto, CancellationToken ct = default);

        // UPDATE
        Task<OptionReadDto> UpdateAsync(int id, OptionUpdateDto dto, CancellationToken ct = default);

        // DELETE
        Task<bool> DeleteAsync(int optionId, CancellationToken ct = default);
    }
}

