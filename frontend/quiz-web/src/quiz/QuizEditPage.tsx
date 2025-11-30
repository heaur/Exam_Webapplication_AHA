// src/quiz/QuizEditPage.tsx
// --------------------------
// Page for editing an existing quiz.
//
// Responsibilities:
// - Only authenticated users may access this page.
// - Loads an existing quiz from the backend using its ID from the URL.
// - Lets the user update basic metadata:
//     * subjectCode (course code)
//     * title
//     * description
//     * isPublished
// - Sends the changes back to the backend via PUT /api/Quiz/{id}.
//
// NOTE:
//   Editing questions is *not* implemented here. We only update quiz metadata
//   and send an empty questions array on update. The backend is expected to
//   keep the existing questions when questions = [].

import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Navigate } from "react-router-dom";
import { getQuiz, updateQuiz } from "./QuizService";
import type { Quiz } from "../types/quiz";
import { useAuth } from "../auth/UseAuth";
import Loader from "../components/Loader";
import ErrorAlert from "../components/ErrorAlert";

const QuizEditPage: React.FC = () => {
  // ------------------------
  // Hooks (must be at the top)
  // ------------------------
  const { user } = useAuth();
  const navigate = useNavigate();
  const { id } = useParams();

  // ------------------------
  // Loading state for initial fetch
  // ------------------------
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);

  // ------------------------
  // Form fields (quiz metadata)
  // ------------------------
  const [subjectCode, setSubjectCode] = useState(""); // course code, e.g. "DATA1700"
  const [title, setTitle] = useState("");             // quiz title
  const [description, setDescription] = useState(""); // optional description
  const [isPublished, setIsPublished] = useState(false);

  // ------------------------
  // UI state for submitting updates
  // ------------------------
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [apiError, setApiError] = useState<string | null>(null);

  // ------------------------
  // Load quiz data when component mounts or id changes
  // ------------------------
  useEffect(() => {
    async function load() {
      // If the route param is missing, we cannot load anything.
      if (!id) {
        setLoadError("Invalid quiz ID.");
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setLoadError(null);

        const quiz = await getQuiz(Number(id));

        // Populate form fields with data from the backend quiz
        setSubjectCode(quiz.subjectCode ?? "");
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

  // ------------------------
  // Auth guard (after hooks)
  // ------------------------
  if (!user) {
    // User is not authenticated -> redirect to login.
    return <Navigate to="/login" replace />;
  }

  // ------------------------
  // Initial loading / error states
  // ------------------------
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

  // ------------------------
  // Simple form validation
  // ------------------------
  function validate(): boolean {
    if (!subjectCode.trim()) {
      setFormError("Subject / course code is required.");
      return false;
    }
    if (!title.trim()) {
      setFormError("Title is required.");
      return false;
    }

    setFormError(null);
    return true;
  }

  // ------------------------
  // Handle submit (PUT /api/Quiz/{id})
  // ------------------------
  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault();

    if (!validate()) return;
    if (!id) return; // Extra safety; should never happen if we got this far.

    setSaving(true);
    setApiError(null);

    // Build a Quiz object that matches the Quiz interface exactly.
    // We only update metadata; questions editing is not supported here,
    // so we send an empty questions array and let the backend keep the old ones.
    const updatedQuiz: Quiz = {
      id: Number(id),
      subjectCode: subjectCode.trim(),
      title: title.trim(),
      description: description.trim() || undefined,
      isPublished,
      questions: [],
    };

    try {
      await updateQuiz(Number(id), updatedQuiz);
      // After successful update, go back to the quiz list.
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

  // ------------------------
  // Render
  // ------------------------
  return (
    <section className="page page-quiz-edit">
      <h1 className="page-title">Edit Quiz</h1>

      {/* Top-level form + API errors */}
      {formError && <ErrorAlert message={formError} />}
      {apiError && <ErrorAlert message={apiError} />}

      <form className="form" onSubmit={handleSubmit}>
        {/* Subject / course code */}
        <div className="form-field">
          <label htmlFor="subjectCode">Subject / course code *</label>
          <input
            id="subjectCode"
            type="text"
            placeholder="e.g. DATA1700"
            value={subjectCode}
            disabled={saving}
            onChange={(e) => setSubjectCode(e.target.value)}
          />
        </div>

        {/* Quiz title */}
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

        {/* Optional description */}
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

        {/* Published flag */}
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

        {/* Actions */}
        <div className="form-actions">
          <button
            type="submit"
            className="btn btn-primary"
            disabled={saving}
          >
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
