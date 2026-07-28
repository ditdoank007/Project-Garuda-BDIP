import { api } from "./api";
import type { RadiusSession, SessionsApiResponse } from "@/types/session";

type BackendSession = {
  radAcctId: number;
  acctSessionId: string;
  username: string | null;
  nasIpAddress: string | null;
  nasPortId: string | null;
  nasPortType: string | null;
  acctStartTime: string | null;
  acctUpdateTime: string | null;
  acctStopTime?: string | null;
  acctSessionTime: number | null;
  acctInputOctets: number | null;
  acctOutputOctets: number | null;
  calledStationId: string | null;
  callingStationId: string | null;
  acctTerminateCause?: string | null;
  serviceType: string | null;
  framedProtocol: string | null;
  framedIpAddress: string | null;
};

type BackendSessionsResponse = {
  success: boolean;
  message: string;
  data: {
    total: number;
    sessions: BackendSession[];
  };
};

export async function getSessions(): Promise<SessionsApiResponse> {
  const response = await api<BackendSessionsResponse>("/sessions");

  const sessions: RadiusSession[] = response.data.sessions.map(
    (session) => ({
      id: session.radAcctId,
      username: session.username ?? "-",
      nasIpAddress: session.nasIpAddress,
      nasIdentifier: session.nasPortId,
      framedIpAddress: session.framedIpAddress,
      callingStationId: session.callingStationId,
      calledStationId: session.calledStationId,
      serviceType: session.serviceType,
      framedProtocol: session.framedProtocol,
      startTime: session.acctStartTime ?? "",
      updateTime: session.acctUpdateTime,
      stopTime: session.acctStopTime ?? null,
      sessionTimeSeconds: session.acctSessionTime ?? 0,
      inputBytes: session.acctInputOctets ?? 0,
      outputBytes: session.acctOutputOctets ?? 0,
      terminateCause: session.acctTerminateCause ?? null,
      active: !session.acctStopTime,
    })
  );

  return {
    success: response.success,
    message: response.message,
    data: {
      summary: {
        totalSessions: response.data.total,
        activeSessions: sessions.filter((session) => session.active).length,
        historicalSessions: sessions.filter((session) => !session.active).length,
        uniqueUsers: new Set(
          sessions.map((session) => session.username.toLowerCase())
        ).size,
      },
      sessions,
    },
  };
}
