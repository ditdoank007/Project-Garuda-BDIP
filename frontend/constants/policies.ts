import type { PolicyFormData } from "@/types/policy";

export const defaultPolicyForm: PolicyFormData = {
  code: "",
  name: "",
  description: "",

  priority: 100,
  enabled: true,

  sessionTimeout: 0,
  idleTimeout: 0,
  simultaneousUse: 1,

  downloadRate: 0,
  uploadRate: 0,

  burstDownload: 0,
  burstUpload: 0,

  dailyQuota: 0,
  monthlyQuota: 0,
  totalQuota: 0,

  addressList: "",
  vlanId: 0,
  ipPool: "",

  expirationDate: null,
  loginSchedule: "",
};