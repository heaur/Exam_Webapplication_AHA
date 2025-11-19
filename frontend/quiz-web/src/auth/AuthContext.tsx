// auth/AuthContext.tsx
// -----------------------
// Global state management for authentication.
// This context holds the logged-in user, JWT token, and helper functions.
// Other components can access this via useAuth().

import React, {
  createContext,
  useContext,
  useEffect,
  useState,
} from "react";
import { jwtDecode } from "jwt-decode";
import { loginUser, registerUser } from "./AuthService";
import type { LoginDto, RegisterDto } from "./types/auth";
import type { User } from "./types/user";

// Shape of the authentication context (what it provides to components)
interface AuthContextValue {
  user: User | null;
  token: string | null;
  isLoading: boolean;
  login: (dto: LoginDto) => Promise<void>;
  register: (dto: RegisterDto) => Promise<void>;
  logout: () => void;
}

// Create context with default empty values
const AuthContext = createContext<AuthContextValue>({
  user: null,
  token: null,
  isLoading: true,
  login: async () => {},
  register: async () => {},
  logout: () => {},
});

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Load token from localStorage on first page load
  useEffect(() => {
    const savedToken = localStorage.getItem("token");

    if (savedToken) {
      try {
        const decoded = jwtDecode<User>(savedToken);

        // Check if token has expired
        const now = Date.now() / 1000;
        if (decoded.exp < now) {
          localStorage.removeItem("token");
        } else {
          setToken(savedToken);
          setUser(decoded);
        }
      } catch {
        // Token is invalid / cannot be decoded
        localStorage.removeItem("token");
      }
    }

    setIsLoading(false);
  }, []);

  // Login function
  async function login(dto: LoginDto): Promise<void> {
    const newToken = await loginUser(dto);
    localStorage.setItem("token", newToken);

    const decoded = jwtDecode<User>(newToken);
    setToken(newToken);
    setUser(decoded);
  }

  // Register + auto-login
  async function register(dto: RegisterDto): Promise<void> {
    await registerUser(dto);
    await login({ username: dto.username, password: dto.password });
  }

  // Logout function
  function logout(): void {
    localStorage.removeItem("token");
    setToken(null);
    setUser(null);
  }

  return (
    <AuthContext.Provider
      value={{ user, token, isLoading, login, register, logout }}
    >
      {children}
    </AuthContext.Provider>
  );
};

// Hook for easier use of the context.
// eslint-disable-next-line react-refresh/only-export-components
export function useAuth(): AuthContextValue {
  return useContext(AuthContext);
}
