// src/auth/types/user.ts
// ----------------------
// The User object kept in AuthContext.
// Must match backend's CurrentUserDto.

export interface User {
  id: string;
  userName: string;     // <-- THIS is what NavMenu expects
  email?: string | null;
}
