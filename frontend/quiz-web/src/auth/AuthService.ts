// auth/AuthService.ts
// ---------------------
// Dette er det eneste stedet i frontend som kommuniserer direkte
// med backend sitt /api/User-endepunkt.
//
// BACKEND I DITT PROSJEKT BRUKER COOKIE-BASERT AUTENTISERING (Identity)
// ----------------------------------------------------------------------
// Det betyr:
//   - Backend setter en AUTH COOKIE ved login (ikke JWT).
//   - Frontend MÅ sende `credentials: "include"` for at cookies skal følge med.
//   - Backend returnerer 204 NoContent på login (ingen token).
//   - Backend returnerer 201 Created + JSON på register.
//
// Dette er HELT annerledes enn JWT-strukturen du hadde opprinnelig.
// Denne filen er fullstendig refaktorert for å matche korrekt identitetsflyt.
//

import type { LoginDto, RegisterDto } from "./types/auth";
import { parseJsonSafe } from "../utils/parseJsonSafe";

// Base URL fra .env (skal være "http://localhost:XXXX")
const API_URL = import.meta.env.VITE_API_URL as string;

/**
 * Henter en lesbar feilmelding fra ASP.NET Identity responses.
 *
 * ASP.NET kan returnere:
 *  - ProblemDetails (400, 401, 404...)
 *  - ValidationProblemDetails (400 med errors{...})
 *  - Tom respons (f.eks. 204)
 *
 * Denne funksjonen sørger for at vi ALDRI viser rå JSON i UI,
 * men alltid en ren tekstmelding.
 */
async function extractErrorMessage(response: Response): Promise<string> {
  const json = await parseJsonSafe(response);

  // 1) Håndter ModelState Validation-feil
  if (json && json.errors && typeof json.errors === "object") {
    const errObj = json.errors as Record<string, unknown>;
    const messages: string[] = [];

    for (const key of Object.keys(errObj)) {
      const value = errObj[key];
      if (Array.isArray(value)) {
        messages.push(...(value as string[]));
      }
    }

    if (messages.length > 0) return messages.join(" ");
  }

  // 2) ProblemDetails-standarder (title, detail)
  if (json?.detail) return json.detail as string;
  if (json?.title) return json.title as string;

  // 3) Prøv tekst-body
  const text = await response.text().catch(() => "");
  if (text) return text;

  // 4) Fallback
  return `Request failed with status ${response.status}`;
}

/**
 * LOGIN — Cookie-based identity
 * ------------------------------
 * Backend returnerer 204 NoContent ved suksess.
 * Cookies blir satt automatisk (hvis credentials: "include").
 *
 * Derfor:
 *   - Vi forventer IKKE JSON-body
 *   - Vi mottar IKKE en token
 *   - Vi må hente brukeren etterpå via /api/User/me
 */
export async function loginUser(dto: LoginDto): Promise<void> {
  const response = await fetch(`${API_URL}/api/User/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(dto),
    credentials: "include", // ⬅ NØDVENDIG for at cookie skal følge med
  });

  if (!response.ok) {
    const msg = await extractErrorMessage(response);
    throw new Error(msg || "Login failed");
  }

  // 204 = suksess (backend har satt cookie)
}

/**
 * REGISTER
 * --------
 * Backend returnerer:
 *   - 201 Created + JSON (CurrentUserDto)
 *   - 400 bad request (validation)
 * Vi bryr oss ikke om JSON her, bare om suksess.
 */
export async function registerUser(dto: RegisterDto): Promise<void> {
  const response = await fetch(`${API_URL}/api/User/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(dto),
    credentials: "include", // cookie kreves for auto-login etterpå
  });

  if (!response.ok) {
    const msg = await extractErrorMessage(response);
    throw new Error(msg || "Registration failed");
  }
}

/**
 * LOGOUT
 * ------
 * Backend signerer brukeren ut via cookie.
 */
export async function logoutUser(): Promise<void> {
  const response = await fetch(`${API_URL}/api/User/logout`, {
    method: "POST",
    credentials: "include",
  });

  if (!response.ok) {
    const msg = await extractErrorMessage(response);
    throw new Error(msg || "Logout failed");
  }
}

/**
 * ME — henter nåværende bruker fra cookie-basert sesjon.
 * Returnerer enten UserDto eller null.
 */
export async function fetchMe() {
  const response = await fetch(`${API_URL}/api/User/me`, {
    method: "GET",
    credentials: "include",
  });

  if (!response.ok) return null;
  return await parseJsonSafe(response);
}
