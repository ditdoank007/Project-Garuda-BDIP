import AppShell from "@/components/layout/AppShell";
import UsersClient from "@/components/users/UsersClient";
import { getUsers } from "@/services/users.service";

export default async function UsersPage() {
  const response = await getUsers();

  return (
    <AppShell>
      <UsersClient users={response.data.users} />
    </AppShell>
  );
}