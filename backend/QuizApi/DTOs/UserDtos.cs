namespace QuizApi.DTOs
{
    public record RegisterDto(string Username, string Password);
    public record LoginDto(string Username, string Password, bool RememberMe = false);
    public record CurrentUserDto(string Id, string UserName, string? Email);
    public record UpdateUsernameDto(string NewUsername);
    public record UpdatePasswordDto(string CurrentPassword, string NewPassword);
    public record DeleteAccountDto(string CurrentPassword);
}