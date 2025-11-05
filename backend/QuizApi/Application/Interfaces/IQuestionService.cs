using QuizApi.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace QuizApi.Application.Interfaces
{
    public interface IQuestionService
    {
        // CREATE
        Task<QuestionReadDto> CreateAsync(QuestionCreateDto dto, CancellationToken ct = default);

        // READ single object by id
        Task<QuestionReadDto?> GetByIdAsync(int questionId, CancellationToken ct = default);

        // READ list by quiz id with pagination
        Task<IReadOnlyList<QuestionReadDto>> ListByQuizAsync(
            int quizId,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default);

        Task<int> CountByQuizAsync(int quizId, CancellationToken ct = default);

        // UPDATE
        Task<bool> UpdateAsync(int questionId, QuestionUpdateDto dto, CancellationToken ct = default);

        // DELETE
        Task<bool> DeleteAsync(int questionId, CancellationToken ct = default);
    }
}
