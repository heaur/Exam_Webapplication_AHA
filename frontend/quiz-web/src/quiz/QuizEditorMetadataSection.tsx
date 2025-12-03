// src/quiz/QuizEditorMetadataSection.tsx
// ---------------------------------------------------------
// Renders the "quiz settings" section: subject, title,
// description, imageUrl, isPublished.
// Keeps QuizEditor itself smaller and easier to reason about.
// ---------------------------------------------------------

import React from "react";
import type { Quiz } from "../types/quiz";

type Props = {
  quiz: Quiz;
  saving: boolean;
  updateMetadata: <K extends keyof Quiz>(field: K, value: Quiz[K]) => void;
};

const QuizEditorMetadataSection: React.FC<Props> = ({
  quiz,
  saving,
  updateMetadata,
}) => {
  return (
    <section className="quiz-editor-section">
      <h2 className="page-description" style={{ marginBottom: "0.75rem" }}>
        Quiz settings
      </h2>

      {/* Subject / course code */}
      <div className="form-field">
        <label htmlFor="subjectCode">Subject / course code *</label>
        <input
          id="subjectCode"
          type="text"
          placeholder="e.g. ITPE3200"
          value={quiz.subjectCode ?? ""}
          disabled={saving}
          onChange={(e) => updateMetadata("subjectCode", e.target.value)}
        />
      </div>

      {/* Title */}
      <div className="form-field">
        <label htmlFor="title">Title *</label>
        <input
          id="title"
          type="text"
          value={quiz.title ?? ""}
          disabled={saving}
          onChange={(e) => updateMetadata("title", e.target.value)}
        />
      </div>

      {/* Description */}
      <div className="form-field">
        <label htmlFor="description">Description *</label>
        <textarea
          id="description"
          rows={3}
          value={quiz.description ?? ""}
          disabled={saving}
          onChange={(e) => updateMetadata("description", e.target.value)}
        />
      </div>

      {/* Cover image */}
      <div className="form-field">
        <label htmlFor="imageUrl">Cover image URL *</label>
        <input
          id="imageUrl"
          type="text"
          placeholder="https://example.com/cover.jpg"
          value={quiz.imageUrl ?? ""}
          disabled={saving}
          onChange={(e) => updateMetadata("imageUrl", e.target.value)}
        />
      </div>
    </section>
  );
};

export default QuizEditorMetadataSection;
