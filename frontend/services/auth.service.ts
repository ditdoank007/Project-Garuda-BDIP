import { apiGet, apiPost } from "@/services/api";

export type AuthUser = {
  username: string;
  fullName: string;
  email: string;
  role: string;
};

type LoginRequest = {
  username: string;
  password: string;
};

type ApiResponse<T> = {
  success: boolean;
  message?: string;
  data?: T;
};


export async function login(
  username: string,
  password: string,
): Promise<AuthUser> {
  const result =
    await apiPost<ApiResponse<AuthUser>>(
      "/auth/login",
      {
        username,
        password,
      },
    );

  if (!result.success || !result.data) {
    throw new Error(
      result.message ?? "Login gagal.",
    );
  }

  return result.data;
}

export async function getCurrentUser(
  cookieHeader?: string,
): Promise<AuthUser | null> {
  try {
    const result = await apiGet<ApiResponse<AuthUser>>("/auth/me", {
      headers: cookieHeader
        ? {
            Cookie: cookieHeader,
          }
        : undefined,
    });

    return result.success && result.data
      ? result.data
      : null;
  } catch {
    return null;
  }
}
