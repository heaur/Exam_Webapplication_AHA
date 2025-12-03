// src/quiz/QuizResultPage.tsx
// ---------------------------
// Shows quiz result.
//
// Two entry points:
//  1) Directly after taking a quiz:
//       navigate(`/quizzes/${quiz.id}/result`, { state: { result, quiz, answers, ... } })
//     -> we already have quiz + answers + result, no extra fetch.
//
//  2) From Profile page:
//       navigate(`/quizzes/${r.quizId}/result`, { state: { result: r } })
//     -> we only have result summary, so we fetch full data
//        (quiz + answers) from backend via /api/Result/{resultId}/full.

import React, { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import type { Quiz, Question, Option, QuizResult } from "../types/quiz";
import ErrorAlert from "../components/ErrorAlert";
import { getFullResult, type AnswerMap } from "./QuizService";

type ResultLocationState =
  | {
      result?: QuizResult;
      quiz?: Quiz;
      answers?: AnswerMap;
      score?: number;
      maxScore?: number;
    }
  | null;

const QuizResultPage: React.FC = () => {
  const location = useLocation();
  const navigate = useNavigate();

  const state = location.state as ResultLocationState;

  // Whatever we got from navigation state
  const [result, setResult] = useState<QuizResult | null>(
    state?.result ?? null
  );
  const [quiz, setQuiz] = useState<Quiz | null>(state?.quiz ?? null);
  const [answers, setAnswers] = useState<AnswerMap | null>(
    state?.answers ?? null
  );

  const [error, setError] = useState<string | null>(null);
  const [loadingQuiz, setLoadingQuiz] = useState(false);

  // ------------------------------------------------------
  // If we only have result summary (coming from Profile),
  // fetch full result (quiz + answers) from backend.
  // If we already have quiz + answers (coming from Take),
  // we skip the fetch.
  // ------------------------------------------------------
  useEffect(() => {
    let cancelled = false;

    async function load() {
      // Coming directly from QuizTakePage -> we already have everything
      if (quiz && answers) return;
      if (!result) return;

      try {
        setLoadingQuiz(true);
        setError(null);

        // resultId is the backend ID from /my/results
        const full = await getFullResult(result.resultId);

        if (cancelled) return;

        // Replace local state with backend versions (in case they differ)
        setResult(full.result);
        setQuiz(full.quiz);
        setAnswers(full.answers);
      } catch (err: unknown) {
        if (cancelled) return;
        console.error("Failed to load full result", err);
        const msg =
          err instanceof Error
            ? err.message || "Failed to load quiz details."
            : "Failed to load quiz details.";
        setError(msg);
      } finally {
        if (!cancelled) setLoadingQuiz(false);
      }
    }

    void load();

    return () => {
      cancelled = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [result?.resultId]);

  // ------------------------------------------------------
  // No result at all -> we don't know what to display
  // (e.g. user hard refreshes /quizzes/:id/result)
  // ------------------------------------------------------
  if (!result) {
    return (
      <section className="page page-quiz-result">
        <h1 className="page-title">Quiz result</h1>
        <ErrorAlert message="Could not load this result in the browser. Open it right after finishing the quiz, or by clicking a result on your profile page." />
      </section>
    );
  }

  const percentage = Math.round(result.percentage);
  const completedAt = new Date(result.completedAt).toLocaleString("nb-NO", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
  });

  function handleExit() {
    navigate("/");
  }

  // ------------------------------------------------------
  // Render
  // ------------------------------------------------------
  return (
    <section className="page page-quiz-result">
      {/* Top bar – same structure as take-page */}
      <div className="quiz-top-bar">
        <p className="quiz-subject">{result.subjectCode}</p>
        <h1 className="quiz-title">{result.quizTitle}</h1>
        <button
          type="button"
          className="quiz-exit-btn"
          onClick={handleExit}
        >
          ×
        </button>
      </div>

      {/* Summary card */}
      <div className="quiz-result-summary">
        <div className="quiz-result-percent">{percentage}%</div>
        <div className="quiz-result-counts">
          <div>
            <strong>Correct:</strong> {result.correctCount} /{" "}
            {result.totalQuestions}
          </div>
          <div>
            <strong>Completed:</strong> {completedAt}</div>
        </div>
      </div>

      {/* Any error when loading full quiz details */}
      {error && <ErrorAlert message={error} />}

      {/* Loading text while fetching full result (profile path) */}
      {loadingQuiz && !quiz && (
        <p className="quiz-result-loading">Loading questions…</p>
      )}

      {/* Question cards – only when we have quiz + answers */}
      {quiz &&
        quiz.questions &&
        quiz.questions.length > 0 &&
        answers && (
          <>
            {quiz.questions.map((question: Question, index: number) => {
              const qId = question.id;
              if (qId == null) return null;

              const chosenIndex =
                answers && Object.prototype.hasOwnProperty.call(answers, qId)
                  ? answers[qId]
                  : null;

              const correctIndex = question.options.findIndex(
                (opt: Option) => opt.isCorrect
              );

              const isCorrect =
                chosenIndex != null && chosenIndex === correctIndex;

              return (
                <article
                  key={qId}
                  className={
                    "quiz-result-question-card " +
                    (isCorrect ? "correct" : "incorrect")
                  }
                >
                  <div className="quiz-question-header">
                    <p className="quiz-question-index">
                      Question {index + 1} • {question.points} point
                      {question.points !== 1 ? "s" : ""}
                    </p>
                  </div>

                  <h2 className="quiz-question-text">{question.text}</h2>

                  {question.imageUrl && (
                    <div className="quiz-question-image">
                      <img src={question.imageUrl} alt="Question" />
                    </div>
                  )}

                  <ul className="quiz-options-list">
                    {question.options.map((opt: Option, optIndex: number) => {
                      const isOptionCorrect = optIndex === correctIndex;
                      const isOptionChosen = optIndex === chosenIndex;

                      let optionClass = "quiz-option";
                      if (isOptionCorrect) optionClass += " quiz-option-correct";
                      if (isOptionChosen && !isOptionCorrect)
                        optionClass += " quiz-option-incorrect";
                      if (isOptionChosen) optionClass += " quiz-option-selected";

                      // Inline label text inside the option itself
                      let inlineLabel = "";
                      if (isOptionCorrect && isOptionChosen) {
                        inlineLabel = "– your answerd correct";
                      } else if (isOptionCorrect) {
                        inlineLabel = "– correct answer";
                      } else if (isOptionChosen) {
                        inlineLabel = "– your answer";
                      }

                      return (
                        <li key={optIndex} className={optionClass}>
                          <span>
                            {opt.text}
                            {inlineLabel && (
                              <span className="quiz-inline-label">{inlineLabel}</span>
                            )}
                          </span>
                        </li>
                      );
                    })}
                  </ul>
                </article>
              );
            })}
          </>
        )}
    </section>
  );
};

export default QuizResultPage;
