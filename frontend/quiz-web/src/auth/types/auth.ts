// src/auth/types/auth.ts
// ----------------------
// Defines the shapes of the objects used when a user logs in or registers.

export interface LoginDto {
  username: string;
  password: string;
}

export interface RegisterDto {
  username: string;
  email: string;
  password: string;
}

// Represents the raw JWT token returned from the backend
export interface AuthTokenResponse {
  token: string;
}
