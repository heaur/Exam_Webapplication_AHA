using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuizApi.DTOs;

namespace QuizApi.Application.Interfaces
{
    public interface IUserService
    {
        // CREATE
        Task<UserReadDto> CreateAsync(UserCreateDto dto, CancellationToken ct = default);

        // READ single
        Task<UserReadDto?> GetByIdAsync(int userId, CancellationToken ct = default);

        // LIST all users
        Task<IReadOnlyList<UserReadDto>> ListAsync(
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default);

        Task<int> CountAsync(CancellationToken ct = default);

        // UPDATE
        Task<UserReadDto?> UpdateAsync(int userId, UserUpdateDto dto, CancellationToken ct = default);

        // DELETE
        Task<bool> DeleteAsync(int userId, CancellationToken ct = default);
    }
}
