import type { MonitoringServersResponse } from "@/types/monitoring";
import { apiGet } from "@/services/api";

export async function getMonitoringServers(): Promise<MonitoringServersResponse> {
  return apiGet<MonitoringServersResponse>("/monitoring/servers");
}
