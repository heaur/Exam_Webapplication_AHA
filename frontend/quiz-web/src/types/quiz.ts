// src/types/quiz.ts
// ------------------
// Shared TypeScript types for quizzes used across the frontend.
// These types should match the backend DTOs as closely as possible.

export interface QuizOption {
  id?: number;
  text: string;
  isCorrect: boolean;
}

export interface QuizQuestion {
  id?: number;
  text: string;
  // Optional image per question (not required)
  imageUrl?: string;
  // Number of points this question is worth
  points: number;
  options: QuizOption[];
}

export interface Quiz {
  // id is only present for existing quizzes (not when creating a new one)
  id?: number;

  // Course / subject code, e.g. "ITPE3200"
  subjectCode: string;

  // Human-readable title shown in lists and on the quiz page
  title: string;

  // Short description shown on the browse cards and quiz page
  description: string;

  // Cover image shown on the browse cards and quiz page
  imageUrl: string;

  // Whether the quiz is visible to others
  isPublished: boolean;

  // Full set of questions including options
  questions: QuizQuestion[];
}

// Lightweight summary used on list / browse pages
export interface QuizSummary {
  id: number;
  subjectCode: string;
  title: string;
  description: string;
  imageUrl: string;

  // These can be sent from backend to show quick stats if you want
  questionCount?: number;
  totalPoints?: number;
}
