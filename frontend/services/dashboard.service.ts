import type { DashboardResponse } from "@/types/dashboard";
import { apiGet } from "@/services/api";
import { cookies } from "next/headers";

type ApiResponse = {
  success: boolean;
  message?: string;
  data?: DashboardResponse;
};

export async function getDashboard(): Promise<DashboardResponse> {
  const cookieStore = await cookies();

  const result = await apiGet<ApiResponse>("/dashboard", {
    headers: {
      Cookie: cookieStore.toString(),
    },
  });

  if (!result.success || !result.data) {
    throw new Error(
      result.message ?? "Failed to load dashboard.",
    );
  }

  return result.data;
}
