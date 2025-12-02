// src/quiz/QuizCreatePage.tsx
// ---------------------------
// Page for creating a new quiz.

import React, { useState } from "react";
import { Navigate, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/UseAuth";
import { createQuiz } from "./QuizService";
import type { Quiz } from "../types/quiz";
import ErrorAlert from "../components/ErrorAlert";

interface QuestionForm {
  id: number;                // Local id only used as a React key
  text: string;              // Question text shown to the user
  imageUrl: string;          // Optional image URL (may be empty string)
  options: string[];         // Text for the 4 answer options
  correctIndex: number | null; // Index of the correct option (0-3)
}

const QuizCreatePage: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();

  // Quiz-level form state
  const [subjectCode, setSubjectCode] = useState("");
  const [title, setTitle] = useState("");
  const [coverImageUrl, setCoverImageUrl] = useState("");
  const [description, setDescription] = useState("");

  // Question cards form state
  const [questions, setQuestions] = useState<QuestionForm[]>([
    {
      id: 1,
      text: "",
      imageUrl: "",
      options: ["", "", "", ""],
      correctIndex: null,
    },
  ]);

  // UI state for request handling
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [apiError, setApiError] = useState<string | null>(null);

  // Auth guard
  if (!user) {
    return <Navigate to="/login" replace />;
  }

  // Helper: add a new question card
  function addQuestionCard() {
    setQuestions((prev) => [
      ...prev,
      {
        id: prev.length + 1,
        text: "",
        imageUrl: "",
        options: ["", "", "", ""],
        correctIndex: null,
      },
    ]);
  }

  function updateQuestionText(id: number, text: string) {
    setQuestions((prev) =>
      prev.map((q) => (q.id === id ? { ...q, text } : q))
    );
  }

  function updateQuestionImage(id: number, imageUrl: string) {
    setQuestions((prev) =>
      prev.map((q) => (q.id === id ? { ...q, imageUrl } : q))
    );
  }

  function updateOptionText(
    questionId: number,
    optionIndex: number,
    text: string
  ) {
    setQuestions((prev) =>
      prev.map((q) => {
        if (q.id !== questionId) return q;
        const options = [...q.options];
        options[optionIndex] = text;
        return { ...q, options };
      })
    );
  }

  function updateCorrectIndex(questionId: number, optionIndex: number) {
    setQuestions((prev) =>
      prev.map((q) =>
        q.id === questionId ? { ...q, correctIndex: optionIndex } : q
      )
    );
  }

  // Client-side validation
  function validate(): boolean {
    if (!subjectCode.trim()) {
      setFormError("Subject code / course code is required.");
      return false;
    }
    if (!title.trim()) {
      setFormError("Title is required.");
      return false;
    }
    if (!coverImageUrl.trim()) {
      setFormError("Quiz cover image URL is required.");
      return false;
    }
    if (!description.trim()) {
      setFormError("Description is required.");
      return false;
    }
    if (questions.length === 0) {
      setFormError("At least one question card is required.");
      return false;
    }

    for (const [index, q] of questions.entries()) {
      if (!q.text.trim()) {
        setFormError(`Question ${index + 1}: text is required.`);
        return false;
      }
      if (q.options.some((opt) => !opt.trim())) {
        setFormError(`Question ${index + 1}: all 4 options must be filled.`);
        return false;
      }
      if (q.correctIndex === null) {
        setFormError(
          `Question ${index + 1}: you must select which option is correct.`
        );
        return false;
      }
    }

    setFormError(null);
    return true;
  }

  // Submit handler
  async function handleSubmit(action: "create" | "createAndTake") {
    if (!validate()) return;

    setSaving(true);
    setApiError(null);

    // Build the Quiz object exactly as the backend expects.
    const quiz: Quiz = {
      subjectCode: subjectCode.trim(),
      title: title.trim(),
      description: description.trim(),
      imageUrl: coverImageUrl.trim(),
      isPublished: true, // Mark quizzes as published immediately
      questions: questions.map((q) => ({
        text: q.text.trim(),
        imageUrl: q.imageUrl.trim() || undefined,
        points: 1,
        options: q.options.map((optText, idx) => ({
          text: optText.trim(),
          isCorrect: q.correctIndex === idx,
        })),
      })),
    };

    try {
      const created = await createQuiz(quiz);
      const quizId = created.id;

      if (action === "createAndTake" && quizId != null) {
        navigate(`/quizzes/${quizId}/take`);
      } else {
        navigate("/");
      }
    } catch (err: unknown) {
      if (err instanceof Error) {
        setApiError(err.message || "Failed to create quiz.");
      } else {
        setApiError("Failed to create quiz.");
      }
    } finally {
      setSaving(false);
    }
  }

  // Render
  return (
    <section className="page page-quiz-create">
      <h1 className="page-title">Create a New Quiz</h1>

      {formError && <ErrorAlert message={formError} />}
      {apiError && <ErrorAlert message={apiError} />}

      <form className="form" onSubmit={(e) => e.preventDefault()}>
        {/* Subject / course code */}
        <div className="form-field">
          <label htmlFor="subjectCode">Subject / course code *</label>
          <input
            id="subjectCode"
            type="text"
            placeholder="e.g. ITPE3200"
            value={subjectCode}
            onChange={(e) => setSubjectCode(e.target.value)}
            disabled={saving}
          />
        </div>

        {/* Quiz title */}
        <div className="form-field">
          <label htmlFor="title">Quiz title *</label>
          <input
            id="title"
            type="text"
            placeholder="Enter quiz title..."
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            disabled={saving}
          />
        </div>

        {/* Quiz cover image URL */}
        <div className="form-field">
          <label htmlFor="coverImageUrl">Quiz image URL *</label>
          <input
            id="coverImageUrl"
            type="text"
            placeholder="https://example.com/my-quiz-image.jpg"
            value={coverImageUrl}
            onChange={(e) => setCoverImageUrl(e.target.value)}
            disabled={saving}
          />
        </div>

        {/* Description */}
        <div className="form-field">
          <label htmlFor="description">Description *</label>
          <textarea
            id="description"
            placeholder="Describe what this quiz is about..."
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            disabled={saving}
            rows={3}
          />
        </div>

        {/* Question cards */}
        <h2>Question cards</h2>

        {questions.map((q, index) => {
          const questionTextId = `question-${q.id}-text`;
          const imageId = `question-${q.id}-image`;

          return (
            <div key={q.id} className="quiz-question-card">
              <p className="quiz-question-index">Question {index + 1}</p>

              {/* Question text */}
              <div className="form-field">
                <label htmlFor={questionTextId}>Question text *</label>
                <input
                  id={questionTextId}
                  type="text"
                  value={q.text}
                  onChange={(e) => updateQuestionText(q.id, e.target.value)}
                  disabled={saving}
                />
              </div>

              {/* Optional image URL */}
              <div className="form-field">
                <label htmlFor={imageId}>Image URL (optional)</label>
                <input
                  id={imageId}
                  type="text"
                  placeholder="https://..."
                  value={q.imageUrl}
                  onChange={(e) => updateQuestionImage(q.id, e.target.value)}
                  disabled={saving}
                />
              </div>

              {/* Answer options with radio button to select the correct one */}
              <div className="form-field">
                <p>Answer options (one must be correct)</p>
                {q.options.map((opt, idx) => (
                  <label key={idx} className="quiz-option-edit">
                    <input
                      type="radio"
                      name={`correct-${q.id}`}
                      checked={q.correctIndex === idx}
                      onChange={() => updateCorrectIndex(q.id, idx)}
                      disabled={saving}
                    />
                    <input
                      type="text"
                      placeholder={`Option ${idx + 1}`}
                      value={opt}
                      onChange={(e) =>
                        updateOptionText(q.id, idx, e.target.value)
                      }
                      disabled={saving}
                    />
                  </label>
                ))}
              </div>
            </div>
          );
        })}

        <button
          type="button"
          className="btn btn-secondary"
          disabled={saving}
          onClick={addQuestionCard}
        >
          + Add question card
        </button>

        <div className="form-actions">
          <button
            type="button"
            className="btn btn-primary"
            disabled={saving}
            onClick={() => void handleSubmit("createAndTake")}
          >
            {saving ? "Saving..." : "Create and take quiz"}
          </button>

          <button
            type="button"
            className="btn btn-secondary"
            disabled={saving}
            onClick={() => void handleSubmit("create")}
          >
            {saving ? "Saving..." : "Create"}
          </button>
        </div>
      </form>
    </section>
  );
};

export default QuizCreatePage;
