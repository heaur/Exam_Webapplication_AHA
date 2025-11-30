// src/quiz/QuizService.ts
// ------------------------
// Centralised service for all quiz-related HTTP calls.
// Talks to the ASP.NET Core API (QuizController).

import type { Quiz, QuizSummary } from "../types/quiz";

// Base URL from Vite env. Fallback to localhost:5154 if missing.
const API_URL =
  (import.meta.env.VITE_API_URL as string | undefined) ??
  "http://localhost:5154";

// Controller name = "Quiz"  => base URL = /api/Quiz
const QUIZ_BASE_URL = `${API_URL}/api/Quiz`;

/**
 * Builds default headers.
 * We *could* sende Authorization: Bearer <token> her,
 * men i ditt oppsett brukes Identity-cookie, så det er ikke nødvendig.
 */
function jsonHeaders(): HeadersInit {
  return {
    "Content-Type": "application/json",
  };
}

/**
 * GET /api/Quiz
 * Returns a list of quizzes (summary objects) from the backend.
 */
export async function getQuizzes(): Promise<QuizSummary[]> {
  const response = await fetch(QUIZ_BASE_URL, {
    method: "GET",
    headers: jsonHeaders(),
    credentials: "include", // <--- VIKTIG
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(errorText || "Failed to load quizzes.");
  }

  const data = (await response.json()) as QuizSummary[];
  return data;
}

/**
 * GET /api/Quiz/{id}
 * Loads one full quiz including questions and options.
 */
export async function getQuiz(id: number): Promise<Quiz> {
  const response = await fetch(`${QUIZ_BASE_URL}/${id}`, {
    method: "GET",
    headers: jsonHeaders(),
    credentials: "include", // <--- VIKTIG
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(errorText || `Failed to load quiz with id ${id}.`);
  }

  const data = (await response.json()) as Quiz;
  return data;
}

/**
 * POST /api/Quiz
 * Creates a new quiz.
 */
export async function createQuiz(quiz: Quiz): Promise<Quiz> {
  const response = await fetch(QUIZ_BASE_URL, {
    method: "POST",
    headers: jsonHeaders(),
    credentials: "include", // <--- VIKTIG
    body: JSON.stringify(quiz),
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(errorText || "Failed to create quiz.");
  }

  const data = (await response.json()) as Quiz;
  return data;
}

/**
 * PUT /api/Quiz/{id}
 * Updates an existing quiz.
 */
export async function updateQuiz(id: number, quiz: Quiz): Promise<Quiz> {
  const response = await fetch(`${QUIZ_BASE_URL}/${id}`, {
    method: "PUT",
    headers: jsonHeaders(),
    credentials: "include", // <--- VIKTIG
    body: JSON.stringify(quiz),
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(errorText || `Failed to update quiz with id ${id}.`);
  }

  const data = (await response.json()) as Quiz;
  return data;
}

/**
 * DELETE /api/Quiz/{id}
 * Deletes the quiz.
 */
export async function deleteQuiz(id: number): Promise<void> {
  const response = await fetch(`${QUIZ_BASE_URL}/${id}`, {
    method: "DELETE",
    headers: jsonHeaders(),
    credentials: "include", // <--- VIKTIG
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(errorText || `Failed to delete quiz with id ${id}.`);
  }
}
