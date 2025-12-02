// src/quiz/QuizService.ts
// ------------------------
// Centralised service for all quiz-related HTTP calls.
// Talks to the ASP.NET Core API (QuizController).

import type { Quiz, QuizSummary, Question } from "../types/quiz";

// ======================================================
// API BASE CONFIG
// ======================================================

const API_URL =
  (import.meta.env.VITE_API_URL as string | undefined) ??
  "http://localhost:5154";

const QUIZ_BASE_URL = `${API_URL}/api/Quiz`;

function jsonHeaders(): HeadersInit {
  return {
    "Content-Type": "application/json",
  };
}

// ======================================================
// TYPE FOR TAKE QUIZ DTO (what backend returns)
// ======================================================

type TakeQuizApiDto = {
  id: number;
  subjectCode: string;
  title: string;
  description: string;
  imageUrl: string;
  isPublished: boolean;

  questions: {
    id: number;
    text: string;
    imageUrl?: string | null;
    points?: number;
    options: {
      id: number;
      text: string;
      isCorrect: boolean;
    }[];
  }[];
};

// ======================================================
// GET ALL QUIZZES (SUMMARY)
// ======================================================

export async function getQuizzes(): Promise<QuizSummary[]> {
  const response = await fetch(QUIZ_BASE_URL, {
    method: "GET",
    headers: jsonHeaders(),
    credentials: "include",
  });

  if (!response.ok) {
    const msg = await response.text();
    throw new Error(msg || "Failed to load quizzes.");
  }

  return (await response.json()) as QuizSummary[];
}

// ======================================================
// GET FULL QUIZ (TAKE VIEW)
// ======================================================

export async function getQuiz(id: number): Promise<Quiz> {
  const response = await fetch(`${QUIZ_BASE_URL}/${id}/take`, {
    method: "GET",
    headers: jsonHeaders(),
    credentials: "include",
  });

  if (!response.ok) {
    const msg = await response.text();
    throw new Error(msg || `Failed to load quiz (take view) with id ${id}.`);
  }

  // Typed DTO from backend
  const raw = (await response.json()) as TakeQuizApiDto;

  // Normalise questions
  const questions: Question[] = Array.isArray(raw.questions)
    ? raw.questions.map((q) => ({
        id: q.id,
        text: q.text,
        imageUrl: q.imageUrl ?? undefined,
        points: q.points ?? 1,
        options: q.options.map((o) => ({
          text: o.text,
          isCorrect: !!o.isCorrect,
        })),
      }))
    : [];

  // Build strict Quiz for frontend
  const quiz: Quiz = {
    id: raw.id,
    subjectCode: raw.subjectCode ?? "",
    title: raw.title ?? "Untitled quiz",
    description: raw.description ?? "",
    imageUrl: raw.imageUrl ?? undefined,
    isPublished: raw.isPublished ?? false,
    questions,
  };

  return quiz;
}

// ======================================================
// CREATE QUIZ
// ======================================================

export async function createQuiz(quiz: Quiz): Promise<Quiz> {
  const response = await fetch(QUIZ_BASE_URL, {
    method: "POST",
    headers: jsonHeaders(),
    credentials: "include",
    body: JSON.stringify(quiz),
  });

  if (!response.ok) {
    const msg = await response.text();
    throw new Error(msg || "Failed to create quiz.");
  }

  return (await response.json()) as Quiz;
}

// ======================================================
// UPDATE QUIZ
// ======================================================

export async function updateQuiz(id: number, quiz: Quiz): Promise<void> {
  const response = await fetch(`${QUIZ_BASE_URL}/${id}`, {
    method: "PUT",
    headers: jsonHeaders(),
    credentials: "include",
    body: JSON.stringify(quiz),
  });

  if (!response.ok) {
    const msg = await response.text();
    throw new Error(msg || `Failed to update quiz with id ${id}.`);
  }
}

// ======================================================
// DELETE QUIZ
// ======================================================

export async function deleteQuiz(id: number): Promise<void> {
  const response = await fetch(`${QUIZ_BASE_URL}/${id}`, {
    method: "DELETE",
    headers: jsonHeaders(),
    credentials: "include",
  });

  if (!response.ok) {
    const msg = await response.text();
    throw new Error(msg || `Failed to delete quiz with id ${id}.`);
  }
}
