// layout/NotFoundPage.tsx
// -------------------------
// This component is shown when the user navigates to a URL
// that does not match any defined route (a "404 Not Found" page).

import React from "react";
import { Link } from "react-router-dom";

const NotFoundPage: React.FC = () => {
  return (
    <section className="page page-not-found">
      <h1 className="page-title">404 - Page Not Found</h1>

      <p className="page-description">
        The page you are looking for does not exist, has been moved,
        or you might have typed the address incorrectly.
      </p>

      {/* Provide a clear way for the user to get back into the app flow. */}
      <div className="not-found-actions">
        <Link to="/" className="btn btn-primary">
          Go back to Home
        </Link>
        <Link to="/quizzes" className="btn btn-secondary">
          View available quizzes
        </Link>
      </div>
    </section>
  );
};

export default NotFoundPage;
