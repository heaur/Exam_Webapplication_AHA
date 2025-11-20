using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuizApi.DTOs;

namespace QuizApi.Application.Interfaces
{
    public interface IResultService
    {
        // CREATE
        Task<ResultReadDto> CreateAsync(ResultCreateDto dto, CancellationToken ct = default);

        // READ single
        Task<ResultReadDto?> GetByIdAsync(int resultId, CancellationToken ct = default);

        // LIST by user
        Task<IReadOnlyList<ResultReadDto>> ListByUserAsync(
            string userId,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default);

        Task<int> CountByUserAsync(string userId, CancellationToken ct = default);

        // LIST by quiz
        Task<IReadOnlyList<ResultReadDto>> ListByQuizAsync(
            int quizId,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default);

        Task<int> CountByQuizAsync(int quizId, CancellationToken ct = default);

        // DELETE, if needed
        Task<bool> DeleteAsync(int resultId, CancellationToken ct = default);
    }
}
