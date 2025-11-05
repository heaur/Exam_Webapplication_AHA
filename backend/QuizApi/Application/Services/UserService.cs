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

namespace QuizApi.Application.Services
{
    public class UserService : IUserService
    {
        // Dependencies
        private readonly IGenericRepository<User> _users;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<UserService> _logger;
        private readonly IMapper _mapper;

        // Constructor
        public UserService(
            IGenericRepository<User> users,
            IUnitOfWork uow,
            ILogger<UserService> logger,
            IMapper mapper)
        {
            _users = users;   // Injected generic repository for User entity
            _uow = uow;       // Injected Unit of Work for transaction management
            _logger = logger; // Injected logger for logging operations
            _mapper = mapper; // Injected AutoMapper for DTO <-> entity mapping
        }

        // CREATE
        public async Task<UserReadDto> CreateAsync(UserCreateDto dto, CancellationToken ct = default)
        {
            ValidateCreate(dto);

            // Hashing av passord kan legges til her senere
            var entity = _mapper.Map<User>(dto);

            await _users.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Created user {UserId}", entity.UserId);

            return _mapper.Map<UserReadDto>(entity); // Return created user as DTO
        }

        // READ single object by id
        public async Task<UserReadDto?> GetByIdAsync(int userId, CancellationToken ct = default)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user id", nameof(userId));

            var entity = await _users.GetByIdAsync(userId, ct);
            return entity is null ? null : _mapper.Map<UserReadDto>(entity);
        }

        // LIST all users
        public async Task<IReadOnlyList<UserReadDto>> ListAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            if (page <= 0) throw new ArgumentOutOfRangeException(nameof(page), "Page must be >= 1");
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize), "PageSize must be >= 1");

            var all = await _users.GetAllAsync(ct);

            return all
                .OrderBy(u => u.Username)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => _mapper.Map<UserReadDto>(u))
                .ToList();
        }

        // COUNT all users
        public async Task<int> CountAsync(CancellationToken ct = default)
        {
            var all = await _users.GetAllAsync(ct);
            return all.Count();
        }

        // UPDATE
        public async Task<UserReadDto?> UpdateAsync(int userId, UserUpdateDto dto, CancellationToken ct = default)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user id", nameof(userId));
            ValidateUpdate(dto);

            var entity = await _users.GetByIdAsync(userId, ct);
            if (entity is null) return null;

            // Oppdater feltene
            entity.Username = dto.Username.Trim();
            entity.Password = dto.Password.Trim();

            _users.Update(entity);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Updated user {UserId}", userId);

            return _mapper.Map<UserReadDto>(entity);
        }

        // DELETE
        public async Task<bool> DeleteAsync(int userId, CancellationToken ct = default)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user id", nameof(userId));

            var entity = await _users.GetByIdAsync(userId, ct);
            if (entity is null) return false;

            _users.Remove(entity);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Deleted user {UserId}", userId);
            return true;
        }

        // Validation for creating a user
        private static void ValidateCreate(UserCreateDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Username)) throw new ArgumentException("Username is required", nameof(dto.Username));
            if (string.IsNullOrWhiteSpace(dto.Password)) throw new ArgumentException("Password is required", nameof(dto.Password));
        }

        // Validation for updating a user
        private static void ValidateUpdate(UserUpdateDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Username)) throw new ArgumentException("Username is required", nameof(dto.Username));
            if (string.IsNullOrWhiteSpace(dto.Password)) throw new ArgumentException("Password is required", nameof(dto.Password));
        }
    }
}
