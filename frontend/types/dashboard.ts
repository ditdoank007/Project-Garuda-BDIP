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

export interface SynologyDisk {
  id: string;
  name: string;
  model: string;
  serial: string;
  capacityBytes: number;
  status: string;
  health: string;
  temperature: number | null;
}

export interface SynologySsdCache {
  enabled: boolean;
  status: string;
  raidType: string;
  diskCount: number;
  hitRate: number | null;
}

export interface SynologyHardware {
  bayCount: number;
  diskCount: number;
  healthyDisks: number;
  warningDisks: number;
  failedDisks: number;
  disks: SynologyDisk[];
  poolStatus: string;
  poolRaidType: string;
  ssdCache: SynologySsdCache;
}

export interface SynologyNetworkInterface {
  id: string;
  ip: string;
  type: string;
}

export interface SynologySystemHealth {
  hostname: string;
  uptime: string;
  interfaces: SynologyNetworkInterface[];
  healthy: boolean;
}

export interface SynologyPerformance {
  readBytesPerSecond: number | null;
  writeBytesPerSecond: number | null;
  readIops: number | null;
  writeIops: number | null;
}

export interface SynologySystemResources {
  cpuPercent: number | null;
  memoryPercent: number | null;
  temperatureC: number | null;
  fanStatus: string | null;
}

export interface SynologyStorageHealth {
  raidStatus: string | null;
  filesystemStatus: string | null;
  diskHealth: string | null;
  badSectors: number | null;
}

export interface SynologyMonitoring {
  online: boolean;
  model: string;
  dsmVersion: string;
  volumeName: string;
  volumePath: string;
  fileSystem: string;
  raidType: string;
  status: string;
  totalBytes: number;
  usedBytes: number;
  freeBytes: number;
  usedPercent: number;
  performance?: SynologyPerformance;
  systemResources?: SynologySystemResources;
  storageHealth?: SynologyStorageHealth;
  connections: SynologyConnectionActivity[];
  hardware: SynologyHardware;
  systemHealth: SynologySystemHealth;
}

export interface SynologyConnectionActivity {
  user: string;
  sourceIp: string;
  protocol: string;
  type: string;
  application: string;
  time: string;
  firstLoginTime: string;
  currentConnected: boolean;
  location: string;
  userAgent: string;
  pid: number;
  deviceId: string;
  canBeKicked: boolean;
  isAmfa: boolean;
  isOtpTrusted: boolean;
}

export interface DashboardResponse {
  stats: DashboardStats;
  synology: SynologyMonitoring;
  activities: DashboardActivity[];
}