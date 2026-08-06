import type { DashboardResponse } from "@/types/dashboard";
import { apiGet } from "@/services/api";

type ApiResponse = {
  success: boolean;
  message?: string;
  data?: DashboardResponse;
};


export async function getDashboard(): Promise<DashboardResponse> {
  const result = await apiGet<ApiResponse>("/dashboard");

  if (!result.success || !result.data) {
    throw new Error(
      result.message ?? "Failed to load dashboard.",
    );
  }

  return result.data;
}
