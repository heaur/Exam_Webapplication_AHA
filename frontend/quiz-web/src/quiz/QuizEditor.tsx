// src/quiz/QuizEditor.tsx
// ---------------------------------------------------------
// Master component used for both creating and editing quizzes.
// Works with your existing Quiz / Question / Option types.
// Responsibilities:
//  - Edit quiz metadata (subjectCode, title, description, imageUrl, isPublished)
//  - Manage full list of questions (add/edit/delete)
//  - Manage options per question (add/edit/delete, mark correct)
//  - Expose the final Quiz object via onSubmit
// ---------------------------------------------------------

import React, { useEffect, useRef, useState } from "react";
import type { Quiz, Question, Option } from "../types/quiz";
import QuizEditorMetadataSection from "./QuizEditorMetadataSection";
import QuizEditorQuestionList from "./QuizEditorQuestionList";
import ErrorAlert from "../components/ErrorAlert";

export type QuizEditorMode = "create" | "edit";

type QuizEditorProps = {
  initialQuiz: Quiz;
  mode: QuizEditorMode;
  saving?: boolean;
  error?: string | null;
  onSubmit: (quiz: Quiz) => void;
  onCancel?: () => void;
};

const QuizEditor: React.FC<QuizEditorProps> = ({
  initialQuiz,
  mode,
  saving = false,
  error,
  onSubmit,
  onCancel,
}) => {
  // Local draft state of the quiz that the user is editing
  const [draft, setDraft] = useState<Quiz>(initialQuiz);

  // Keep local draft in sync when initialQuiz changes (e.g. after async load)
  useEffect(() => {
    setDraft(initialQuiz);
  }, [initialQuiz]);

  // Temporary negative IDs for new questions/options so React keys are stable
  const tempIdRef = useRef(-1);
  function getTempId(): number {
    const id = tempIdRef.current;
    tempIdRef.current -= 1;
    return id;
  }

  // ---------------------------------------------------------
  // Metadata updates
  // ---------------------------------------------------------
  function updateMetadata<K extends keyof Quiz>(field: K, value: Quiz[K]) {
    setDraft((prev) => ({ ...prev, [field]: value }));
  }

  // ---------------------------------------------------------
  // Question helpers
  // ---------------------------------------------------------
  function addQuestion() {
    const newQuestion: Question = {
      id: getTempId(),                     // temporary local id
      quizId: draft.id,                    // may be undefined, that's fine
      text: "",
      imageUrl: "",
      points: 1,                           // default to 1 point
      options: [],
    };

    setDraft((prev) => ({
      ...prev,
      questions: [...prev.questions, newQuestion],
    }));
  }

  function updateQuestion(index: number, updated: Question) {
    setDraft((prev) => {
      const questions = [...prev.questions];
      questions[index] = updated;
      return { ...prev, questions };
    });
  }

  function deleteQuestion(index: number) {
    setDraft((prev) => ({
      ...prev,
      questions: prev.questions.filter((_, i) => i !== index),
    }));
  }

  // ---------------------------------------------------------
  // Option helpers (called via child components)
  // ---------------------------------------------------------
  function addOption(questionIndex: number) {
    setDraft((prev) => {
      const questions = [...prev.questions];
      const q = questions[questionIndex];

      const newOption: Option = {
        id: getTempId(),
        questionId: q.id ?? undefined,
        text: "",
        isCorrect: q.options.length === 0, // first option defaults to correct
      };

      const updatedQuestion: Question = {
        ...q,
        options: [...q.options, newOption],
      };

      questions[questionIndex] = updatedQuestion;
      return { ...prev, questions };
    });
  }

  function updateOption(
    questionIndex: number,
    optionIndex: number,
    updated: Option
  ) {
    setDraft((prev) => {
      const questions = [...prev.questions];
      const q = questions[questionIndex];
      const options = [...q.options];
      options[optionIndex] = updated;
      questions[questionIndex] = { ...q, options };
      return { ...prev, questions };
    });
  }

  function deleteOption(questionIndex: number, optionIndex: number) {
    setDraft((prev) => {
      const questions = [...prev.questions];
      const q = questions[questionIndex];
      const options = q.options.filter((_, i) => i !== optionIndex);
      questions[questionIndex] = { ...q, options };
      return { ...prev, questions };
    });
  }

  function markOptionAsCorrect(questionIndex: number, optionIndex: number) {
    // Single-correct model: exactly one option isCorrect = true
    setDraft((prev) => {
      const questions = [...prev.questions];
      const q = questions[questionIndex];
      const options = q.options.map((opt, idx) => ({
        ...opt,
        isCorrect: idx === optionIndex,
      }));
      questions[questionIndex] = { ...q, options };
      return { ...prev, questions };
    });
  }

  // ---------------------------------------------------------
  // Submit
  // ---------------------------------------------------------
  function handleSubmit(event: React.FormEvent) {
    event.preventDefault();

    // Simple frontend validation – backend should still validate
    if (!draft.subjectCode.trim() || !draft.title.trim()) {
      // You can extend this with local error state if you want
      // For now we still submit and let backend complain if needed
    }

    onSubmit(draft);
  }

  return (
    <form className="form" onSubmit={handleSubmit}>
      {error && <ErrorAlert message={error} />}

      <QuizEditorMetadataSection
        quiz={draft}
        saving={saving}
        updateMetadata={updateMetadata}
      />

      <QuizEditorQuestionList
        questions={draft.questions}
        saving={saving}
        addQuestion={addQuestion}
        updateQuestion={updateQuestion}
        deleteQuestion={deleteQuestion}
        addOption={addOption}
        updateOption={updateOption}
        deleteOption={deleteOption}
        markOptionAsCorrect={markOptionAsCorrect}
      />

      <div className="form-actions">
        <button
          type="submit"
          className="btn btn-primary"
          disabled={saving || draft.questions.length === 0}
        >
          {saving
            ? "Saving..."
            : mode === "create"
            ? "Create quiz"
            : "Save changes"}
        </button>

        <button
          type="button"
          className="btn btn-secondary"
          disabled={saving}
          onClick={onCancel ?? (() => window.history.back())}
        >
          Cancel
        </button>
      </div>
    </form>
  );
};

export default QuizEditor;
