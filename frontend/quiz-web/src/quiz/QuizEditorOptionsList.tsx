// src/quiz/QuizEditorOptionsList.tsx
// ---------------------------------------------------------
// Renders the list of answer options for one question.
// Lets the user add/remove options and mark which option is
// correct. Assumes a single-correct MCQ model.
// ---------------------------------------------------------

import React from "react";
import type { Option } from "../types/quiz";
import QuizEditorOptionItem from "./QuizEditorOptionItem";

type Props = {
  options: Option[];
  saving: boolean;
  addOption: () => void;
  updateOption: (optionIndex: number, option: Option) => void;
  deleteOption: (optionIndex: number) => void;
  markOptionAsCorrect: (optionIndex: number) => void;
};

const QuizEditorOptionsList: React.FC<Props> = ({
  options,
  saving,
  addOption,
  updateOption,
  deleteOption,
  markOptionAsCorrect,
}) => {
  return (
    <div className="quiz-options-editor" style={{ marginTop: "1rem" }}>
      <div className="quiz-question-header">
        <span>Answer options</span>
        <button
          type="button"
          className="btn btn-secondary btn-small"
          disabled={saving}
          onClick={addOption}
        >
          + Add option
        </button>
      </div>

      {options.length === 0 && (
        <p className="quiz-question-index">
          This question has no options yet. Add at least two options.
        </p>
      )}

      <ul className="quiz-options-list">
        {options.map((opt, idx) => (
          <li key={opt.id ?? idx} className="quiz-option">
            <QuizEditorOptionItem
              option={opt}
              index={idx}
              saving={saving}
              onChange={(updated) => updateOption(idx, updated)}
              onDelete={() => deleteOption(idx)}
              onMarkCorrect={() => markOptionAsCorrect(idx)}
            />
          </li>
        ))}
      </ul>
    </div>
  );
};

export default QuizEditorOptionsList;
