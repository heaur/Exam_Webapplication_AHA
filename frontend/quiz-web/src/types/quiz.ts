// src/types/quiz.ts
// ------------------
// Shared TypeScript types for quiz data used across the frontend.

// ---------- QUIZ LIST / SUMMARY ----------

// Short quiz info used in lists/tables and on the homepage.
export type QuizSummary = {
  id: number;
  subjectCode: string;    // Course code, e.g. "ITPE3200"
  title: string;          // Quiz title
  description: string;    // Required description on frontend
  imageUrl: string;       // Required cover image URL on frontend
  questionCount: number;  // Number of questions in the quiz
  totalPoints?: number;   // Optional total score if you calculate it
  isPublished: boolean;   // Whether the quiz is visible to users
};

// ---------- QUESTIONS & OPTIONS ----------

// Single answer option for a question
export type Option = {
  id?: number | null;     // Optional: exists only for persisted options
  questionId?: number;
  text: string;           // Display text for this option
  isCorrect: boolean;     // True if this is the correct answer
};

// Question with its options
export type Question = {
  id?: number | null;     // Optional: exists only for persisted questions
  quizId?: number;        // FK to quiz if needed
  text: string;           // Question text
  imageUrl?: string;      // Optional image URL per question
  points: number;         // Points awarded for this question
  options: Option[];      // Answer options for this question
};

// ---------- FULL QUIZ (CREATE / EDIT / TAKE) ----------

// Full quiz type used when creating, editing and taking a quiz
export type Quiz = {
  id?: number;            // Optional when creating a new quiz
  subjectCode: string;    // Course code, required on frontend
  title: string;          // Quiz title, required
  description: string;    // Required description
  imageUrl: string;       // Required cover image URL
  isPublished: boolean;   // Publication flag

  createdAt?: string;         // ISO timestamp from backend
  updatedAt?: string | null;
  publishedAt?: string | null;
  ownerId?: string | null;

  questionCount?: number;     // Optional aggregate
  totalPoints?: number;       // Optional aggregate

  questions: Question[];      // Full list of questions for this quiz
};

// ---------- RESULTS (USED ON RESULT PAGE + MIN PROFIL) ----------

// One stored result for a completed quiz.
// Mirrors ResultReadDto in the backend.
export type QuizResult = {
  resultId: number;
  userId?: string | null;
  quizId: number;

  quizTitle: string;      // Title of the quiz you took
  subjectCode: string;    // Course code (e.g. "ITPE3200")

  correctCount: number;   // Number of correct questions
  totalQuestions: number; // Number of questions in the quiz
  completedAt: string;    // ISO timestamp when quiz was completed
  percentage: number;     // 0–100 (already calculated in backend)
};
