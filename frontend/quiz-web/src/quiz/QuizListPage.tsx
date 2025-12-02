// src/quiz/QuizListPage.tsx
// --------------------------
// Page for listing all quizzes (the "Read" part of CRUD).

import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { getQuizzes, deleteQuiz } from "./QuizService";
import type { QuizSummary } from "../types/quiz";
import { useAuth } from "../auth/UseAuth";
import Loader from "../components/Loader";
import ErrorAlert from "../components/ErrorAlert";
import ConfirmDialog from "../components/ConfirmDialog";

const QuizListPage: React.FC = () => {
  const { user } = useAuth();

  const [quizzes, setQuizzes] = useState<QuizSummary[]>([]);
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [confirmOpen, setConfirmOpen] = useState(false);
  const [quizToDelete, setQuizToDelete] = useState<QuizSummary | null>(null);

  // Load quizzes once on mount
  useEffect(() => {
    void loadQuizzes();
  }, []);

  async function loadQuizzes() {
    try {
      setLoading(true);
      setError(null);

      const data = await getQuizzes();
      setQuizzes(data);
    } catch (err: unknown) {
      if (err instanceof Error) {
        setError(err.message || "Failed to load quizzes.");
      } else {
        setError("Failed to load quizzes.");
      }
    } finally {
      setLoading(false);
    }
  }

  function openDeleteConfirm(quiz: QuizSummary) {
    setQuizToDelete(quiz);
    setConfirmOpen(true);
  }

  function closeDeleteConfirm() {
    setConfirmOpen(false);
    setQuizToDelete(null);
  }

  async function confirmDelete() {
    if (!quizToDelete || quizToDelete.id == null) {
      closeDeleteConfirm();
      return;
    }

    try {
      await deleteQuiz(quizToDelete.id);
      setQuizzes((prev) => prev.filter((q) => q.id !== quizToDelete.id));
    } catch (err: unknown) {
      if (err instanceof Error) {
        alert(err.message || "Failed to delete quiz.");
      } else {
        alert("Failed to delete quiz.");
      }
    } finally {
      closeDeleteConfirm();
    }
  }

  // Client-side filtering by title
  const filtered = quizzes.filter((quiz) => {
    const term = search.trim().toLowerCase();
    if (!term) return true; // hvis søkefeltet er tomt, vis alle

    const title = quiz.title?.toLowerCase() ?? "";
    const subject = quiz.subjectCode?.toLowerCase() ?? "";
    const description = quiz.description?.toLowerCase() ?? "";

    return (
      title.includes(term) ||
      subject.includes(term) ||
      description.includes(term)
    );
  });

  return (
    <section className="page page-quiz-list">
      <div className="page-header">
        <h1 className="page-title">Quizzes</h1>

        {user && (
          <Link to="/quizzes/create" className="btn btn-primary">
            Create New Quiz
          </Link>
        )}
      </div>

      {/* Search/filter + refresh */}
      <div className="quiz-search">
        <input
          type="text"
          placeholder="Search by course code, title or description..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />

        <button
          type="button"
          className="btn btn-secondary"
          onClick={() => loadQuizzes()}
        >
          Refresh
        </button>
      </div>

      {loading && <Loader />}
      {error && <ErrorAlert message={error} />}

      {!loading && !error && filtered.length === 0 && (
        <p>No quizzes found.</p>
      )}

      {!loading && !error && filtered.length > 0 && (
        <table className="quiz-table">
          <thead>
            <tr>
              <th>Title</th>
              <th>Questions</th>
              <th>Total Points</th>
              <th>Published</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {filtered.map((quiz) => (
              <tr key={quiz.id}>
                <td>{quiz.title}</td>
                <td>{quiz.questionCount}</td>
                <td>{quiz.totalPoints}</td>
                <td>{quiz.isPublished ? "Yes" : "No"}</td>
                <td className="quiz-actions">
                  <Link
                    to={`/quizzes/${quiz.id}/take`}
                    className="btn btn-secondary"
                  >
                    Take
                  </Link>

                  {user && (
                    <>
                      <Link
                        to={`/quizzes/${quiz.id}/edit`}
                        className="btn btn-secondary"
                      >
                        Edit
                      </Link>
                      <button
                        type="button"
                        className="btn btn-secondary"
                        onClick={() => openDeleteConfirm(quiz)}
                      >
                        Delete
                      </button>
                    </>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      <ConfirmDialog
        open={confirmOpen}
        title="Delete quiz"
        message={
          quizToDelete
            ? `Are you sure you want to delete the quiz "${quizToDelete.title}"?`
            : "Are you sure you want to delete this quiz?"
        }
        confirmText="Delete"
        cancelText="Cancel"
        onConfirm={confirmDelete}
        onCancel={closeDeleteConfirm}
      />
    </section>
  );
};

export default QuizListPage;
