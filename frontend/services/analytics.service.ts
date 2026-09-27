import { apiGet } from "@/services/api";
import type { AnalyticsApiResponse } from "@/types/analytics";

export interface AnalyticsQuery {
  from: string;
  to: string;
  username?: string;
  access?: "all" | "ovpn" | "hotspot";
}

export function getAnalytics(
  query: AnalyticsQuery,
): Promise<AnalyticsApiResponse> {
  const params = new URLSearchParams({
    from: query.from,
    to: query.to,
    access: query.access ?? "all",
  });

  if (query.username?.trim()) {
    params.set("username", query.username.trim());
  }

  return apiGet<AnalyticsApiResponse>(
    `/analytics?${params.toString()}`,
  );
}
