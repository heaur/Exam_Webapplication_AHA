// auth/useAuth.ts
// -----------------------
// En liten hook for å gjøre det enklere å hente auth-context.
//
// Dette ble flyttet hit for å unngå React Fast Refresh warnings.
// (Fast refresh krever at filer som eksporterer en komponent,
// IKKE også eksporterer hooks.)

// auth/useAuth.ts

import { useContext } from "react";
import { AuthContext } from "./AuthContextDef";

export function useAuth() {
  return useContext(AuthContext);
}
