export interface DashboardStats {
  users: number;
  groups: number;
  units: number;
  applications: number;

  ldap: "healthy" | "warning" | "offline";
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