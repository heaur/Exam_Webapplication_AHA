// auth/AuthService.ts
// ---------------------
// This file contains all HTTP requests related to authentication.
// It communicates directly with the backend API and returns tokens
// or throws typed errors to be handled in the UI.

import type { LoginDto, RegisterDto, AuthTokenResponse } from "./types/auth";

// Read the base URL from the Vite environment.
// Example in .env.development:
// VITE_API_URL=http://localhost:5154
const API_URL = import.meta.env.VITE_API_URL as string;

/**
 * Extract a meaningful error message from an ASP.NET Core Web API response.
 * This function tries:
 *   1. JSON with fields: "detail", "title"
 *   2. JSON with ASP.NET validation errors: { errors: { Field: ["msg1", "msg2"] } }
 *   3. Plain text errors
 *   4. Fallback: status message
 */
async function extractErrorMessage(response: Response): Promise<string> {
  const contentType = response.headers.get("content-type") ?? "";

  try {
    if (contentType.includes("application/json")) {
      const json = (await response.json()) as Record<string, unknown>;

      // Pattern 1: { detail: "..." }
      if (typeof json.detail === "string") {
        return json.detail;
      }

      // Pattern 2: { title: "..." }
      if (typeof json.title === "string") {
        return json.title;
      }

      // Pattern 3: ASP.NET model validation errors
      // { errors: { Email: ["Invalid email"], Password: ["Too short"] } }
      const errors = json.errors;
      if (errors && typeof errors === "object") {
        const errObj = errors as Record<string, unknown>;
        const messages: string[] = [];

        for (const key of Object.keys(errObj)) {
          const value = errObj[key];
          if (Array.isArray(value)) {
            messages.push(...(value as string[]));
          }
        }

        if (messages.length > 0) {
          return messages.join(" ");
        }
      }

      // Fallback: show whatever JSON was returned
      return JSON.stringify(json);
    } else {
      // Try plain text
      const text = await response.text();
      if (text) return text;
    }
  } catch {
    // Ignore parsing errors and fall back below
  }

  return `Request failed with status ${response.status}`;
}

// -------------------------------
// PUBLIC AUTH FUNCTIONS
// -------------------------------

export async function loginUser(dto: LoginDto): Promise<string> {
  // Swagger: POST /api/User/login
  const response = await fetch(`${API_URL}/api/User/login`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(dto),
  });

  if (!response.ok) {
    const message = await extractErrorMessage(response);
    console.error("Login failed:", response.status, message);
    throw new Error(message || "Login failed.");
  }

  const data = (await response.json()) as AuthTokenResponse;
  return data.token;
}

export async function registerUser(dto: RegisterDto): Promise<void> {
  // Swagger: POST /api/User/register
  const response = await fetch(`${API_URL}/api/User/register`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(dto),
  });

  if (!response.ok) {
    const message = await extractErrorMessage(response);
    console.error("Registration failed:", response.status, message);
    throw new Error(message || "Registration failed.");
  }
}
