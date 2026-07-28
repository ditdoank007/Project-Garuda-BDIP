import { api } from "./api";

import type {
  CreateRoleRequest,
  Role,
  RoleApiResponse,
  RoleMembersApiResponse,
  RoleMembersResponse,
  RolesApiResponse,
  UpdateRoleRequest,
} from "@/types/role";

export async function getRoles(): Promise<Role[]> {
  const response = await api<RolesApiResponse>("/roles");

  return response.data;
}

export async function createRole(
  request: CreateRoleRequest,
): Promise<Role> {
  const response = await api<RoleApiResponse>(
    "/roles",
    {
      method: "POST",
      body: JSON.stringify(request),
    },
  );

  return response.data;
}

export async function updateRole(
  currentName: string,
  request: UpdateRoleRequest,
): Promise<Role> {
  const response = await api<RoleApiResponse>(
    `/roles/${encodeURIComponent(currentName)}`,
    {
      method: "PUT",
      body: JSON.stringify(request),
    },
  );

  return response.data;
}

export async function deleteRole(
  name: string,
): Promise<void> {
  await api(
    `/roles/${encodeURIComponent(name)}`,
    {
      method: "DELETE",
    },
  );
}

export async function getRoleMembers(
  name: string,
): Promise<RoleMembersResponse> {
  const response = await api<RoleMembersApiResponse>(
    `/roles/${encodeURIComponent(name)}/members`,
  );

  return response.data;
}

export async function addRoleMember(
  name: string,
  username: string,
): Promise<void> {
  await api(
    `/roles/${encodeURIComponent(name)}/members`,
    {
      method: "POST",
      body: JSON.stringify({ username }),
    },
  );
}

export async function removeRoleMember(
  name: string,
  username: string,
): Promise<void> {
  await api(
    `/roles/${encodeURIComponent(name)}/members/${encodeURIComponent(username)}`,
    {
      method: "DELETE",
    },
  );
}
