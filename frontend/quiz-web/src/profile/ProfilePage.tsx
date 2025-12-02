// src/profile/ProfilePage.tsx
// ----------------------------
// "My profile" page.
// Shows:
// - Logged-in user (from AuthContext)
// - Collapsible sections for user info, results, and quizzes
// - Logout button at the bottom.

import "../styles/profile.css";

import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

import { useAuth } from "../auth/UseAuth";
import { getMyResults, getMyQuizzes, deleteQuiz } from "../quiz/QuizService";
import type { QuizSummary, QuizResult } from "../types/quiz";

const ProfilePage: React.FC = () => {
  const navigate = useNavigate();
  const { user, logout } = useAuth();

  const [results, setResults] = useState<QuizResult[]>([]);
  const [quizzes, setQuizzes] = useState<QuizSummary[]>([]);

  const [loadingResults, setLoadingResults] = useState(true);
  const [loadingQuizzes, setLoadingQuizzes] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [deletingId, setDeletingId] = useState<number | null>(null);

  // Local UI state: which sections are expanded
  const [openUserInfo, setOpenUserInfo] = useState(true);
  const [openResults, setOpenResults] = useState(true);
  const [openQuizzes, setOpenQuizzes] = useState(true);

  // Local search filters
  const [resultsFilter, setResultsFilter] = useState("");
  const [quizzesFilter, setQuizzesFilter] = useState("");

  // Helper: nice display name for the header
  const displayName =
    user?.userName?.slice(0, 8) ||
    (user?.email ? user.email.split("@")[0].slice(0, 8) : "User");

  // --------------------------------------------------
  // Load data on mount
  // --------------------------------------------------
  useEffect(() => {
    let isCancelled = false;

    async function loadData() {
      try {
        setError(null);
        setLoadingResults(true);
        setLoadingQuizzes(true);

        const [quizRes, resultRes] = await Promise.all([
          getMyQuizzes(),
          getMyResults(),
        ]);

        if (isCancelled) return;

        setQuizzes(quizRes);
        setResults(resultRes);
      } catch (err: unknown) {
        if (isCancelled) return;

        console.error("Failed to load profile data", err);
        const msg =
          err instanceof Error
            ? err.message ?? "Failed to load profile data."
            : "Failed to load profile data.";
        setError(msg);
      } finally {
        if (!isCancelled) {
          setLoadingQuizzes(false);
          setLoadingResults(false);
        }
      }
    }

    void loadData();
    return () => {
      isCancelled = true;
    };
  }, []);

  // --------------------------------------------------
  // Handlers
  // --------------------------------------------------
  async function handleDeleteQuiz(id: number) {
    const confirmed = window.confirm(
      "Are you sure you want to delete this quiz? This cannot be undone."
    );
    if (!confirmed) return;

    try {
      setDeletingId(id);
      await deleteQuiz(id);
      setQuizzes((prev) => prev.filter((q) => q.id !== id));
    } catch (err: unknown) {
      console.error("Failed to delete quiz", err);
      const msg =
        err instanceof Error
          ? err.message ?? "Failed to delete quiz."
          : "Failed to delete quiz.";
      setError(msg);
    } finally {
      setDeletingId(null);
    }
  }

  async function handleLogoutClick() {
    await logout();
    navigate("/");
  }

  if (!user) {
    // Route should be protected, but guard anyway.
    navigate("/login");
    return null;
  }

  // Filtered lists based on search query
  const filteredResults = resultsFilter
    ? results.filter((r) =>
        (r.quizTitle + " " + r.subjectCode)
          .toLowerCase()
          .includes(resultsFilter.toLowerCase())
      )
    : results;

  const filteredQuizzes = quizzesFilter
    ? quizzes.filter((q) =>
        (q.title + " " + q.subjectCode)
          .toLowerCase()
          .includes(quizzesFilter.toLowerCase())
      )
    : quizzes;

  return (
    <section className="profile-page">
      {/* Single outer card */}
      <div className="profile-card">
        {/* Header */}
        <header className="profile-header">
          <h1 className="page-title">My profile</h1>
          <p className="page-description">
            Signed in as <strong>{displayName}</strong>.
          </p>
        </header>

        {/* Error banner, if any */}
        {error && <div className="profile-error">{error}</div>}

        {/* USER INFO SECTION */}
        <section className="profile-section">
          <button
            type="button"
            className="profile-section-header"
            onClick={() => setOpenUserInfo((v) => !v)}
          >
            <h2 className="profile-section-title">Brukeropplysninger</h2>
            <span className="profile-section-toggle">
              {openUserInfo ? "▴" : "▾"}
            </span>
          </button>

          {openUserInfo && (
            <div className="profile-section-body">
              {/* Semantic definition list: label/value pairs */}
              <dl className="profile-user-info">
                <div className="profile-user-row">
                  <dt>Username</dt>
                  <dd>{displayName}</dd>
                </div>

                <div className="profile-user-row">
                  <dt>Email</dt>
                  <dd>{user.email ?? "Not set"}</dd>
                </div>

                <div className="profile-user-row profile-user-row--password">
                  <dt>Password</dt>
                  <dd>
                    ********
                    <button
                      type="button"
                      className="btn btn-small"
                      onClick={() =>
                        alert("Password change UI not implemented yet.")
                      }
                    >
                      Change
                    </button>
                  </dd>
                </div>
              </dl>
            </div>
          )}
        </section>

        {/* RESULTS SECTION */}
        <section className="profile-section">
          <button
            type="button"
            className="profile-section-header"
            onClick={() => setOpenResults((v) => !v)}
          >
            <h2 className="profile-section-title">My results</h2>
            <span className="profile-section-toggle">
              {openResults ? "▴" : "▾"}
            </span>
          </button>

          {openResults && (
            <div className="profile-section-body">
              <div className="profile-search-row">
                <input
                  type="text"
                  className="profile-search-input"
                  placeholder="Search by title or course code..."
                  value={resultsFilter}
                  onChange={(e) => setResultsFilter(e.target.value)}
                />
              </div>

              {loadingResults && <p>Loading results…</p>}

              {!loadingResults && filteredResults.length === 0 && (
                <p className="profile-empty-text">
                  You have not completed any quizzes yet.
                </p>
              )}

              <ul className="profile-list">
                {filteredResults.map((r) => (
                  <li key={r.resultId} className="profile-list-item">
                    <div className="profile-list-main">
                      <span className="profile-list-title">
                        {r.quizTitle}
                      </span>
                      <span className="profile-list-meta">
                        {r.subjectCode} • {r.correctCount}/{r.totalQuestions}{" "}
                        correct ({Math.round(r.percentage)}%)
                      </span>
                    </div>
                    <div className="profile-list-meta">
                      {new Date(r.completedAt).toLocaleString()}
                    </div>
                  </li>
                ))}
              </ul>
            </div>
          )}
        </section>

        {/* QUIZZES SECTION */}
        <section className="profile-section">
          <button
            type="button"
            className="profile-section-header"
            onClick={() => setOpenQuizzes((v) => !v)}
          >
            <h2 className="profile-section-title">My quizzes</h2>
            <span className="profile-section-toggle">
              {openQuizzes ? "▴" : "▾"}
            </span>
          </button>

          {openQuizzes && (
            <div className="profile-section-body">
              <div className="profile-search-row">
                <input
                  type="text"
                  className="profile-search-input"
                  placeholder="Search by title or course code..."
                  value={quizzesFilter}
                  onChange={(e) => setQuizzesFilter(e.target.value)}
                />
              </div>

              {loadingQuizzes && <p>Loading quizzes…</p>}

              {!loadingQuizzes && filteredQuizzes.length === 0 && (
                <p className="profile-empty-text">
                  You have not created any quizzes yet.
                </p>
              )}

              <ul className="profile-list">
                {filteredQuizzes.map((q) => (
                  <li key={q.id} className="profile-list-item">
                    <div className="profile-list-main">
                      <span className="profile-list-title">{q.title}</span>
                      <span className="profile-list-meta">
                        {q.subjectCode} • {q.questionCount} questions •{" "}
                        {q.isPublished ? "Published" : "Draft"}
                      </span>
                    </div>

                    <div className="profile-quiz-actions">
                      <button
                        type="button"
                        className="btn btn-secondary btn-small"
                        onClick={() => navigate(`/quizzes/${q.id}/edit`)}
                      >
                        Edit
                      </button>
                      <button
                        type="button"
                        className="btn btn-danger btn-small"
                        disabled={deletingId === q.id}
                        onClick={() => void handleDeleteQuiz(q.id)}
                      >
                        {deletingId === q.id ? "Deleting…" : "Delete"}
                      </button>
                    </div>
                  </li>
                ))}
              </ul>
            </div>
          )}
        </section>

        {/* Logout at the very bottom, centered */}
        <div className="profile-footer">
          <button
            type="button"
            className="btn"
            onClick={() => void handleLogoutClick()}
          >
            Log out
          </button>
        </div>
      </div>
    </section>
  );
};

export default ProfilePage;
