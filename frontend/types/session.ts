export interface RadiusSession {
  id: number;
  username: string;
  nasIpAddress: string | null;
  nasIdentifier: string | null;
  framedIpAddress: string | null;
  routerOsId: string;
  routerAddress: string | null;
  routerServer: string | null;
  macAddress: string | null;
  isRouterActive: boolean;
  callingStationId: string | null;
  calledStationId: string | null;
  serviceType: string | null;
  framedProtocol: string | null;
  startTime: string;
  updateTime: string | null;
  stopTime: string | null;
  sessionTimeSeconds: number;
  inputBytes: number;
  outputBytes: number;
  terminateCause: string | null;
  active: boolean;
  policyCode: string | null;
  policyName: string | null;
  downloadRate: number | null;
  uploadRate: number | null;
  sessionTimeout: number | null;
  idleTimeout: number | null;
  simultaneousUse: number | null;
}

export interface SessionSummary {
  totalSessions: number;
  activeSessions: number;
  historicalSessions: number;
  uniqueUsers: number;
}

export interface SessionsData {
  summary: SessionSummary;
  sessions: RadiusSession[];
}

export interface SessionsApiResponse {
  success: boolean;
  message: string;
  data: SessionsData;
}
