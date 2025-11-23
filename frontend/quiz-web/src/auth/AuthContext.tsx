// auth/AuthContext.tsx
// -----------------------
// Denne fila eksporterer KUN AuthProvider-komponenten.
// Selve contexten ligger i AuthContextDef.ts
// -> ingen Fast Refresh-warning.

import React, { useEffect, useState } from "react";
import { AuthContext } from "./AuthContextDef";
import { loginUser, registerUser, logoutUser, fetchMe } from "./AuthService";
import type { LoginDto, RegisterDto } from "./types/auth";
import type { User } from "./types/user";

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Sjekk cookie-session når appen starter / refresher
  useEffect(() => {
    (async () => {
      try {
        const me = await fetchMe();
        setUser(me);
      } finally {
        setIsLoading(false);
      }
    })();
  }, []);

  async function login(dto: LoginDto): Promise<void> {
    await loginUser(dto);
    const me = await fetchMe();
    if (!me) throw new Error("Login succeeded, but session cookie missing.");
    setUser(me);
  }

  async function register(dto: RegisterDto): Promise<void> {
    await registerUser(dto);
    await login({ username: dto.username, password: dto.password });
  }

  async function logout(): Promise<void> {
    await logoutUser();
    setUser(null);
  }

  return (
    <AuthContext.Provider value={{ user, isLoading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
};
