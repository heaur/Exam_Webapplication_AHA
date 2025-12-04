// src/quiz/QuizEditorQuestionItem.tsx
// ---------------------------------------------------------
// Editor for a single question: text, image URL, points,
// and its list of options.
// ---------------------------------------------------------

import React from "react";
import type { Question, Option } from "../types/quiz";
import QuizEditorOptionsList from "./QuizEditorOptionsList";

type Props = {
  question: Question;
  index: number;
  saving: boolean;
  onChange: (question: Question) => void;
  onDelete: () => void;
  addOption: () => void;
  updateOption: (optionIndex: number, option: Option) => void;
  deleteOption: (optionIndex: number) => void;
  markOptionAsCorrect: (optionIndex: number) => void;
};

const QuizEditorQuestionItem: React.FC<Props> = ({
  question,
  index,
  saving,
  onChange,
  onDelete,
  addOption,
  updateOption,
  deleteOption,
  markOptionAsCorrect,
}) => {
  function updateField<K extends keyof Question>(
    field: K,
    value: Question[K]
  ) {
    onChange({ ...question, [field]: value });
  }

  return (
    <div className="quiz-question-card quiz-question-card--spaced">
      <div className="quiz-question-header">
        <p className="quiz-question-index">Question {index + 1}</p>
        <button
          type="button"
          className="btn btn-secondary btn-small"
          disabled={saving}
          onClick={onDelete}
        >
          Delete question
        </button>
      </div>

      {/* Question text */}
      <div className="form-field">
        <label htmlFor={`question-text-${index}`}>Question text *</label>
        <textarea
          id={`question-text-${index}`}
          rows={2}
          value={question.text ?? ""}
          disabled={saving}
          onChange={(e) => updateField("text", e.target.value)}
        />
      </div>

      {/* Image URL */}
      <div className="form-field">
        <label htmlFor={`question-image-${index}`}>
          Image URL (optional, used for illustration in the quiz)
        </label>
        <input
          id={`question-image-${index}`}
          type="text"
          value={question.imageUrl ?? ""}
          disabled={saving}
          onChange={(e) => updateField("imageUrl", e.target.value)}
        />
      </div>

      {/* Points */}
      <div className="form-field">
        <label htmlFor={`question-points-${index}`}>Points *</label>
        <input
          id={`question-points-${index}`}
          type="number"
          min={0}
          value={question.points ?? 1}
          disabled={saving}
          onChange={(e) =>
            updateField("points", Number(e.target.value) || 0)
          }
        />
      </div>

      <QuizEditorOptionsList
        options={question.options}
        saving={saving}
        addOption={addOption}
        updateOption={updateOption}
        deleteOption={deleteOption}
        markOptionAsCorrect={markOptionAsCorrect}
      />
    </div>
  );
};

export default QuizEditorQuestionItem;
