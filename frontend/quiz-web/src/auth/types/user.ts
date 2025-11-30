// src/auth/types/user.ts
// ----------------------
// The User interface represents the decoded payload of a JWT token.

export interface User {
  sub: string;     // Usually: username or userId
  exp: number;     // Expiration timestamp (seconds since epoch)
  iat?: number;    // Issued-at timestamp
  email?: string;  // If included by backend
  role?: string;   // Optional: admin / user etc.
}
