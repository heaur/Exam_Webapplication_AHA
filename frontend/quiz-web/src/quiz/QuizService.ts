// src/quiz/QuizService.ts
// ------------------------
// This file contains all HTTP requests related to quizzes.
// It acts as the "API service layer" for the quiz domain.
// Every component should import functions from here instead of calling fetch() directly.

import type { Quiz, QuizSummary } from "../types/quiz";

// Read base URL from Vite environment variables.
// Example: VITE_API_URL = "http://localhost:5154"
const API_URL = import.meta.env.VITE_API_URL as string;

/**
 * Builds the headers for authenticated requests.
 * If the user is logged in, the Authorization header will include the JWT.
 */
function authHeaders(): HeadersInit {
  const token = localStorage.getItem("token");

  const headers: HeadersInit = {
    "Content-Type": "application/json",
  };

  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  return headers;
}

/**
 * GET /api/quizzes
 * Retrieves a list of quiz summaries (lightweight quiz objects).
 */
export async function getQuizzes(): Promise<QuizSummary[]> {
  const response = await fetch(`${API_URL}/api/quizzes`, {
    method: "GET",
    headers: authHeaders(),
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(errorText || "Failed to load quizzes.");
  }

  const data = (await response.json()) as QuizSummary[];
  return data;
}

/**
 * GET /api/quizzes/{id}
 * Retrieves a full quiz, including all questions and answer options.
 */
export async function getQuiz(id: number): Promise<Quiz> {
  const response = await fetch(`${API_URL}/api/quizzes/${id}`, {
    method: "GET",
    headers: authHeaders(),
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(errorText || `Failed to load quiz with id ${id}.`);
  }

  const data = (await response.json()) as Quiz;
  return data;
}

/**
 * POST /api/quizzes
 * Creates a new quiz (including its questions and answer options).
 */
export async function createQuiz(quiz: Quiz): Promise<Quiz> {
  const response = await fetch(`${API_URL}/api/quizzes`, {
    method: "POST",
    headers: authHeaders(),
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
 * PUT /api/quizzes/{id}
 * Updates an existing quiz.
 */
export async function updateQuiz(id: number, quiz: Quiz): Promise<Quiz> {
  const response = await fetch(`${API_URL}/api/quizzes/${id}`, {
    method: "PUT",
    headers: authHeaders(),
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
 * DELETE /api/quizzes/{id}
 * Deletes the quiz from the system.
 */
export async function deleteQuiz(id: number): Promise<void> {
  const response = await fetch(`${API_URL}/api/quizzes/${id}`, {
    method: "DELETE",
    headers: authHeaders(),
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(errorText || `Failed to delete quiz with id ${id}.`);
  }
}
