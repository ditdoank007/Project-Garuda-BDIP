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
    enabled: p.isActive,
    priority: p.priority ?? 0,
    createdAt: p.createdAt,
    updatedAt: p.updatedAt,
  }));

  return (
    <AppShell>
      <PoliciesClient policies={policies} />
    </AppShell>
  );
}