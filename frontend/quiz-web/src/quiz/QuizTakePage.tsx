// src/quiz/QuizTakePage.tsx
// --------------------------
// Full quiz-taking page.
// - Loads a quiz from the API
// - Lets the user select answers (supports multiple correct options)
// - Calculates score on the client side
// - Shows a simple result summary

import React, { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { getQuiz } from "./QuizService";
import type { Quiz } from "../types/quiz";
import Loader from "../components/Loader";
import ErrorAlert from "../components/ErrorAlert";

type AnswerMap = Record<number, number[]>; 
// questionId -> array of selected answerOptionIds

const QuizTakePage: React.FC = () => {
  const { id } = useParams();

  // Data + loading/error state
  const [quiz, setQuiz] = useState<Quiz | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // User's selected answers
  const [answers, setAnswers] = useState<AnswerMap>({});
  const [submitted, setSubmitted] = useState(false);
  const [score, setScore] = useState(0);
  const [maxScore, setMaxScore] = useState(0);

  // For navigating between questions
  const [currentIndex, setCurrentIndex] = useState(0);

  // Load quiz on mount / when id changes
  useEffect(() => {
    async function load() {
      if (!id) {
        setError("Invalid quiz ID.");
        setLoading(false);
        return;
      }

      try {
        const q = await getQuiz(Number(id));
        setQuiz(q);

        // Precompute max possible score
        const totalPoints = q.questions.reduce(
          (sum, question) => sum + question.points,
          0
        );
        setMaxScore(totalPoints);
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

  /**
   * Toggles a given answer option for a question.
   * Supports multiple correct answers (checkbox-style).
   */
  function toggleAnswer(questionId: number, optionId: number) {
    if (submitted) return; // Do not allow changes after submitting

    setAnswers((prev) => {
      const current = prev[questionId] ?? [];
      const exists = current.includes(optionId);

      let next: number[];
      if (exists) {
        next = current.filter((id) => id !== optionId);
      } else {
        next = [...current, optionId];
      }

      return {
        ...prev,
        [questionId]: next,
      };
    });
  }

  /**
   * Calculates the score based on the selected answers and the quiz definition.
   * A question is counted as correct only if:
   * - The set of selected option IDs is exactly equal to the set of correct option IDs.
   */
  function calculateScore(currentQuiz: Quiz, currentAnswers: AnswerMap): number {
    let total = 0;

    for (const question of currentQuiz.questions) {
      const questionId = question.id;
      if (questionId == null) continue; // Safety check

      const selected = currentAnswers[questionId] ?? [];

      const correctIds = question.options
        .filter((opt) => opt.isCorrect)
        .map((opt) => opt.id)
        .filter((id): id is number => id !== undefined);

      // If IDs are missing from backend, skip scoring for that question
      if (correctIds.length === 0) continue;

      const selectedSet = new Set(selected);
      const correctSet = new Set(correctIds);

      const sameSize = selectedSet.size === correctSet.size;
      const allMatch = correctIds.every((id) => selectedSet.has(id));

      if (sameSize && allMatch) {
        total += question.points;
      }
    }

    return total;
  }

  function handleSubmit() {
    if (!quiz) return;

    const s = calculateScore(quiz, answers);
    setScore(s);
    setSubmitted(true);
  }

  function handleRestart() {
    setAnswers({});
    setSubmitted(false);
    setScore(0);
    setCurrentIndex(0);
  }

  if (loading) {
    return (
      <section className="page page-quiz-take">
        <h1 className="page-title">Take Quiz</h1>
        <Loader />
      </section>
    );
  }

  if (error || !quiz) {
    const message = error ?? "Quiz not found.";

    return (
      <section className="page page-quiz-take">
        <h1 className="page-title">Take Quiz</h1>
        <ErrorAlert message={message} />
        <Link to="/quizzes" className="btn btn-secondary">
          Back to quizzes
        </Link>
      </section>
    );
  }

  const questions = quiz.questions;
  const currentQuestion = questions[currentIndex];
  const totalQuestions = questions.length;

  const currentSelectedIds: number[] =
    currentQuestion.id !== undefined && answers[currentQuestion.id]
      ? answers[currentQuestion.id]!
      : [];

  return (
    <section className="page page-quiz-take">
      <h1 className="page-title">{quiz.title}</h1>

      {quiz.description && <p className="quiz-description">{quiz.description}</p>}

      {/* Result summary after submission */}
      {submitted && (
        <div className="quiz-result">
          <p>
            You scored <strong>{score}</strong> out of{" "}
            <strong>{maxScore}</strong> points.
          </p>
          <p>
            Questions answered:{" "}
            <strong>
              {
                Object.keys(answers).filter((qid) => answers[Number(qid)]?.length > 0)
                  .length
              }
            </strong>{" "}
            / <strong>{totalQuestions}</strong>
          </p>
        </div>
      )}

      {/* Only show questions if there are any */}
      {currentQuestion ? (
        <div className="quiz-question-card">
          <div className="quiz-question-header">
            <p className="quiz-question-index">
              Question {currentIndex + 1} of {totalQuestions}
            </p>
            <p className="quiz-question-points">
              {currentQuestion.points} point
              {currentQuestion.points !== 1 ? "s" : ""}
            </p>
          </div>

          <h2 className="quiz-question-text">{currentQuestion.text}</h2>

          <ul className="quiz-options-list">
            {currentQuestion.options.map((option) => {
              if (option.id == null) return null; // Safety

              const checked = currentSelectedIds.includes(option.id);

              // If submitted, we mark correct/incorrect for visual feedback
              let optionClass = "quiz-option";
              if (submitted) {
                if (option.isCorrect) optionClass += " quiz-option-correct";
                if (!option.isCorrect && checked)
                  optionClass += " quiz-option-incorrect";
              } else if (checked) {
                optionClass += " quiz-option-selected";
              }

              return (
                <li key={option.id} className={optionClass}>
                  <label>
                    <input
                      type="checkbox"
                      disabled={submitted}
                      checked={checked}
                      onChange={() =>
                        toggleAnswer(currentQuestion.id as number, option.id!)
                      }
                    />
                    <span>{option.text}</span>
                  </label>
                </li>
              );
            })}
          </ul>

          <div className="quiz-navigation">
            <button
              type="button"
              className="btn btn-secondary"
              onClick={() => setCurrentIndex((i) => Math.max(0, i - 1))}
              disabled={currentIndex === 0}
            >
              Previous
            </button>

            <button
              type="button"
              className="btn btn-secondary"
              onClick={() =>
                setCurrentIndex((i) =>
                  Math.min(totalQuestions - 1, i + 1)
                )
              }
              disabled={currentIndex === totalQuestions - 1}
            >
              Next
            </button>
          </div>

          <div className="quiz-actions">
            {!submitted && (
              <button
                type="button"
                className="btn btn-primary"
                onClick={handleSubmit}
              >
                Submit quiz
              </button>
            )}

            {submitted && (
              <button
                type="button"
                className="btn btn-secondary"
                onClick={handleRestart}
              >
                Retake quiz
              </button>
            )}

            <Link to="/quizzes" className="btn btn-secondary">
              Back to quizzes
            </Link>
          </div>
        </div>
      ) : (
        <p>No questions in this quiz.</p>
      )}
    </section>
  );
};

export default QuizTakePage;
