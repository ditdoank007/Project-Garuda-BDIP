export interface Group {
  name: string;
  description: string | null;
  gidNumber: number;
  distinguishedName: string;
  memberCount: number;
  createdAt: string;
}

export interface GroupFormData {
  name: string;
  description: string;
}

export const defaultGroupForm: GroupFormData = {
  name: "",
  description: "",
};

export interface GroupMember {
  username: string;
  fullName: string;
  email: string;
  unit: string;
  distinguishedName: string;
}

export interface GroupMembersResponse {
  groupName: string;
  members: GroupMember[];
  total: number;
}
