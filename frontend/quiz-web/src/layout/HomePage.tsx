// src/layout/HomePage.tsx
// ------------------------
// Home page with:
// - Hero section (Create new quiz / Browse quizzes)
// - Browse section lower on the page with search and quizzes grouped by subjectCode.
// "Browse quizzes" button scrolls to the browse section,
// NOT a separate page.

import React, { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../auth/UseAuth";
import { getQuizzes } from "../quiz/QuizService";
import type { QuizSummary } from "../types/quiz";
import Loader from "../components/Loader";
import ErrorAlert from "../components/ErrorAlert";

const HomePage: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();

  const [quizzes, setQuizzes] = useState<QuizSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState("");

  // Reference to the "browse" section so the hero button can scroll down
  const browseRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    void loadQuizzes();
  }, []);

  // Load all quizzes from the backend and store them in state
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

  // If the user is not logged in, send them to login first.
  // Otherwise navigate directly to the quiz-creation page.
  function handleCreateQuizHeroClick() {
    if (!user) {
      navigate("/login");
    } else {
      navigate("/quizzes/create");
    }
  }

  // Scroll smoothly to the "Browse quizzes" section on the home page
  function handleBrowseQuizzesClick() {
    if (browseRef.current) {
      browseRef.current.scrollIntoView({ behavior: "smooth", block: "start" });
    }
  }

  // When the user selects a quiz card, send them to the "take quiz" page.
  function handleTakeQuizClick(quizId: number) {
    if (!user) {
      navigate("/login");
    } else {
      navigate(`/quizzes/${quizId}/take`);
    }
  }

  // Filter by subjectCode or title (case-insensitive, safe for missing fields)
  const filtered = quizzes.filter((q) => {
    const term = search.trim().toLowerCase();
    if (!term) return true; // empty search => show everything

    const subject = q.subjectCode?.toLowerCase() ?? "";
    const title = q.title?.toLowerCase() ?? "";

    return subject.includes(term) || title.includes(term);
  });

  // Group the filtered quizzes by subjectCode so each course has its own section
  const groupedBySubject = filtered.reduce<Record<string, QuizSummary[]>>(
    (acc, quiz) => {
      const key = quiz.subjectCode || "Other";
      if (!acc[key]) acc[key] = [];
      acc[key].push(quiz);
      return acc;
    },
    {}
  );

  const subjectCodes = Object.keys(groupedBySubject).sort();

  return (
    <main>
      {/* Hero section */}
      <section className="page page-home-hero">
        <div className="home-hero-content">
          <h1 className="page-title">Welcome to Student Quiz</h1>
          <p className="home-hero-text">
            Create quizzes for your courses, practice for exams, and share with
            your classmates.
          </p>

          <div className="home-hero-actions">
            <button
              type="button"
              className="btn btn-primary"
              onClick={handleCreateQuizHeroClick}
            >
              Create new quiz
            </button>

            <button
              type="button"
              className="btn btn-secondary"
              onClick={handleBrowseQuizzesClick}
            >
              Browse quizzes
            </button>
          </div>
        </div>
      </section>

      {/* Browse section */}
      <section ref={browseRef} className="page page-home-browse">
        <div className="page-header">
          <h2 className="page-title">Browse quizzes by course</h2>
          <button
            type="button"
            className="btn btn-secondary"
            onClick={() => void loadQuizzes()}
          >
            Refresh
          </button>
        </div>

        {/* Search input */}
        <div className="quiz-search">
          <input
            type="text"
            placeholder="Search by course code or title..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>

        {loading && <Loader />}
        {error && <ErrorAlert message={error} />}

        {!loading && !error && subjectCodes.length === 0 && (
          <p>No quizzes found.</p>
        )}

        {/* One section per subjectCode with a grid of clickable cards */}
        {!loading &&
          !error &&
          subjectCodes.map((subjectCode) => {
            const subjectQuizzes = groupedBySubject[subjectCode] ?? [];

            return (
              <div key={subjectCode} className="browse-subject-section">
                {/* Course header, e.g. "ITPE3200 – WebProgramming" later */}
                <h3 className="browse-subject-title">{subjectCode}</h3>

                {/* Responsive grid with quiz cards */}
                <div className="browse-subject-quizzes-grid">
                  {subjectQuizzes.map((quiz) => (
                    <article
                      key={quiz.id}
                      className="quiz-card"
                      onClick={() => handleTakeQuizClick(quiz.id)}
                      tabIndex={0}
                      onKeyDown={(e) => {
                        if (e.key === "Enter" || e.key === " ") {
                          e.preventDefault();
                          handleTakeQuizClick(quiz.id);
                        }
                      }}
                    >
                      {/* Cover image */}
                      {quiz.imageUrl && (
                        <div className="quiz-card-image-wrapper">
                          <img
                            src={quiz.imageUrl}
                            alt={quiz.title || "Quiz cover image"}
                            className="quiz-card-image"
                          />
                        </div>
                      )}

                      <div className="quiz-card-body">
                        <h4 className="quiz-card-title">{quiz.title}</h4>

                        {quiz.description && (
                          <p className="quiz-card-description">
                            {quiz.description}
                          </p>
                        )}
                      </div>
                    </article>
                  ))}
                </div>
              </div>
            );
          })}
      </section>
    </main>
  );
};

export default HomePage;
