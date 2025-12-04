using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuizApi.DAL;
using QuizApi.Domain;
using QuizApi.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace QuizApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public UserController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<UserController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    // REGISTER / LOGIN / LOGOUT

    // POST /api/user/register
    // Creates a new Identity user and signs them in.
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromBody] RegisterDto dto,
        CancellationToken ct)
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(dto.Username))
        {
            ModelState.AddModelError(nameof(dto.Username), "Username is required.");
        }
        else
        {
            var trimmed = dto.Username.Trim();

            // Enforce max 8 characters for display username
            if (trimmed.Length > 8)
            {
                ModelState.AddModelError(nameof(dto.Username), "Username cannot be longer than 8 characters.");
            }
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            ModelState.AddModelError(nameof(dto.Password), "Password is required.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var username = dto.Username.Trim();

        // Optional: pre-check if username is available (nicer error than Identity)
        var existing = await _userManager.FindByNameAsync(username);
        if (existing is not null)
        {
            ModelState.AddModelError(nameof(dto.Username), "Username is already taken.");
            return ValidationProblem(ModelState);
        }

        // Create ApplicationUser.
        // We do NOT abuse Username as email anymore.
        var user = new ApplicationUser
        {
            UserName = username,
            Email = string.IsNullOrWhiteSpace(dto.Email)
                ? null
                : dto.Email.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
            {
                ModelState.AddModelError(err.Code, err.Description);
            }

            // 400 with Identity validation errors
            return ValidationProblem(ModelState);
        }

        // Auto sign-in after registration (cookie-based auth)
        await _signInManager.SignInAsync(user, isPersistent: false);

        var response = new CurrentUserDto(
            Id: user.Id,
            UserName: user.UserName ?? string.Empty,
            Email: user.Email
        );

        // 201 Created + current user info
        return CreatedAtAction(nameof(Me), null, response);
    }

    // POST /api/user/login
    // Log in existing user (cookie-based).
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginDto dto,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return ValidationProblem(
                new ValidationProblemDetails(
                    new Dictionary<string, string[]>
                    {
                        ["credentials"] = new[] { "Username and password are required." }
                    }
                )
            );
        }

        var username = dto.Username.Trim();

        // PasswordSignInAsync creates/updates the auth cookie
        var result = await _signInManager.PasswordSignInAsync(
            username,
            dto.Password,
            dto.RememberMe,
            lockoutOnFailure: true
        );

        if (result.Succeeded)
        {
            // 204: logged in, cookie set
            return NoContent();
        }

        if (result.IsLockedOut)
        {
            // 403: user is locked out
            return Forbid();
        }

        if (result.IsNotAllowed)
        {
            // 401: maybe email not confirmed, etc.
            return Unauthorized();
        }

        // 401: wrong credentials
        return Unauthorized();
    }

    // POST /api/user/logout
    // Log out current user.
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await _signInManager.SignOutAsync();
        // 204: cookie cleared
        return NoContent();
    }

    // PROFILE / ACCOUNT

    // GET /api/user/me
    // Returns minimal info about the current user.
    // This is what the frontend uses in AuthContext + navbar.
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<CurrentUserDto>> Me(CancellationToken ct)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var dto = new CurrentUserDto(
            Id: user.Id,
            UserName: user.UserName ?? string.Empty,
            Email: user.Email
        );

        return Ok(dto);
    }

    // PUT /api/user/username
    // Change username for the current user.
    [HttpPut("username")]
    [Authorize]
    public async Task<IActionResult> UpdateUsername(
        [FromBody] UpdateUsernameDto dto,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.NewUsername))
        {
            return ValidationProblem(
                new ValidationProblemDetails(
                    new Dictionary<string, string[]>
                    {
                        [nameof(dto.NewUsername)] = new[] { "New username is required." }
                    }
                )
            );
        }

        var newName = dto.NewUsername.Trim();

        // Enforce same 8-char rule as registration
        if (newName.Length > 8)
        {
            return ValidationProblem(
                new ValidationProblemDetails(
                    new Dictionary<string, string[]>
                    {
                        [nameof(dto.NewUsername)] = new[] { "Username cannot be longer than 8 characters." }
                    }
                )
            );
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        // Check if username is available
        var existing = await _userManager.FindByNameAsync(newName);
        if (existing is not null && existing.Id != user.Id)
        {
            return Conflict(new { message = "Username already taken." });
        }

        user.UserName = newName;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
            {
                ModelState.AddModelError(err.Code, err.Description);
            }

            return ValidationProblem(ModelState);
        }

        // Refresh sign-in to update auth cookie with new username
        await _signInManager.RefreshSignInAsync(user);

        // 204, no body needed
        return NoContent();
    }

    // PUT /api/user/password
    // Change current user's password. Requires current password.
    [HttpPut("password")]
    [Authorize]
    public async Task<IActionResult> UpdatePassword(
        [FromBody] UpdatePasswordDto dto,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.CurrentPassword) ||
            string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            return ValidationProblem(
                new ValidationProblemDetails(
                    new Dictionary<string, string[]>
                    {
                        [nameof(dto.NewPassword)] = new[] { "New password is required." }
                    }
                )
            );
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
            {
                ModelState.AddModelError(err.Code, err.Description);
            }

            return ValidationProblem(ModelState);
        }

        // Keep user logged in with new password
        await _signInManager.RefreshSignInAsync(user);
        return NoContent();
    }

    // DELETE /api/user
    // Deletes the current user's account (asks for password confirmation).
    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteAccount(
        [FromBody] DeleteAccountDto dto,
        CancellationToken ct)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        // Verify password before deleting account
        var passwordOk = await _userManager.CheckPasswordAsync(user, dto.CurrentPassword);
        if (!passwordOk)
        {
            return Unauthorized();
        }

        // Delete account after explicit password confirmation
        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
            {
                ModelState.AddModelError(err.Code, err.Description);
            }

            return ValidationProblem(ModelState);
        }

        await _signInManager.SignOutAsync();
        return NoContent();
    }

    // UTILITY ENDPOINTS

    // GET /api/user/check-username?username=alice
    // Quick availability check for UI when user picks a username.
    [HttpGet("check-username")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckUsername(
        [FromQuery] string username,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return BadRequest(new { message = "username is required." });
        }

        var trimmed = username.Trim();

        // Enforce the same 8-character rule here as well
        if (trimmed.Length > 8)
        {
            return BadRequest(new { message = "Username cannot be longer than 8 characters." });
        }

        var user = await _userManager.FindByNameAsync(trimmed);
        return Ok(new { available = user is null });
    }

    // POST /api/user/refresh
    // Re-issues the auth cookie to extend session.
    [HttpPost("refresh")]
    [Authorize]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        // Extend session by renewing auth cookie
        await _signInManager.RefreshSignInAsync(user);
        return NoContent();
    }
}
