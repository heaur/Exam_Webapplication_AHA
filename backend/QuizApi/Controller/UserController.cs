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

// CONTROLLER
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    // CONSTRUCTOR
    public UserController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ILogger<UserController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    // LOGIN / LOGOUT / REGISTER METHODS

    // POST /api/user/register
    // Creates a new Identity user and signs them in.
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Username))
            ModelState.AddModelError(nameof(dto.Username), "Username is required.");
        if (string.IsNullOrWhiteSpace(dto.Password))
            ModelState.AddModelError(nameof(dto.Password), "Password is required.");
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        // Uses the same value for UserName 
        var user = new IdentityUser
        {
            UserName = dto.Username.Trim(),
            Email = dto.Username.Contains("@") ? dto.Username.Trim() : null
        };

        // Check if username is available
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError(err.Code, err.Description);
            return ValidationProblem(ModelState);
            // Returns 400 with validation errors
        }

        // Auto sign-in after registration
        await _signInManager.SignInAsync(user, isPersistent: false);

        return CreatedAtAction(nameof(Me), null);
        // Returns 201 Created pointing to GET /api/user/me
    }

    // POST /api/user/login
    // Log in existing user.
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["credentials"] = new[] { "Username and password are required." }
            }));

        // Attempt sign-in
        var result = await _signInManager.PasswordSignInAsync(dto.Username.Trim(), dto.Password, dto.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded) return NoContent();                 // 204: logged in
        if (result.IsLockedOut) return Forbid();                  // 403: locked out
        if (result.IsNotAllowed) return Unauthorized();           // 401: e.g., not confirmed
        return Unauthorized();                                    // 401: wrong credentials
    }

    // POST /api/user/logout
    // Log out current user.
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await _signInManager.SignOutAsync();
        return NoContent();
        // Returns 204 No Content
    }



    // PROFILE/ACCOUNT METHODS

    // GET /api/user/me
    // Returns minimal info about the current user.
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<CurrentUserDto>> Me(CancellationToken ct)
    {
        // Get current user
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        return Ok(new CurrentUserDto(
            Id: user.Id,
            UserName: user.UserName ?? "",
            Email: user.Email
        ));
        // Returns 200 OK with current user info
    }

    // PUT /api/user/username
    // Change username for the current user.
    [HttpPut("username")]
    [Authorize]
    public async Task<IActionResult> UpdateUsername([FromBody] UpdateUsernameDto dto, CancellationToken ct)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(dto.NewUsername))
            return ValidationProblem(

        new ValidationProblemDetails(
        new Dictionary<string, string[]>
        {
            [nameof(dto.NewUsername)] = new[] { "New username is required." }
        }));

        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        // Check if username is available
        var existing = await _userManager.FindByNameAsync(dto.NewUsername.Trim());
        if (existing is not null)
            return Conflict(new { message = "Username already taken." });

        user.UserName = dto.NewUsername.Trim();
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError(err.Code, err.Description);
            return ValidationProblem(ModelState);
        }

        // Refresh sign-in to update cookie
        await _signInManager.RefreshSignInAsync(user);
        return NoContent();
        // Returns 204 No Content
    }

    // PUT /api/user/password
    // Change the current user's password. Requires current password.
    [HttpPut("password")]
    [Authorize]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.CurrentPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
            return ValidationProblem(
                new ValidationProblemDetails(
                    new Dictionary<string, string[]>
                    {
                        [nameof(dto.NewPassword)] = new[] { "New password is required." }
                    }
                )
            );


        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError(err.Code, err.Description);
            return ValidationProblem(ModelState);
        }

        await _signInManager.RefreshSignInAsync(user); // keep the user signed in
        return NoContent();
        // Returns 204 No Content
    }

    // DELETE /api/user
    // Deletes the current user's account (asks for password confirmation).
    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto dto, CancellationToken ct)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        // Verify password before delete
        if (!await _userManager.CheckPasswordAsync(user, dto.CurrentPassword))
            return Unauthorized();

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError(err.Code, err.Description);
            return ValidationProblem(ModelState);
        }

        await _signInManager.SignOutAsync();
        return NoContent();
    }
    

    // UTILITY METHODS

    // GET /api/user/check-username?username=alice
    // Quick availability check for UI.
    [HttpGet("check-username")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckUsername([FromQuery] string username, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(username))
            return BadRequest(new { message = "username is required." });

        var user = await _userManager.FindByNameAsync(username.Trim());
        return Ok(new { available = user is null });
    }

    // (Optional) POST /api/user/refresh
    // Re-issues the auth cookie to extend session
    [HttpPost("refresh")]
    [Authorize]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        await _signInManager.RefreshSignInAsync(user);
        return NoContent();
    }
}
