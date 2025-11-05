using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuizApi.DTOs;

namespace QuizApi.Application.Interfaces
{
    public interface IQuizService
    {
        // CREATE
        Task<QuizReadDto> CreateAsync(QuizCreateDto dto, CancellationToken ct = default);

        // READ single object by id
        Task<QuizReadDto?> GetByIdAsync(int quizId, CancellationToken ct = default);

        // LIST with pagination and optional filters
        Task<IReadOnlyList<QuizReadDto>> ListAsync(
            int page = 1,
            int pageSize = 20,
            string? search = null,      // Search by title or description
            int? ownerId = null,        // or other filter dimension
            bool? isPublished = null,   // filter by publication status
            CancellationToken ct = default);

        Task<int> CountAsync(
            string? search = null,
            int? ownerId = null,
            bool? isPublished = null,
            CancellationToken ct = default);

        // UPDATE 
        Task<QuizReadDto?> UpdateAsync(int quizId, QuizUpdateDto dto, CancellationToken ct = default);

        // PUBLISH / UNPUBLISH
        Task<bool> PublishAsync(int quizId, CancellationToken ct = default);
        Task<bool> UnpublishAsync(int quizId, CancellationToken ct = default);

        // DELETE
        Task<bool> DeleteAsync(int quizId, CancellationToken ct = default);

    }
}
