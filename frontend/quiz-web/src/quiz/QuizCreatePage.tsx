// src/quiz/QuizCreatePage.tsx
// ---------------------------
// Page for creating a new quiz.
// For now we only edit basic metadata (title, description, isPublished).
// Questions can be added later. We still send an empty questions array
// so that the object matches the Quiz interface.

// src/quiz/QuizCreatePage.tsx

import React, { useState } from "react";
import { useNavigate, Navigate } from "react-router-dom";
import { createQuiz } from "./QuizService";
import type { Quiz } from "../types/quiz";
import { useAuth } from "../auth/AuthContext";
import ErrorAlert from "../components/ErrorAlert";

const QuizCreatePage: React.FC = () => {
  // Hooks MUST always be at the top level of the component
  const { user } = useAuth();
  const navigate = useNavigate();

  // Local form state
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [isPublished, setIsPublished] = useState(false);

  // UI state for request handling
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [apiError, setApiError] = useState<string | null>(null);

  // After all hooks are declared, you can branch/return
  if (!user) {
    return <Navigate to="/login" replace />;
  }

  function validate(): boolean {
    if (!title.trim()) {
      setFormError("Title is required.");
      return false;
    }
    setFormError(null);
    return true;
  }

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault();

    if (!validate()) return;

    setSaving(true);
    setApiError(null);

    const newQuiz: Quiz = {
      title: title.trim(),
      description: description.trim() || undefined,
      isPublished,
      questions: [],
    };

    try {
      await createQuiz(newQuiz);
      navigate("/quizzes");
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

  return (
    <section className="page page-quiz-create">
      <h1 className="page-title">Create a New Quiz</h1>

      {formError && <ErrorAlert message={formError} />}
      {apiError && <ErrorAlert message={apiError} />}

      <form className="form" onSubmit={handleSubmit}>
        <div className="form-field">
          <label htmlFor="title">Title *</label>
          <input
            id="title"
            type="text"
            placeholder="Enter quiz title..."
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            disabled={saving}
          />
        </div>

        <div className="form-field">
          <label htmlFor="description">Description</label>
          <textarea
            id="description"
            placeholder="Optional: describe what this quiz is about..."
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            disabled={saving}
            rows={3}
          />
        </div>

        <div className="form-field checkbox-field">
          <label>
            <input
              type="checkbox"
              checked={isPublished}
              onChange={(e) => setIsPublished(e.target.checked)}
              disabled={saving}
            />
            <span>Published (visible to users)</span>
          </label>
        </div>

        <div className="form-actions">
          <button
            type="submit"
            className="btn btn-primary"
            disabled={saving}
          >
            {saving ? "Saving..." : "Create Quiz"}
          </button>

          <button
            type="button"
            className="btn btn-secondary"
            disabled={saving}
            onClick={() => navigate("/quizzes")}
          >
            Cancel
          </button>
        </div>
      </form>
    </section>
  );
};

export default QuizCreatePage;
