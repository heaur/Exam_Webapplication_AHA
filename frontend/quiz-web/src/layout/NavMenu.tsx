// src/layout/NavMenu.tsx
// ----------------------------------------------------
// Global navigation bar for the app.
// Hidden on quiz "take" and "result" pages.
// Shows username (max 8 chars) when logged in.
// ----------------------------------------------------

import React from "react";
import { Link, NavLink, useLocation } from "react-router-dom";
import { useAuth } from "../auth/UseAuth";

// SUPER SIMPLE: hide nav on any route that is a quiz-take or quiz-result.
// We assume only quiz-pages use "/take" or "/result" in the path.
function shouldHideNav(pathname: string): boolean {
  if (pathname.includes("/take")) return true;
  if (pathname.includes("/result")) return true;
  return false;
}

const NavMenu: React.FC = () => {
  const { user, isLoading } = useAuth();
  const location = useLocation();

  // Hide navbar completely on quiz take/result
  if (shouldHideNav(location.pathname)) {
    return null;
  }

  // Display name in navbar:
  // 1) Prefer user.userName from JWT (max 8 chars)
  // 2) Fallback: first part of email (before "@"), max 8 chars
  // 3) Fallback: "User"
  const displayName =
    user?.userName?.slice(0, 8) ||
    (user?.email ? user.email.split("@")[0].slice(0, 8) : "User");

  return (
    <header className="nav-header">
      <div className="nav-container">
        {/* Left: brand/logo */}
        <Link to="/" className="nav-brand">
          Student Quiz
        </Link>

        {/* Right: navigation links */}
        <nav className="nav-right">
          {/* Home */}
          <NavLink
            to="/"
            className={({ isActive }) =>
              "nav-link" + (isActive ? " nav-link-active" : "")
            }
          >
            Home
          </NavLink>

          {/* New quiz */}
          <NavLink
            to="/quizzes/create"
            className={({ isActive }) =>
              "nav-link" + (isActive ? " nav-link-active" : "")
            }
          >
            New quiz
          </NavLink>

          {/* Logged-out links */}
          {!isLoading && !user && (
            <>
              <NavLink
                to="/login"
                className={({ isActive }) =>
                  "nav-link" + (isActive ? " nav-link-active" : "")
                }
              >
                Login
              </NavLink>

              <NavLink
                to="/register"
                className={({ isActive }) =>
                  "nav-link" + (isActive ? " nav-link-active" : "")
                }
              >
                Register
              </NavLink>
            </>
          )}

          {/* Logged-in: show username as link to profile */}
          {!isLoading && user && (
            <NavLink
              to="/profile"
              className={({ isActive }) =>
                "nav-link" + (isActive ? " nav-link-active" : "")
              }
            >
              {displayName}
            </NavLink>
          )}
        </nav>
      </div>
    </header>
  );
};

export default NavMenu;
