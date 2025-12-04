// src/quiz/QuizTakePage.tsx
// --------------------------
// Page for taking a quiz.

import React, { useEffect, useState } from "react";
import { Navigate, useNavigate, useParams } from "react-router-dom";
import { getQuiz, submitResult, type AnswerMap } from "./QuizService";
import type { Quiz, Question, Option, QuizResult } from "../types/quiz";
import { useAuth } from "../auth/UseAuth";
import Loader from "../components/Loader";
import ErrorAlert from "../components/ErrorAlert";

const QuizTakePage: React.FC = () => {
  const { id } = useParams();
  const { user } = useAuth();
  const navigate = useNavigate();

  const [quiz, setQuiz] = useState<Quiz | null>(null);
  const [answers, setAnswers] = useState<AnswerMap>({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // ------------------------------------------------------
  // Load quiz from backend
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

        const q = await getQuiz(Number(id));
        setQuiz(q);

        // Initialise the answer map: one entry per question id
        const initialAnswers: AnswerMap = {};
        for (const question of q.questions) {
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
  // Auth guard
  // ------------------------------------------------------
  if (!user) {
    return <Navigate to="/login" replace />;
  }

  // ------------------------------------------------------
  // Handlers
  // ------------------------------------------------------
  function handleExit() {
    navigate("/");
  }

  function handleSelectOption(questionId: number, optionIndex: number) {
    // Update selected option index for one question
    setAnswers((prev) => ({
      ...prev,
      [questionId]: optionIndex,
    }));
  }

  // ------------------------------------------------------
  // Submit quiz: send to backend + navigate to result page
  // ------------------------------------------------------
  async function handleSubmitQuiz() {
    if (!quiz || !quiz.id) return;

    const questionList: Question[] = quiz.questions ?? [];

    let correctCount = 0;
    let totalQuestions = 0;
    let score = 0; // local score
    let maxScore = 0; // sum of question points

    // Answers payload for backend: questionId -> optionId (DB id)
    const answersForApi: Record<number, number> = {};

    for (const question of questionList) {
      const qId = question.id;
      if (qId == null) continue;

      totalQuestions += 1;
      const points = question.points ?? 1;
      maxScore += points;

      const chosenIndex = answers[qId];
      const correctIndex = question.options.findIndex(
        (opt: Option) => opt.isCorrect
      );

      // Map chosen *index* -> OptionId for backend
      if (chosenIndex != null) {
        const chosenOption = question.options[chosenIndex];
        if (chosenOption && chosenOption.id != null) {
          answersForApi[qId] = chosenOption.id;
        }
      }

      // Local scoring logic
      if (chosenIndex != null && chosenIndex === correctIndex) {
        correctCount += 1;
        score += points;
      }
    }

    // 1) Persist result in backend (summary + per-question answers)
    try {
      await submitResult({
        quizId: quiz.id,
        correctCount,
        totalQuestions,
        answers: answersForApi,
      });
    } catch (err) {
      console.error("Failed to submit quiz result", err);
      alert("Failed to submit quiz result. Please try again.");
      return;
    }

    // 2) Build local QuizResult object – same shape as getMyResults()
    //    Backend will have its own ResultId, so we just use 0 here.
    const result: QuizResult = {
      resultId: 0,
      userId: undefined,
      quizId: quiz.id,
      quizTitle: quiz.title,
      subjectCode: quiz.subjectCode,
      correctCount,
      totalQuestions,
      completedAt: new Date().toISOString(),
      percentage:
        totalQuestions > 0 ? (correctCount / totalQuestions) * 100 : 0,
    };

    // 3) Navigate to result page with everything needed for immediate view:
    //    - result: summary block
    //    - quiz + answers + score/maxScore: question cards with styling
    navigate(`/quizzes/${quiz.id}/result`, {
      state: {
        result,
        quiz,
        answers,
        score,
        maxScore,
      },
    });
  }

  // ------------------------------------------------------
  // Render: loading / error / quiz
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

  const questionList: Question[] = quiz.questions ?? [];

  return (
    <section className="page page-quiz-take">
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

      {questionList.map((question, index) => {
        const qId = question.id;
        if (qId == null) return null;

        const selectedIndex = answers[qId];

        return (
          <div key={qId} className="quiz-question-card">
            <p className="quiz-question-index">
              Question {index + 1} • {question.points} point
              {question.points !== 1 ? "s" : ""}
            </p>

            <h2 className="quiz-question-text">{question.text}</h2>

            {question.imageUrl && (
              <div className="quiz-question-image">
                <img src={question.imageUrl} alt="Question" />
              </div>
            )}

            <ul className="quiz-options-list">
              {question.options.map((opt: Option, optIndex: number) => (
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
