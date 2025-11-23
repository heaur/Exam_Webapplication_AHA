// auth/LoginPage.tsx
// -------------------
// Form for logging in a user. Calls the login() function from the AuthContext.

import React, { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useAuth } from "./UseAuth";

const LoginPage: React.FC = () => {
  const { login } = useAuth();
  const navigate = useNavigate();

  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);

    try {
      await login({ username, password });
      navigate("/");
    } catch (err: unknown) {
      // Narrow "unknown" to an Error if possible, otherwise show a generic message.
      if (err instanceof Error) {
        setError(err.message || "Login failed.");
      } else {
        setError("Login failed.");
      }
    }
  }

  return (
    <section className="page form-page">
      <h1>Login</h1>

      {error && <p className="error-text">{error}</p>}

      <form onSubmit={handleSubmit} className="form">
        <label>
          Username
          <input
            type="text"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
          />
        </label>

        <label>
          Password
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </label>

        <button className="btn btn-primary" type="submit">
          Login
        </button>

        <p>
          Don’t have an account?
          <Link to="/register"> Register here</Link>
        </p>
      </form>
    </section>
  );
};

export default LoginPage;
