// src/types/quiz.ts
// ------------------
// Defines TypeScript interfaces for quiz-related data structures.
// These types should mirror your backend DTOs as closely as possible,
// so that the frontend and backend "speak the same language".

/**
 * Represents a single answer option for a question.
 * Example: "Oslo", "Berlin", "Paris" etc.
 */
export interface AnswerOption {
  /**
   * Optional numeric identifier for the answer option.
   * This is usually created by the database.
   * It can be undefined when you create a new quiz on the client.
   */
  id?: number;

  /**
   * The visible text of the answer option shown to the user.
   */
  text: string;

  /**
   * Indicates whether this answer option is a correct answer.
   * Some quizzes may allow multiple correct answers, others only one.
   */
  isCorrect: boolean;
}

/**
 * Represents a single question in a quiz.
 */
export interface Question {
  /**
   * Optional numeric identifier for the question.
   * Created by the backend/database when the quiz is saved.
   */
  id?: number;

  /**
   * The text of the question shown to the user.
   * Example: "What is the capital of Norway?"
   */
  text: string;

  /**
   * Number of points given for a correct answer to this question.
   * This allows you to weight some questions higher than others.
   */
  points: number;

  /**
   * All possible answer options for this question.
   * At least one option should have isCorrect = true.
   */
  options: AnswerOption[];
}

/**
 * Represents a full quiz with all questions and answer options.
 */
export interface Quiz {
  /**
   * Optional numeric identifier for the quiz.
   * Set by the backend/database when the quiz is created.
   */
  id?: number;

  /**
   * Short, descriptive title of the quiz.
   * Example: "General Knowledge", "C# Basics", "Web Development Quiz".
   */
  title: string;

  /**
   * Optional longer description of the quiz.
   * Explains what the quiz is about or who it is for.
   */
  description?: string;

  /**
   * Indicates whether the quiz is published/visible to users.
   * false = draft, true = visible in quiz list for normal users.
   */
  isPublished: boolean;

  /**
   * All questions that belong to this quiz.
   * Can be an empty array while the quiz is being created.
   */
  questions: Question[];
}

/**
 * A lighter version of a quiz used for list views and overview pages.
 * Does not include the full questions and answer options.
 */
export interface QuizSummary {
  /**
   * Unique identifier of the quiz.
   */
  id: number;

  /**
   * Title of the quiz.
   */
  title: string;

  /**
   * Number of questions in the quiz.
   * This is typically calculated on the backend.
   */
  questionCount: number;

  /**
   * Total number of points for the quiz.
   * Also typically calculated on the backend.
   */
  totalPoints: number;

  /**
   * Published state of the quiz.
   */
  isPublished: boolean;
}
