export interface MonitoringServer {
  name: string;
  isOnline: boolean;

  cpuPercent: number | null;

  memoryTotalBytes: number;
  memoryAvailableBytes: number;
  memoryPercent: number;

  swapTotalBytes: number;
  swapFreeBytes: number;
  swapPercent: number;

  diskTotalBytes: number;
  diskAvailableBytes: number;
  diskPercent: number;

  networkReceiveBytesPerSecond: number | null;
  networkTransmitBytesPerSecond: number | null;

  uptimeSeconds: number;

  lastUpdated: string;
}

export interface MonitoringServersResponse {
  success: boolean;
  count: number;
  data: MonitoringServer[];
}
