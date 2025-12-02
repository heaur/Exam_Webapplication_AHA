// src/quiz/QuizResultPage.tsx
// ---------------------------
// Result page shown after taking a quiz.
// - No navbar (NavMenu hides itself on this route).
// - Shows subjectCode + title.
// - Shows percentage, number of correct and wrong questions.
// - Shows each question card with red/green depending on correctness.

import React from "react";
import { useLocation, useNavigate, Navigate } from "react-router-dom";
import type { Quiz } from "../types/quiz";

type AnswerMap = Record<number, number | null>; // questionId -> optionIndex

interface ResultLocationState {
  quiz: Quiz;
  answers: AnswerMap;
  score: number;
  maxScore: number;
}

const QuizResultPage: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();

  const state = location.state as ResultLocationState | null;

  if (!state || !state.quiz) {
    // No state -> user navigated directly here -> send home
    return <Navigate to="/" replace />;
  }

  const { quiz, answers, score, maxScore } = state;

  const totalQuestions = quiz.questions.length;
  let correctQuestions = 0;

  quiz.questions.forEach((question) => {
    const qId = question.id;
    if (qId == null) return;

    const chosenIndex = answers[qId];
    const correctIndex = question.options.findIndex((opt) => opt.isCorrect);

    if (chosenIndex != null && chosenIndex === correctIndex) {
      correctQuestions += 1;
    }
  });

  const wrongQuestions = totalQuestions - correctQuestions;
  const percentage =
    maxScore > 0 ? Math.round((score / maxScore) * 100) : 0;

  function handleExit() {
    navigate("/");
  }

  return (
    <section className="page page-quiz-result">
      {/* Full-width top bar: course (left) – title (center) – X (right) */}
      <div className="quiz-top-bar">
        <p className="quiz-top-course">{quiz.subjectCode}</p>

        <h1 className="quiz-top-title">{quiz.title}</h1>

        <button
          type="button"
          className="quiz-exit-btn"
          onClick={handleExit}
        >
          ×
        </button>
      </div>

      <div className="quiz-result">
        <p>
          You scored <strong>{score}</strong> out of{" "}
          <strong>{maxScore}</strong> points (
          <strong>{percentage}%</strong>).
        </p>
        <p>
          Correct questions: <strong>{correctQuestions}</strong> • Wrong
          questions: <strong>{wrongQuestions}</strong>
        </p>
      </div>

      {quiz.questions.map((question, index) => {
        const qId = question.id;
        if (qId == null) return null;

        const chosenIndex = answers[qId];
        const correctIndex = question.options.findIndex((opt) => opt.isCorrect);

        const isCorrect =
          chosenIndex != null && chosenIndex === correctIndex;

        let cardClass = "quiz-question-card";
        if (isCorrect) {
          cardClass += " quiz-option-correct";
        } else {
          cardClass += " quiz-option-incorrect";
        }

        return (
          <div key={qId} className={cardClass}>
            <p className="quiz-question-index">
              Question {index + 1}
            </p>
            <h2 className="quiz-question-text">{question.text}</h2>

            {question.imageUrl && (
              <div className="quiz-question-image">
                <img src={question.imageUrl} alt="Question" />
              </div>
            )}

            <ul className="quiz-options-list">
              {question.options.map((opt, optIndex) => {
                const isChosen = chosenIndex === optIndex;
                const isCorrectOption = correctIndex === optIndex;

                let optionClass = "quiz-option";
                if (isCorrectOption) {
                  optionClass += " quiz-option-correct";
                } else if (isChosen && !isCorrectOption) {
                  optionClass += " quiz-option-incorrect";
                }

                return (
                  <li key={optIndex} className={optionClass}>
                    <span>
                      {opt.text}
                      {isChosen && " (your answer)"}
                      {isCorrectOption && " (correct answer)"}
                    </span>
                  </li>
                );
              })}
            </ul>
          </div>
        );
      })}
    </section>
  );
};

export default QuizResultPage;
