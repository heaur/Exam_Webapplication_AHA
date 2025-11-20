// src/components/Loader.tsx
// -------------------------
// Simple loading indicator used across the app.

import React from "react";

const Loader: React.FC = () => {
  return (
    <div className="loader">
      <div className="loader-spinner" />
      <span className="loader-text">Loading...</span>
    </div>
  );
};

export default Loader;
