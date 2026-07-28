import type { DashboardResponse } from "@/types/dashboard";

const API_URL =
  process.env.NEXT_PUBLIC_API_URL ??
  "http://192.168.100.120:8080/api";

type ApiResponse = {
  success: boolean;
  message?: string;
  data?: DashboardResponse;
};

export async function getDashboard(): Promise<DashboardResponse> {
  const response = await fetch(`${API_URL}/dashboard`, {
    cache: "no-store",
  });

  const responseText = await response.text();

  let result: ApiResponse = {
    success: false,
  };

  if (responseText.trim()) {
    try {
      result = JSON.parse(responseText) as ApiResponse;
    } catch {
      throw new Error(
        `Dashboard API returned invalid JSON (HTTP ${response.status}).`,
      );
    }
  }

  if (!response.ok || !result.success || !result.data) {
    throw new Error(
      result.message ??
        `Failed to load dashboard (HTTP ${response.status}).`,
    );
  }

  return result.data;
}
