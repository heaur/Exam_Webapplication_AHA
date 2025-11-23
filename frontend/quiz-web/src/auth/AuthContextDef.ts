// auth/AuthContextDef.ts
// ----------------------
// Denne fila inneholder KUN context-definisjonen.
// Ingen komponenter her -> Fast Refresh blir happy.

import { createContext } from "react";
import type { LoginDto, RegisterDto } from "./types/auth";
import type { User } from "./types/user";

export interface AuthContextValue {
  user: User | null;
  isLoading: boolean;
  login: (dto: LoginDto) => Promise<void>;
  register: (dto: RegisterDto) => Promise<void>;
  logout: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextValue>({
  user: null,
  isLoading: true,
  login: async () => {},
  register: async () => {},
  logout: async () => {},
});
