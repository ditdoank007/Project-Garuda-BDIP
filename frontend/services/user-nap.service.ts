import { api } from "./api";

export interface UserNap {
  uid: string;
  policyId?: string | null;
  policyCode?: string | null;

  downloadKbps: number;
  uploadKbps: number;

  sessionTimeout: number;
  idleTimeout: number;

  isActive: boolean;

  createdAt: string;
  updatedAt: string;
}

interface UserNapListResponse {
  success: boolean;
  data: UserNap[];
}

export async function getUserNap() {
  return api<UserNapListResponse>("/nap/users");
}