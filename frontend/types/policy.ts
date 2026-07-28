export interface Policy {
  id: string;
  code: string;
  name: string;
  description?: string;
  enabled: boolean;
  priority: number;
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
  priority: number;
  enabled: boolean;
}