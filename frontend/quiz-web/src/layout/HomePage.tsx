// layout/HomePage.tsx
// ----------------------
// This component is the landing page of the web application.
// It explains what the app does and provides quick navigation buttons.

import React from "react";
import { Link } from "react-router-dom";

const HomePage: React.FC = () => {
  return (
    <section className="page page-home">
      {/* Page title */}
      <h1 className="page-title">Quiz Web Application</h1>

      {/* Short description of the purpose of the app.
          This can be reused / extended in your documentation. */}
      <p className="page-description">
        This web application allows users to create and take quizzes. 
        The project is built as part of the ITPE3200 Web Applications exam.
        It demonstrates a modern frontend using React, TypeScript and a .NET API backend.
      </p>

      {/* Call-to-action area: where the user can go next. */}
      <div className="home-actions">
        {/* Button-like links to important sections. */}
        <Link to="/quizzes" className="btn btn-primary">
          Browse Quizzes
        </Link>

        {/* Later, when authentication is implemented, this link
            will be useful for quiz creators / teachers. */}
        <Link to="/quizzes/create" className="btn btn-secondary">
          Create a New Quiz
        </Link>
      </div>

      {/* Additional information section (optional but nice for UX and documentation). */}
      <section className="home-info">
        <h2>About this project</h2>
        <ul>
          <li>Frontend: React + TypeScript + React Router.</li>
          <li>Backend: ASP.NET Core 8.0 Web API with Entity Framework.</li>
          <li>
            Features (planned): quiz creation, quiz taking, scoring, 
            authentication, and role-based access for quiz management.
          </li>
        </ul>
      </section>
    </section>
  );
};

export default HomePage;
