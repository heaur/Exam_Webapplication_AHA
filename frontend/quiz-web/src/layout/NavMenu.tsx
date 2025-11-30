// src/layout/NavMenu.tsx
// ----------------------
// Top navigation bar for the app.
//
// - Logo "Student Quiz" on the left (click -> home)
// - Links on the right: "Home", "New quiz", "Login"/"Logout"
// - Hides completely on quiz taking and result pages

import React from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
// øverst
import { useAuth } from "../auth/UseAuth";

const NavMenu: React.FC = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const { user, logout } = useAuth();

  const path = location.pathname;

  // Hide navbar on quiz-taking and result pages
  const hideNav =
    path.startsWith("/quizzes/") &&
    (path.endsWith("/take") || path.endsWith("/result"));

  if (hideNav) {
    return null;
  }

  function isActive(to: string): boolean {
    return path === to;
  }

  function handleNewQuizClick() {
    if (!user) {
      navigate("/login");
    } else {
      navigate("/quizzes/create");
    }
  }

  function handleLogoutClick() {
    logout();
    navigate("/");
  }

  return (
    <header className="nav-header">
      <div className="nav-container">
        {/* Logo / brand on the left */}
        <Link to="/" className="nav-brand">
          Student Quiz
        </Link>

        {/* Links on the right */}
        <nav className="nav-right">
          <Link
            to="/"
            className={`nav-link ${isActive("/") ? "nav-link-active" : ""}`}
          >
            Home
          </Link>

          <button
            type="button"
            className="nav-link"
            onClick={handleNewQuizClick}
          >
            New quiz
          </button>

          {!user && (
            <Link
              to="/login"
              className={`nav-link ${
                isActive("/login") || isActive("/register")
                  ? "nav-link-active"
                  : ""
              }`}
            >
              Login
            </Link>
          )}

          {user && (
            <button
              type="button"
              className="nav-link"
              onClick={handleLogoutClick}
            >
              Logout
            </button>
          )}
        </nav>
      </div>
    </header>
  );
};

export default NavMenu;
