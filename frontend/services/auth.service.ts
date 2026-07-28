export type AuthUser = {
  username: string;
  fullName: string;
  email: string;
  role: string;
};

type ApiResponse<T> = {
  success: boolean;
  message?: string;
  data?: T;
};

const API_URL =
  process.env.NEXT_PUBLIC_API_URL ??
  "http://192.168.100.120:8080/api";

export async function getCurrentUser(): Promise<AuthUser | null> {
  try {
    const response = await fetch(`${API_URL}/auth/me`, {
      cache: "no-store",
      credentials: "include",
    });

    if (!response.ok) {
      return null;
    }

    const result = (await response.json()) as ApiResponse<AuthUser>;

    return result.success && result.data ? result.data : null;
  } catch {
    return null;
  }
}
