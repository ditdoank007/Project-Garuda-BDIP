export interface DashboardStats {
  totalUsers: number;
  activeSessions: number;
  hotspotSessions: number;
  vpnSessions: number;
  totalPolicies: number;
  nasOnline: number;

  groups: number;
  units: number;
  applications: number;

  ldap: string;
}

export interface DashboardActivity {
  id: number;
  title: string;
  description: string;
  time: string;
}

export interface DashboardResponse {
  stats: DashboardStats;
  activities: DashboardActivity[];
}