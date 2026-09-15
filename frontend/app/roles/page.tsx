import RolesClient from "@/components/roles/RolesClient";
import { getRoles } from "@/services/role.service";
import { getUsers } from "@/services/users.service";

export default async function RolesPage() {
  const [roles, usersResponse] = await Promise.all([
    getRoles(),
    getUsers(),
  ]);

  return (
    <RolesClient
        initialRoles={roles}
        users={usersResponse.data.users}
      />
  );
}
