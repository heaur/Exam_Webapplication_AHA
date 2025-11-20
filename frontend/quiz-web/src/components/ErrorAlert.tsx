// src/components/ErrorAlert.tsx
// -----------------------------
// Reusable error message component.

import React from "react";

interface ErrorAlertProps {
  message: string;
}

const ErrorAlert: React.FC<ErrorAlertProps> = ({ message }) => {
  if (!message) return null;

  return (
    <div className="error-alert">
      <strong>Error:</strong> <span>{message}</span>
    </div>
  );
};

export default ErrorAlert;
