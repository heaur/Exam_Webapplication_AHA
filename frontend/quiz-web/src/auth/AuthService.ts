// src/auth/AuthService.ts
// ------------------------------------------------------
// Handles ALL authentication-related HTTP requests.
// Matches your ASP.NET backend routes under /api/User/*
//
// This service is used by AuthContext.tsx to perform
// login, registration, logout and session restoration.
//
// NO "any", fully typed, ESLint compatible.
// ------------------------------------------------------

import type { LoginDto, RegisterDto, AuthTokenResponse } from "./types/auth";
import type { User } from "./types/user";

// Base URL, ex: VITE_API_URL = "http://localhost:5154"
const API_URL = import.meta.env.VITE_API_URL as string;
const USER_BASE_URL = `${API_URL}/api/User`;

/**
 * Returns standard JSON headers.
 * We do NOT send Authorization header here — backend auth is cookie-based.
 */
function jsonHeaders(): HeadersInit {
  return {
    "Content-Type": "application/json",
  };
}

/**
 * Type guard to validate if an unknown response looks like
 * { token: string }
 */
function isAuthTokenResponse(value: unknown): value is AuthTokenResponse {
  return (
    typeof value === "object" &&
    value !== null &&
    "token" in value &&
    typeof (value as { token: unknown }).token === "string"
  );
}

/* ------------------------------------------------------
 *  LOGIN
 *  POST /api/User/login
 *  Returns a JWT token as either:
 *     1) raw string
 *     2) { token: "..." }
 *
 *  If backend uses ONLY cookies and returns no token,
 *  we treat that as success and return an empty string.
 * ----------------------------------------------------- */
export async function loginUser(dto: LoginDto): Promise<string> {
  const response = await fetch(`${USER_BASE_URL}/login`, {
    method: "POST",
    headers: jsonHeaders(),
    body: JSON.stringify(dto),
    credentials: "include", // send/receive auth cookies
  });

  // ---------- user friendly error handling ----------
  if (!response.ok) {
    let message = "Innlogging feilet.";

    try {
      const err = await response.json();

      if (response.status === 401) {
        // typisk “feil brukernavn/passord”
        message = "Feil brukernavn eller passord.";
      } else if (response.status === 400 && err?.errors) {
        // model validation errors fra backend
        message =
          (Object.values(err.errors) as unknown[])
            .flat()
            .join(" ") || "Ugyldige innloggingsdata.";
      } else if (err?.title) {
        // ProblemDetails.title
        message = err.title;
      }
    } catch {
      // hvis parsing feiler, bruk default message
    }

    throw new Error(message);
  }

  // ---------- success path ----------
  const data: unknown = await response.json().catch(() => null);

  // Case 1: backend returnerer ren string
  if (typeof data === "string" && data.trim()) {
    return data.trim();
  }

  // Case 2: backend returnerer { token: "..." }
  if (isAuthTokenResponse(data) && data.token.trim()) {
    return data.token.trim();
  }

  // Case 3: ingen token i body (cookie-basert auth)
  // Login er likevel OK, så vi returnerer tom streng.
  return "";
}


/* ------------------------------------------------------
 *  REGISTER
 *  POST /api/User/register
 *  Behaves like login, but ALSO tolerates no token in response.
 * ----------------------------------------------------- */
export async function registerUser(dto: RegisterDto): Promise<string> {
  const response = await fetch(`${USER_BASE_URL}/register`, {
    method: "POST",
    headers: jsonHeaders(),
    body: JSON.stringify(dto),
    credentials: "include",
  });

  // --- Brukervennlig feilhåndtering ---
  if (!response.ok) {
    let message = "Registrering feilet.";

    try {
      const err = await response.json();

      // Typisk model validation-feil
      if (response.status === 400 && err?.errors) {
        message =
          Object.values(err.errors).flat().join(" ") ||
          "Ugyldige registreringsdata.";
      } else if (err?.title) {
        message = err.title;
      }
    } catch {
      // ignorer parsing-feil, behold fallback-melding
    }

    throw new Error(message);
  }

  // --- Suksess men ingen body (f.eks. 200/204 uten innhold) ---
  // Vi forventer ikke nødvendigvis token her, så dette er OK.
  const data: unknown = await response.json().catch(() => null);

  // Backend kan evt. returnere et rent token som string
  if (typeof data === "string") {
    const trimmed = data.trim();
    return trimmed;
  }

  // Eller et objekt { token: "..." }
  if (isAuthTokenResponse(data)) {
    const trimmed = data.token.trim();
    return trimmed;
  }

  // Ingen token, men registreringen var vellykket -> returner tom streng
  return "";
}


/* ------------------------------------------------------
 *  GET CURRENT USER
 *  GET /api/User/me
 *  Returns decoded user info OR null.
 * ----------------------------------------------------- */
export async function getCurrentUser(): Promise<User | null> {
  const response = await fetch(`${USER_BASE_URL}/me`, {
    method: "GET",
    credentials: "include",
  });

  if (response.status === 401) {
    return null; // Not logged in
  }

  if (!response.ok) {
    return null;
  }

  const data = (await response.json().catch(() => null)) as User | null;
  return data;
}

/* ------------------------------------------------------
 *  LOGOUT
 *  POST /api/User/logout
 * ----------------------------------------------------- */
export async function logoutUser(): Promise<void> {
  const response = await fetch(`${USER_BASE_URL}/logout`, {
    method: "POST",
    credentials: "include",
  });

  if (!response.ok) {
    throw new Error("Logout failed.");
  }
}
