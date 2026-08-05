import AppShell from "@/components/layout/AppShell";
import PoliciesClient from "@/components/policies/PoliciesClient";
import { getPolicies } from "@/services/policy.service";
import type { Policy } from "@/types/policy";

export default async function PoliciesPage() {
  const response = await getPolicies();

const policies: Policy[] = response.data.map((p: any) => ({
  id: p.id,
  code: p.code,
  name: p.name,
  description: p.description ?? "",

  enabled: p.enabled,
  priority: p.priority ?? 0,

  sessionTimeout: p.sessionTimeout,
  idleTimeout: p.idleTimeout,
  simultaneousUse: p.simultaneousUse,

  downloadRate: p.downloadRate,
  uploadRate: p.uploadRate,

  burstDownload: p.burstDownload,
  burstUpload: p.burstUpload,

  dailyQuota: p.dailyQuota,
  monthlyQuota: p.monthlyQuota,
  totalQuota: p.totalQuota,

  addressList: p.addressList,
  vlanId: p.vlanId,
  ipPool: p.ipPool,

  expirationDate: p.expirationDate,
  loginSchedule: p.loginSchedule,

  createdAt: p.createdAt,
  updatedAt: p.updatedAt,
}));

  return (
    <AppShell>
      <PoliciesClient policies={policies} />
    </AppShell>
  );
}