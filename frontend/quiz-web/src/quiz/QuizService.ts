// src/quiz/QuizService.ts
// ------------------------
// Centralised service for all quiz-related HTTP calls.
// Talks to the ASP.NET Core API (QuizController + ResultController).

import type { Quiz, QuizSummary, Question, QuizResult } from "../types/quiz";

// ======================================================
// API BASE CONFIG
// ======================================================

const API_URL =
  (import.meta.env.VITE_API_URL as string | undefined) ??
  "http://localhost:5154";

const QUIZ_BASE_URL = `${API_URL}/api/Quiz`;
const RESULT_BASE_URL = `${API_URL}/api/Result`;

function jsonHeaders(): HeadersInit {
  return {
    "Content-Type": "application/json",
  };
}

// ======================================================
// TYPES FOR "TAKE QUIZ" DTO (what backend returns)
// ======================================================

// This matches QuizApi.DTOs.TakeQuizDto on the backend
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

// Result DTO from backend for "my results" list
type ResultReadApiDto = {
  resultId: number;
  userId: string | null;
  quizId: number;
  quizTitle: string;
  subjectCode: string;
  correctCount: number;
  totalQuestions: number;
  completedAt: string;
  percentage: number;
};

// Payload we send when submitting a result
// Now also includes all user's answers (questionId -> optionId)
type ResultCreateApiDto = {
  quizId: number;
  correctCount: number;
  totalQuestions: number;
  // questionId -> optionId (the *database id* of the chosen option)
  answers: Record<number, number>;
};

// DTO returned by /api/Result/{resultId}/full
// Used when opening a result from the Profile page.
type FullResultApiDto = {
  result: ResultReadApiDto;
  quiz: TakeQuizApiDto;
  // questionId -> optionId (database id of chosen option)
  answers: Record<number, number>;
};

// Internal type used by frontend for "questionId -> chosen option index"
export type AnswerMap = Record<number, number | null>;

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

  const raw = (await response.json()) as TakeQuizApiDto;

  // Map backend DTO -> internal Quiz type
  const questions: Question[] = Array.isArray(raw.questions)
    ? raw.questions.map((q) => ({
        id: q.id,
        text: q.text,
        imageUrl: q.imageUrl ?? undefined,
        points: q.points ?? 1,
        options: q.options.map((o) => ({
          id: o.id,
          text: o.text,
          isCorrect: !!o.isCorrect,
        })),
      }))
    : [];

  const quiz: Quiz = {
    id: raw.id,
    subjectCode: raw.subjectCode ?? "",
    title: raw.title ?? "Untitled quiz",
    description: raw.description ?? "",
    imageUrl: raw.imageUrl ?? "",
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

// ======================================================
// GET MY QUIZZES (PROFILE PAGE)
// ======================================================

export async function getMyQuizzes(): Promise<QuizSummary[]> {
  const response = await fetch(`${QUIZ_BASE_URL}/my`, {
    method: "GET",
    headers: jsonHeaders(),
    credentials: "include",
  });

  if (!response.ok) {
    const text = await response.text();
    console.error(
      "getMyQuizzes failed:",
      response.status,
      response.statusText,
      text
    );
    throw new Error(
      text || `Failed to load quizzes for current user (status ${response.status}).`
    );
  }

  return (await response.json()) as QuizSummary[];
}

// ======================================================
// GET MY RESULTS (PROFILE PAGE – SUMMARY LIST)
// ======================================================

export async function getMyResults(): Promise<QuizResult[]> {
  const response = await fetch(`${RESULT_BASE_URL}/my`, {
    method: "GET",
    headers: jsonHeaders(),
    credentials: "include",
  });

  if (!response.ok) {
    const text = await response.text();
    console.error(
      "getMyResults failed:",
      response.status,
      response.statusText,
      text
    );
    throw new Error(
      text || `Failed to load results for current user (status ${response.status}).`
    );
  }

  const raw = (await response.json()) as ResultReadApiDto[];

  // Map backend DTO -> internal QuizResult type
  const mapped: QuizResult[] = raw.map((r) => ({
    resultId: r.resultId,
    userId: r.userId ?? undefined,
    quizId: r.quizId,
    quizTitle: r.quizTitle,
    subjectCode: r.subjectCode,
    correctCount: r.correctCount,
    totalQuestions: r.totalQuestions,
    percentage: r.percentage,
    completedAt: r.completedAt,
  }));

  return mapped;
}

// ======================================================
// SUBMIT RESULT WHEN USER FINISHES A QUIZ
// ======================================================

export async function submitResult(
  quizId: number,
  payload: ResultCreateApiDto
): Promise<void> {
  const response = await fetch(`${RESULT_BASE_URL}`, {
    method: "POST",
    headers: jsonHeaders(),
    credentials: "include",
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    const text = await response.text();
    console.error(
      "submitResult failed:",
      response.status,
      response.statusText,
      text
    );
    throw new Error(text || "Failed to submit quiz result.");
  }
}

// ======================================================
// GET *FULL* RESULT (summary + quiz + answers)
// Used when opening a result from the Profile page
// so we can reuse the same QuizResultPage layout.
// ======================================================

export async function getFullResult(
  resultId: number
): Promise<{ result: QuizResult; quiz: Quiz; answers: AnswerMap }> {
  const response = await fetch(`${RESULT_BASE_URL}/${resultId}/full`, {
    method: "GET",
    headers: jsonHeaders(),
    credentials: "include",
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || `Failed to load full result (${response.status}).`);
  }

  const raw = (await response.json()) as FullResultApiDto;

  // ----- Map quiz part -----
  const questions: Question[] = Array.isArray(raw.quiz.questions)
    ? raw.quiz.questions.map((q) => ({
        id: q.id,
        text: q.text,
        imageUrl: q.imageUrl ?? undefined,
        points: q.points ?? 1,
        options: q.options.map((o) => ({
          id: o.id,
          text: o.text,
          isCorrect: !!o.isCorrect,
        })),
      }))
    : [];

  const quiz: Quiz = {
    id: raw.quiz.id,
    subjectCode: raw.quiz.subjectCode ?? "",
    title: raw.quiz.title ?? "Untitled quiz",
    description: raw.quiz.description ?? "",
    imageUrl: raw.quiz.imageUrl ?? "",
    isPublished: raw.quiz.isPublished ?? false,
    questions,
  };

  // ----- Map result summary -----
  const result: QuizResult = {
    resultId: raw.result.resultId,
    userId: raw.result.userId ?? undefined,
    quizId: raw.result.quizId,
    quizTitle: raw.result.quizTitle,
    subjectCode: raw.result.subjectCode,
    correctCount: raw.result.correctCount,
    totalQuestions: raw.result.totalQuestions,
    percentage: raw.result.percentage,
    completedAt: raw.result.completedAt,
  };

  // ----- Map answers: optionId -> index in options array -----
  const answers: AnswerMap = {};

  for (const q of questions) {
    if (q.id == null) continue;

    const optionId = raw.answers[q.id];

    if (optionId == null) {
      // User did not answer this question (or data missing)
      answers[q.id] = null;
      continue;
    }

    const idx = q.options.findIndex((o) => o.id === optionId);
    answers[q.id] = idx >= 0 ? idx : null;
  }

  return { result, quiz, answers };
}
