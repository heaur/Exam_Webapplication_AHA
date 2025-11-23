// src/quiz/QuizListPage.tsx
// --------------------------
// Page for listing all quizzes (the "Read" part of CRUD).
// Demonstrates:
// - Loading state
// - Error handling
// - Client-side search/filter
// - Basic actions (view, edit, delete)
// - Conditional UI based on authentication state

import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { getQuizzes, deleteQuiz } from "./QuizService";
import type { QuizSummary } from "../types/quiz";
import { useAuth } from "../auth/UseAuth";
import Loader from "../components/Loader";
import ErrorAlert from "../components/ErrorAlert";
import ConfirmDialog from "../components/ConfirmDialog";


const QuizListPage: React.FC = () => {
  // Current authenticated user (or null if not logged in)
  const { user } = useAuth();

  // All quizzes returned from the backend
  const [quizzes, setQuizzes] = useState<QuizSummary[]>([]);
  // The current value of the search/filter input
  const [search, setSearch] = useState("");
  // Indicates whether we are currently loading data from the backend
  const [loading, setLoading] = useState(true);
  // Holds an error message if something goes wrong while loading
  const [error, setError] = useState<string | null>(null);
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [quizToDelete, setQuizToDelete] = useState<QuizSummary | null>(null);


  // Load quizzes once when the component is first rendered
  useEffect(() => {
    void loadQuizzes();
  }, []);

  /**
   * Loads quizzes from the backend API and updates local state.
   * This function is also used when the user clicks the "Refresh" button.
   */
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

  /**
   * Handles the delete action for a single quiz.
   * Asks the user for confirmation and then calls the API.
   * On success, the quiz is removed from the local state.
   */
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
      // Fjern quizen lokalt
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

  /**
   * Client-side filtering by quiz title (case-insensitive).
   * This satisfies the requirement for search/filtering in the frontend.
   */
  const filtered = quizzes.filter((quiz) =>
    quiz.title.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <section className="page page-quiz-list">
      <div className="page-header">
        <h1 className="page-title">Quizzes</h1>

        {/* Only show "Create New Quiz" button if the user is logged in */}
        {user && (
          <Link to="/quizzes/create" className="btn btn-primary">
            Create New Quiz
          </Link>
        )}
      </div>

      {/* Search/filter input and refresh button */}
      <div className="quiz-search">
        <input
          type="text"
          placeholder="Search quizzes by title..."
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

      {/* Loading and error states */}
      {loading && <Loader />}
      {error && <ErrorAlert message={error} />}

      {/* No quizzes found after filtering (and not loading, and no error) */}
      {!loading && !error && filtered.length === 0 && (
        <p>No quizzes found.</p>
      )}

      {/* Quiz table */}
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
                  {/* View/take quiz link - allowed for everyone */}
                  <Link
                    to={`/quizzes/${quiz.id}/take`}
                    className="btn btn-secondary"
                  >
                    Take
                  </Link>

                  {/* Edit/Delete actions - only for logged in users */}
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
