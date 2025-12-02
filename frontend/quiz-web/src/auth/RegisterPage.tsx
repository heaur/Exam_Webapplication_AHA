// src/auth/RegisterPage.tsx
// --------------------------
// Form for creating a user account.
// After successful registration the user is automatically logged in
// (handled inside the auth context / AuthService).

import React, { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useAuth } from "./UseAuth";
import type { RegisterDto } from "./types/auth";

const USERNAME_MAX_LENGTH = 8;

const RegisterPage: React.FC = () => {
  const { register } = useAuth();
  const navigate = useNavigate();

  // Local form state
  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);

    // Trim values to avoid leading/trailing spaces being stored in DB
    const trimmedUsername = username.trim();
    const trimmedEmail = email.trim();
    const trimmedPassword = password.trim();

    // Simple client-side guard for username length.
    // Backend also validates this, so this is just to give faster feedback.
    if (trimmedUsername.length === 0) {
      setError("Username is required.");
      return;
    }

    if (trimmedUsername.length > USERNAME_MAX_LENGTH) {
      setError(`Username cannot be longer than ${USERNAME_MAX_LENGTH} characters.`);
      return;
    }

    if (trimmedPassword.length === 0) {
      setError("Password is required.");
      return;
    }

    // Build the DTO exactly as backend RegisterDto expects
    const dto: RegisterDto = {
      username: trimmedUsername,
      email: trimmedEmail,
      password: trimmedPassword,
    };

    try {
      // This will call the backend and (in your AuthContext/AuthService)
      // update the current user state if registration succeeds.
      await register(dto);
      navigate("/"); // Redirect to home after successful registration
    } catch (err: unknown) {
      if (err instanceof Error) {
        setError(err.message || "Registration failed.");
      } else {
        setError("Registration failed.");
      }
    }
  }

  return (
    <section className="page form-page">
      <h1>Register</h1>

      {error && <p className="error-text">{error}</p>}

      <form onSubmit={handleSubmit} className="form">
        {/* USERNAME FIELD */}
        <label>
          Username (max {USERNAME_MAX_LENGTH} characters)
          <input
            type="text"
            value={username}
            maxLength={USERNAME_MAX_LENGTH} // hard limit in the UI
            onChange={(e) => setUsername(e.target.value)}
            required
          />
        </label>

        {/* EMAIL FIELD */}
        <label>
          Email
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </label>

        {/* PASSWORD FIELD */}
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
          Register
        </button>

        <p>
          Already have an account?
          <Link to="/login"> Login here</Link>
        </p>
      </form>
    </section>
  );
};

export default RegisterPage;
