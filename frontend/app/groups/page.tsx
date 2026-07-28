import AppShell from "@/components/layout/AppShell";
import GroupsClient from "@/components/groups/GroupsClient";

import { getGroups } from "@/lib/api/groups";
import { getUsers } from "@/services/users.service";

export const dynamic = "force-dynamic";

export default async function GroupsPage() {
  const [groups, usersResponse] = await Promise.all([
    getGroups(),
    getUsers(),
  ]);

  return (
    <AppShell>
      <GroupsClient
        initialGroups={groups}
        users={usersResponse.data.users}
      />
    </AppShell>
  );
}
