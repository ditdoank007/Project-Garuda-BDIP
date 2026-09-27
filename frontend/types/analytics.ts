export interface AnalyticsSummary {
  uniqueUsers: number;
  totalSessions: number;
  totalDurationSeconds: number;
  totalDownloadBytes: number;
  totalUploadBytes: number;
  ovpnSessions: number;
  ovpnDurationSeconds: number;
  hotspotSessions: number;
  hotspotDurationSeconds: number;
}

export interface AnalyticsBreakdown {
  access: string;
  sessions: number;
  durationSeconds: number;
  downloadBytes: number;
  uploadBytes: number;
}

export interface AnalyticsDailyPoint {
  date: string;
  sessions: number;
  durationSeconds: number;
}

export interface AnalyticsUser {
  username: string;
  sessions: number;
  durationSeconds: number;
  downloadBytes: number;
  uploadBytes: number;
  ovpnSessions: number;
  ovpnDurationSeconds: number;
  hotspotSessions: number;
  hotspotDurationSeconds: number;
}

export interface AnalyticsData {
  summary: AnalyticsSummary;
  accessBreakdown: AnalyticsBreakdown[];
  daily: AnalyticsDailyPoint[];
  topUsers: AnalyticsUser[];
  topDownloadUsers: AnalyticsUser[];
  topUploadUsers: AnalyticsUser[];
  topDurationUsers: AnalyticsUser[];
  topOvpnUsers: AnalyticsUser[];
}

export interface AnalyticsApiResponse {
  success: boolean;
  message: string;
  data: AnalyticsData;
}
