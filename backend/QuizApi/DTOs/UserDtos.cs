namespace QuizApi.DTOs
{
    // DTO used during user registration.
    // We now fully support usernames used for display in navbar and profile.
    // Username must be max 8 characters (validated in controller).
    public record RegisterDto(
        string Username,      // Display username (IdentityUser.UserName)
        string Password,      // Login password
        string? Email = null  // Optional email field (depending on your app)
    );

    // DTO used for login requests.
    // Your project logs in with username + password (not email).
    public record LoginDto(
        string Username,
        string Password,
        bool RememberMe = false
    );

    // DTO returned when fetching the currently logged-in user.
    // This is what ProfilePage and AuthContext use.
    public record CurrentUserDto(
        string Id,            // IdentityUser.Id
        string UserName,      // IdentityUser.UserName (display username)
        string? Email         // Optional email
    );

    // DTO used for updating username inside the profile page.
    public record UpdateUsernameDto(
        string NewUsername     // Must follow same 8-char rule
    );

    // DTO used for updating user password.
    public record UpdatePasswordDto(
        string CurrentPassword,
        string NewPassword
    );

    // DTO used when deleting an account.
    public record DeleteAccountDto(
        string CurrentPassword
    );
}
