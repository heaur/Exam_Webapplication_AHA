namespace QuizApi.DTOs
{
    // DTO used during user registration.
    public record RegisterDto(string Username, string Password, string? Email = null);

    // DTO used for login requests.
    public record LoginDto(string Username, string Password, bool RememberMe = false);

    // DTO returned when fetching the currently logged-in user.
    public record CurrentUserDto(string Id, string UserName, string? Email);

    // DTO used for updating a username.
    public record UpdateUsernameDto(string NewUsername);

    // DTO used for updating user password.
    public record UpdatePasswordDto(string CurrentPassword, string NewPassword);

    // DTO used when deleting an account.
    public record DeleteAccountDto(string CurrentPassword);
}
