// src/auth/types/auth.ts
// ----------------------
// Defines the shapes of the objects used when a user logs in or registers.
// These interfaces MUST match what the ASP.NET backend expects on its DTOs.

// Used for POST /api/user/login
// Backend LoginDto: LoginDto(string Username, string Password, bool RememberMe = false)
export interface LoginDto {
  // Must map to "Username" on the C# side (JSON binding is case-insensitive).
  username: string;
  password: string;
}

// Used for POST /api/user/register
// Backend RegisterDto: RegisterDto(string Username, string Password, string? Email = null)
export interface RegisterDto {
  // Will be stored as IdentityUser.UserName.
  // Backend enforces max length (8 chars) and uniqueness.
  username: string;

  // Optional on backend, but we treat it as required in the UI
  // so the user always enters something.
  email: string;

  password: string;
}

// Legacy from the old JWT-based auth flow.
// If your current backend no longer returns a JWT token (only uses cookies),
// this type may not be used anywhere anymore. It is safe but not required.
export interface AuthTokenResponse {
  token: string;
}
