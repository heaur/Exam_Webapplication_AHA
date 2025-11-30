// src/quiz/QuizTakePage.tsx
// --------------------------
// Page for taking a quiz.
//
// Behaviour:
// - Only authenticated users can access this page.
// - Reads quiz id from the URL, loads quiz data from the API.
// - Renders each question with single-choice radio buttons.
// - Keeps selected answers in local React state.
// - On submit, calculates score on the client and navigates
//   to a result page, passing quiz + answers + score via
//   React Router's location state.

import React, { useEffect, useState } from "react";
import { Navigate, useNavigate, useParams } from "react-router-dom";
import { getQuiz } from "./QuizService";
import type { Quiz, Question } from "../types/quiz";
import { useAuth } from "../auth/UseAuth";
import Loader from "../components/Loader";
import ErrorAlert from "../components/ErrorAlert";

// Simple mapping: questionId -> selected option index (0-3) or null
type AnswerMap = Record<number, number | null>;

// Narrow type describing what the backend *might* send for a single quiz.
// We keep everything optional because the API is not fully consistent
// (sometimes it returns a "summary" without questions).
type RawQuizFromApi = {
  id?: number;
  subjectCode?: string | null;
  title?: string | null;
  description?: string | null;
  isPublished?: boolean | null;
  questions?: Question[] | null;
};

const QuizTakePage: React.FC = () => {
  const { id } = useParams();
  const { user } = useAuth();
  const navigate = useNavigate();

  // Quiz loaded from backend (normalised to our Quiz type)
  const [quiz, setQuiz] = useState<Quiz | null>(null);
  // Map of selected answers per question id
  const [answers, setAnswers] = useState<AnswerMap>({});
  // Loading / error state
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // ------------------------------------------------------
  // Load quiz on mount / when id changes
  // ------------------------------------------------------
  useEffect(() => {
    async function load() {
      if (!id) {
        setError("Invalid quiz id.");
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setError(null);

        // We accept that the backend might return a "summary" object
        // without questions, so we cast to our RawQuizFromApi helper type.
        const raw = (await getQuiz(Number(id))) as RawQuizFromApi;

        // Normalise questions: always an array on the frontend,
        // even if backend omitted the property or returned null.
        const questions: Question[] = Array.isArray(raw.questions)
          ? raw.questions
          : [];

        // Normalise into our strict Quiz type so the rest of the
        // component can rely on a stable shape.
        const q: Quiz = {
          id: raw.id,
          subjectCode: raw.subjectCode ?? "",
          title: raw.title ?? "Untitled quiz",
          description: raw.description ?? undefined,
          isPublished: raw.isPublished ?? false,
          questions,
        };

        setQuiz(q);

        // Initialise answer map: one key per question id,
        // all start as "no answer selected" (null).
        const initialAnswers: AnswerMap = {};
        for (const question of questions) {
          if (question.id != null) {
            initialAnswers[question.id] = null;
          }
        }
        setAnswers(initialAnswers);
      } catch (err: unknown) {
        if (err instanceof Error) {
          setError(err.message || "Failed to load quiz.");
        } else {
          setError("Failed to load quiz.");
        }
      } finally {
        setLoading(false);
      }
    }

    void load();
  }, [id]);

  // ------------------------------------------------------
  // Auth guard (after hooks)
  // ------------------------------------------------------
  if (!user) {
    // User is not logged in -> send to login page.
    return <Navigate to="/login" replace />;
  }

  // ------------------------------------------------------
  // Event handlers
  // ------------------------------------------------------

  function handleExit() {
    // Just go back to home page when user clicks the "X" button.
    navigate("/");
  }

  function handleSelectOption(questionId: number, optionIndex: number) {
    // Store chosen option index for this question id.
    setAnswers((prev) => ({
      ...prev,
      [questionId]: optionIndex,
    }));
  }

  function handleSubmitQuiz() {
    if (!quiz) return;

    let score = 0;
    let maxScore = 0;

    // We know quiz.questions is always an array after normalisation,
    // but we still assign to a local const for clarity.
    const questionList: Question[] = quiz.questions ?? [];

    for (const question of questionList) {
      const qId = question.id;
      if (qId == null) continue;

      maxScore += question.points;

      const chosenIndex = answers[qId];
      const correctIndex = question.options.findIndex((opt) => opt.isCorrect);

      if (chosenIndex != null && chosenIndex === correctIndex) {
        score += question.points;
      }
    }

    // Navigate to result page and pass full data in router state
    // so that the result view does not need to re-fetch.
    navigate(`/quizzes/${quiz.id}/result`, {
      state: {
        quiz,
        answers,
        score,
        maxScore,
      },
    });
  }

  // ------------------------------------------------------
  // Render: loading state
  // ------------------------------------------------------
  if (loading) {
    return (
      <section className="page page-quiz-take">
        <div className="quiz-top-bar">
          <button
            type="button"
            className="quiz-exit-btn"
            onClick={handleExit}
          >
            ×
          </button>
        </div>
        <Loader />
      </section>
    );
  }

  // ------------------------------------------------------
  // Render: error state
  // ------------------------------------------------------
  if (error || !quiz) {
    return (
      <section className="page page-quiz-take">
        <div className="quiz-top-bar">
          <button
            type="button"
            className="quiz-exit-btn"
            onClick={handleExit}
          >
            ×
          </button>
        </div>
        <ErrorAlert message={error ?? "Quiz not found."} />
      </section>
    );
  }

  // At this point TypeScript knows quiz is a proper Quiz object.
  const questionList: Question[] = quiz.questions ?? [];

  // ------------------------------------------------------
  // Render: quiz content
  // ------------------------------------------------------
  return (
    <section className="page page-quiz-take">
      {/* Top bar with subject + title on the left, exit button on the right */}
      <div className="quiz-top-bar">
        <div className="quiz-header-info">
          <p className="quiz-subject">{quiz.subjectCode}</p>
          <h1 className="page-title">{quiz.title}</h1>
        </div>
        <button
          type="button"
          className="quiz-exit-btn"
          onClick={handleExit}
        >
          ×
        </button>
      </div>

      {/* Render each question as a card */}
      {questionList.map((question, index) => {
        const qId = question.id;
        if (qId == null) return null;

        const selectedIndex = answers[qId];

        return (
          <div key={qId} className="quiz-question-card">
            {/* Small header with question number and points */}
            <p className="quiz-question-index">
              Question {index + 1} • {question.points} point
              {question.points !== 1 ? "s" : ""}
            </p>

            {/* Main question text */}
            <h2 className="quiz-question-text">{question.text}</h2>

            {/* Optional image */}
            {question.imageUrl && (
              <div className="quiz-question-image">
                <img src={question.imageUrl} alt="Question" />
              </div>
            )}

            {/* Single-choice options */}
            <ul className="quiz-options-list">
              {question.options.map((opt, optIndex) => (
                <li key={optIndex} className="quiz-option">
                  <label>
                    <input
                      type="radio"
                      name={`q-${qId}`}
                      checked={selectedIndex === optIndex}
                      onChange={() => handleSelectOption(qId, optIndex)}
                    />
                    <span>{opt.text}</span>
                  </label>
                </li>
              ))}
            </ul>
          </div>
        );
      })}

      {/* Submit button at the bottom */}
      <div className="quiz-actions">
        <button
          type="button"
          className="btn btn-primary"
          onClick={handleSubmitQuiz}
        >
          Submit quiz
        </button>
      </div>
    </section>
  );
};

export default QuizTakePage;
