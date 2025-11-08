namespace QuizApi.DTOs
{
    // User registration DTO
    public record RegisterDto(string Username, string Password);

    // User login DTO
    public record LoginDto(string Username, string Password, bool RememberMe = false);

    // Current user DTO
    public record CurrentUserDto(string Id, string UserName, string? Email);

    // Update username DTO
    public record UpdateUsernameDto(string NewUsername);

    // Update password DTO
    public record UpdatePasswordDto(string CurrentPassword, string NewPassword);

    // Delete account DTO
    public record DeleteAccountDto(string CurrentPassword);
}