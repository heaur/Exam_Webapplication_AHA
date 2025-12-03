// auth/AuthSection.tsx
// ----------------------
// This component is used inside the navigation bar.
// It displays different options depending on whether the user is logged in.

import React from "react";
import { Link } from "react-router-dom";
import { useAuth } from "./UseAuth";

const AuthSection: React.FC = () => {
  const { user, logout } = useAuth();

  if (!user) {
    // User NOT logged in
    return (
      <div className="nav-auth">
        <Link to="/login" className="nav-link">Login</Link>
        <Link to="/register" className="nav-link">Register</Link>
      </div>
    );
  }

  // User IS logged in
  return (
    <div className="nav-auth nav-auth-logged">
      <span className="nav-user">Welcome, {user.sub}</span>
      <button onClick={logout} className="btn btn-secondary">Logout</button>
    </div>
  );
};

export default AuthSection;
