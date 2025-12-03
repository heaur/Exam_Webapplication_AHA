import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Navigate } from "react-router-dom";

import { useAuth } from "../auth/UseAuth";
import { getQuiz, updateQuiz } from "./QuizService";
import type { Quiz } from "../types/quiz";
import Loader from "../components/Loader";
import ErrorAlert from "../components/ErrorAlert";
import QuizEditor from "./QuizEditor";

const QuizEditPage: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const { id } = useParams();

  const [quiz, setQuiz] = useState<Quiz | null>(null);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [apiError, setApiError] = useState<string | null>(null);

  // ✅ hooks kalles alltid, men vi gjør tidlig return INNI effekten hvis noe mangler
  useEffect(() => {
    async function load() {
      if (!id) {
        setLoadError("Invalid quiz ID.");
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setLoadError(null);

        const loaded = await getQuiz(Number(id));
        setQuiz(loaded);
      } catch (err: unknown) {
        console.error("Failed to load quiz", err);
        if (err instanceof Error) {
          setLoadError(err.message || "Failed to load quiz.");
        } else {
          setLoadError("Failed to load quiz.");
        }
      } finally {
        setLoading(false);
      }
    }

    // hvis ikke innlogget, ikke start load
    if (user) {
      void load();
    } else {
      setLoading(false);
    }
  }, [id, user]);

  // ✅ auth-guard etter at alle hooks er kalt
  if (!user) {
    return <Navigate to="/login" replace />;
  }

  async function handleSubmit(updated: Quiz) {
    if (!id) return;

    try {
      setSaving(true);
      setApiError(null);

      const body: Quiz = { ...updated, id: Number(id) };
      await updateQuiz(Number(id), body);

      navigate("/profile");
    } catch (err: unknown) {
      console.error("Failed to update quiz", err);
      if (err instanceof Error) {
        setApiError(err.message || "Failed to update quiz.");
      } else {
        setApiError("Failed to update quiz.");
      }
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return (
      <section className="page page-quiz-edit">
        <h1 className="page-title">Edit quiz</h1>
        <Loader />
      </section>
    );
  }

  if (loadError || !quiz) {
    return (
      <section className="page page-quiz-edit">
        <h1 className="page-title">Edit quiz</h1>
        <ErrorAlert message={loadError ?? "Quiz not found."} />
      </section>
    );
  }

  return (
    <section className="page page-quiz-edit">
      <h1 className="page-title">Edit quiz</h1>

      <QuizEditor
        initialQuiz={quiz}
        mode="edit"
        saving={saving}
        error={apiError}
        onSubmit={handleSubmit}
        onCancel={() => navigate("/profile")}
      />
    </section>
  );
};

export default QuizEditPage;
