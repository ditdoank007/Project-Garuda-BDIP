export interface Role {
  name: string;
  description: string;
  memberCount: number;
}

export interface RolesApiResponse {
  success: boolean;
  data: Role[];
}

export interface RoleApiResponse {
  success: boolean;
  data: Role;
}

export interface CreateRoleRequest {
  name: string;
  description: string;
}

export interface UpdateRoleRequest {
  name: string;
  description: string;
}

export interface RoleMember {
  username: string;
  fullName: string;
  email: string;
  unit: string;
  distinguishedName: string;
}

export interface RoleMembersResponse {
  roleName: string;
  members: RoleMember[];
  total: number;
}

export interface RoleMembersApiResponse {
  success: boolean;
  data: RoleMembersResponse;
}
