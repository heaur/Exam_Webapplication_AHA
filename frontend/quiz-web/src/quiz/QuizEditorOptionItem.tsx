// src/quiz/QuizEditorOptionItem.tsx
// ---------------------------------------------------------
// Single answer option row:
//  - text input
//  - "Correct" radio toggle
//  - delete button
// ---------------------------------------------------------

import React from "react";
import type { Option } from "../types/quiz";

type Props = {
  option: Option;
  index: number;
  saving: boolean;
  onChange: (option: Option) => void;
  onDelete: () => void;
  onMarkCorrect: () => void;
};

const QuizEditorOptionItem: React.FC<Props> = ({
  option,
  index,
  saving,
  onChange,
  onDelete,
  onMarkCorrect,
}) => {
  function updateField<K extends keyof Option>(
    field: K,
    value: Option[K]
  ) {
    onChange({ ...option, [field]: value });
  }

  return (
    <div className="quiz-option-row">
      <label className="quiz-option-label">
        <input
          type="radio"
          name={`correct-option-${option.questionId ?? "q"}`}
          checked={option.isCorrect}
          disabled={saving}
          onChange={onMarkCorrect}
        />
        <span>Correct</span>
      </label>

      <input
        type="text"
        value={option.text ?? ""}
        disabled={saving}
        onChange={(e) => updateField("text", e.target.value)}
        placeholder={`Option ${index + 1}`}
        className="quiz-option-input"
      />

      <button
        type="button"
        className="btn btn-secondary btn-small"
        disabled={saving}
        onClick={onDelete}
      >
        Delete
      </button>
    </div>
  );
};

export default QuizEditorOptionItem;
