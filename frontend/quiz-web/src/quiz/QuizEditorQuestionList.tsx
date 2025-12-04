// src/quiz/QuizEditorQuestionList.tsx
// ---------------------------------------------------------

import React from "react";
import type { Question, Option } from "../types/quiz";
import QuizEditorQuestionItem from "./QuizEditorQuestionItem";

type Props = {
  questions: Question[];
  saving: boolean;
  addQuestion: () => void;
  updateQuestion: (index: number, question: Question) => void;
  deleteQuestion: (index: number) => void;
  addOption: (questionIndex: number) => void;
  updateOption: (
    questionIndex: number,
    optionIndex: number,
    option: Option
  ) => void;
  deleteOption: (questionIndex: number, optionIndex: number) => void;
  markOptionAsCorrect: (questionIndex: number, optionIndex: number) => void;
};

const QuizEditorQuestionList: React.FC<Props> = ({
  questions,
  saving,
  addQuestion,
  updateQuestion,
  deleteQuestion,
  addOption,
  updateOption,
  deleteOption,
  markOptionAsCorrect,
}) => {
  return (
    <section className="quiz-editor-section quiz-question-list--spaced">
      <div className="page-header">
        <h2 className="page-description quiz-question-list-heading">
          Questions
        </h2>
        <button
          type="button"
          className="btn btn-secondary"
          disabled={saving}
          onClick={addQuestion}
        >
          + Add question
        </button>
      </div>

      {questions.length === 0 && (
        <p className="page-description quiz-question-list-empty">
          No questions yet. Click <strong>“Add question”</strong> to start
          building your quiz.
        </p>
      )}

      <ol className="quiz-editor-question-list quiz-editor-question-list--no-padding">
        {questions.map((q, index) => (
          <li key={q.id ?? index} className="quiz-editor-question-list-item">
            <QuizEditorQuestionItem
              question={q}
              index={index}
              saving={saving}
              onChange={(updated) => updateQuestion(index, updated)}
              onDelete={() => deleteQuestion(index)}
              addOption={() => addOption(index)}
              updateOption={(optIndex, opt) =>
                updateOption(index, optIndex, opt)
              }
              deleteOption={(optIndex) => deleteOption(index, optIndex)}
              markOptionAsCorrect={(optIndex) =>
                markOptionAsCorrect(index, optIndex)
              }
            />
          </li>
        ))}
      </ol>
    </section>
  );
};

export default QuizEditorQuestionList;
