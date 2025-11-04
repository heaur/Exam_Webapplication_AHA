namespace QuizApi.DTOs
{
    // DTOs for User entity
    // Create, Read, and Update operations
    public record UserCreateDto(string Username, string Password);
    public record UserReadDto(int Id, string Username, DateTime CreatedAt);
    public record UserUpdateDto(string Username, string Password);
}