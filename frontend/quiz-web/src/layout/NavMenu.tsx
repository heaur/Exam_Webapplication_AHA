// layout/NavMenu.tsx
// ----------------------
// This component renders the top navigation bar of the application.
// It is displayed on all pages (because App.tsx renders it outside <Routes>).

import React from "react";
import { Link, NavLink } from "react-router-dom";
import AuthSection from "../auth/AuthSection";

const NavMenu: React.FC = () => {
  return (
    // <header> + <nav> are semantic HTML elements for navigation.
    <header className="nav-header">
      <nav className="nav-container">
        {/* Left side: application "logo" or title.
            Clicking this navigates to the home page ("/"). */}
        <div className="nav-left">
          <Link to="/" className="nav-brand">
            Quiz Web
          </Link>
        </div>

        {/* Right side: main navigation links + authentication section. */}
        <div className="nav-right">
          {/* 
            NavLink is like Link but can automatically add an "active" CSS class
            when the current URL matches the link's "to" property.
          */}
          <NavLink
            to="/"
            end
            className={({ isActive }) =>
              "nav-link" + (isActive ? " nav-link-active" : "")
            }
          >
            Home
          </NavLink>

          {/* Placeholder for the quiz list route.
              For now it will lead to 404 until we add QuizListPage. */}
          <NavLink
            to="/quizzes"
            className={({ isActive }) =>
              "nav-link" + (isActive ? " nav-link-active" : "")
            }
          >
            Quizzes
          </NavLink>

          {/* Authentication UI (Login/Register or Welcome + Logout). */}
          <AuthSection />
        </div>
      </nav>
    </header>
  );
};

export default NavMenu;
