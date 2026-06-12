// MainSolutions.React/src/utils/auth.ts
export type Role = "Admin" | "Editor" | "Viewer";

interface DecodedToken {
  role?: string;
  [key: string]: any;
}

export function getUserRole(): Role | null {
  const token = localStorage.getItem("token");
  if (!token) return null;

  try {
    const payload = token.split(".")[1];
    const decoded: DecodedToken = JSON.parse(
      atob(payload.replace(/-/g, "+").replace(/_/g, "/")),
    );

    // ASP.NET serializes ClaimTypes.Role under this long URI key
    const role =
      decoded.role ||
      decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];

    if (role === "Admin" || role === "Editor" || role === "Viewer") {
      return role;
    }
    return null;
  } catch {
    return null;
  }
}

export const canCreate = (role: Role | null) => role === "Admin";
export const canUpdate = (role: Role | null) =>
  role === "Admin" || role === "Editor";
export const canDelete = (role: Role | null) => role === "Admin";
