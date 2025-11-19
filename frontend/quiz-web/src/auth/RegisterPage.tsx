// auth/RegisterPage.tsx
// ----------------------
// Form for creating a user account. After registration the user is logged in automatically.

import React, { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useAuth } from "./AuthContext";

const RegisterPage: React.FC = () => {
  const { register } = useAuth();
  const navigate = useNavigate();

  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);

    try {
      await register({ username, email, password });
      navigate("/");
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
          Email
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
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
