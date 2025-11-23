// src/quiz/QuizEditPage.tsx
// --------------------------
// Page for editing an existing quiz.
// Loads the quiz from the backend and lets an authenticated user
// update basic metadata (title, description, isPublished).

import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Navigate } from "react-router-dom";
import { getQuiz, updateQuiz } from "./QuizService";
import type { Quiz } from "../types/quiz";
import { useAuth } from "../auth/UseAuth";
import Loader from "../components/Loader";
import ErrorAlert from "../components/ErrorAlert";

const QuizEditPage: React.FC = () => {
  // All hooks MUST be at the top of the component, before any early returns.
  const { user } = useAuth();
  const navigate = useNavigate();
  const { id } = useParams();

  // Loading state for initial fetch
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);

  // Form fields
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [isPublished, setIsPublished] = useState(false);

  // UI state for submitting updates
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [apiError, setApiError] = useState<string | null>(null);

  // Load quiz data when component mounts or id changes
  useEffect(() => {
    async function load() {
      // If the route param is missing, we cannot load anything.
      if (!id) {
        setLoadError("Invalid quiz ID.");
        setLoading(false);
        return;
      }

      try {
        const quiz = await getQuiz(Number(id));

        setTitle(quiz.title);
        setDescription(quiz.description ?? "");
        setIsPublished(quiz.isPublished);
      } catch (err: unknown) {
        if (err instanceof Error) {
          setLoadError(err.message || "Failed to load quiz.");
        } else {
          setLoadError("Failed to load quiz.");
        }
      } finally {
        setLoading(false);
      }
    }

    void load();
  }, [id]);

  // After all hooks: early return based on auth
  if (!user) {
    // Not logged in -> send user to login page
    return <Navigate to="/login" replace />;
  }

  // Handle initial loading / error states
  if (loading) {
    return (
      <section className="page page-quiz-edit">
        <h1 className="page-title">Edit Quiz</h1>
        <Loader />
      </section>
    );
  }

  if (loadError) {
    return (
      <section className="page page-quiz-edit">
        <h1 className="page-title">Edit Quiz</h1>
        <ErrorAlert message={loadError} />
      </section>
    );
  }

  // Simple validation for the form
  function validate(): boolean {
    if (!title.trim()) {
      setFormError("Title is required.");
      return false;
    }
    setFormError(null);
    return true;
  }

  // Handle submit (PUT /api/quizzes/{id})
  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault();

    if (!validate()) return;
    if (!id) return; // extra safety

    setSaving(true);
    setApiError(null);

    const updatedQuiz: Quiz = {
      id: Number(id),
      title: title.trim(),
      description: description.trim() || undefined,
      isPublished,
      // Questions editing is not implemented yet, so we send an empty list for now.
      questions: [],
    };

    try {
      await updateQuiz(Number(id), updatedQuiz);
      navigate("/quizzes");
    } catch (err: unknown) {
      if (err instanceof Error) {
        setApiError(err.message || "Failed to update quiz.");
      } else {
        setApiError("Failed to update quiz.");
      }
    } finally {
      setSaving(false);
    }
  }

  return (
    <section className="page page-quiz-edit">
      <h1 className="page-title">Edit Quiz</h1>

      {formError && <ErrorAlert message={formError} />}
      {apiError && <ErrorAlert message={apiError} />}

      <form className="form" onSubmit={handleSubmit}>
        <div className="form-field">
          <label htmlFor="title">Title *</label>
          <input
            id="title"
            type="text"
            value={title}
            disabled={saving}
            onChange={(e) => setTitle(e.target.value)}
          />
        </div>

        <div className="form-field">
          <label htmlFor="description">Description</label>
          <textarea
            id="description"
            rows={3}
            value={description}
            disabled={saving}
            onChange={(e) => setDescription(e.target.value)}
          />
        </div>

        <div className="form-field checkbox-field">
          <label>
            <input
              type="checkbox"
              checked={isPublished}
              disabled={saving}
              onChange={(e) => setIsPublished(e.target.checked)}
            />
            <span>Published (visible to users)</span>
          </label>
        </div>

        <div className="form-actions">
          <button type="submit" className="btn btn-primary" disabled={saving}>
            {saving ? "Saving..." : "Save Changes"}
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

export default QuizEditPage;
