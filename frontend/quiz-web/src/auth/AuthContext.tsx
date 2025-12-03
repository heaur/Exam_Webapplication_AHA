// src/auth/AuthContext.tsx
// ------------------------------------------------------
// React provider that keeps track of the logged-in user.
//
// - On mount: asks backend (/api/User/me) who is logged in
// - On login/register: calls AuthService, stores token in
//   localStorage (for other APIs) and refreshes current user
// - On logout: calls backend and clears user + token
//
// Components use this via the useAuth() hook (UseAuth.ts).
// ------------------------------------------------------

import React, { useEffect, useState, type ReactNode } from "react";
import { AuthContext, type AuthContextValue } from "./AuthContextDef";

import {
  loginUser,
  registerUser,
  logoutUser,
  getCurrentUser,
} from "./AuthService";

import type { LoginDto, RegisterDto } from "./types/auth";
import type { User } from "./types/user";

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // --------------------------------------------------
  // Initial load: try to restore session from backend
  // --------------------------------------------------
  useEffect(() => {
    let isMounted = true;

    (async () => {
      try {
        const current = await getCurrentUser();
        if (isMounted) {
          setUser(current);
        }
      } catch (err) {
        console.error("Failed to restore session:", err);
        if (isMounted) {
          setUser(null);
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    })();

    return () => {
      isMounted = false;
    };
  }, []);

  // --------------------------------------------------
  // LOGIN
  // --------------------------------------------------
  async function login(dto: LoginDto): Promise<void> {
    // 1) Ask backend to log in and return a JWT token
    const token = await loginUser(dto);

    // 2) Store token so other API services (e.g. QuizService)
    //    can still read it from localStorage if needed
    localStorage.setItem("token", token);

    // 3) Ask backend who the current user is
    const current = await getCurrentUser();
    setUser(current);
  }

  // --------------------------------------------------
  // REGISTER
  // --------------------------------------------------
  async function register(dto: RegisterDto): Promise<void> {
    const token = await registerUser(dto);
    localStorage.setItem("token", token);

    const current = await getCurrentUser();
    setUser(current);
  }

  // --------------------------------------------------
  // LOGOUT
  // --------------------------------------------------
  async function logout(): Promise<void> {
    await logoutUser();
    localStorage.removeItem("token");
    setUser(null);
  }

  const value: AuthContextValue = {
    user,
    isLoading,
    login,
    register,
    logout,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};
