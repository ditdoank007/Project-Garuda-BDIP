export interface Policy {
  id: string;
  code: string;
  name: string;
  description?: string;

  enabled: boolean;
  priority: number;

  sessionTimeout: number;
  idleTimeout: number;
  simultaneousUse: number;

  downloadRate: number;
  uploadRate: number;

  burstDownload?: number;
  burstUpload?: number;

  dailyQuota?: number;
  monthlyQuota?: number;
  totalQuota?: number;

  addressList?: string;
  vlanId?: number;
  ipPool?: string;

  expirationDate?: string | null;
  loginSchedule?: string;

  createdAt: string;
  updatedAt: string;
}

export interface PolicyListResponse {
  success: boolean;
  data: Policy[];
}

export interface PolicyFormData {
  code: string;
  name: string;
  description: string;

  enabled: boolean;
  priority: number;

  sessionTimeout: number;
  idleTimeout: number;
  simultaneousUse: number;

  downloadRate: number;
  uploadRate: number;

  burstDownload?: number;
  burstUpload?: number;

  dailyQuota?: number;
  monthlyQuota?: number;
  totalQuota?: number;

  addressList?: string;
  vlanId?: number;
  ipPool?: string;

  expirationDate?: string | null;
  loginSchedule?: string;
}