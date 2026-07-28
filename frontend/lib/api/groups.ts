import axios from "axios";

import type {
  Group,
  GroupFormData,
  GroupMembersResponse,
} from "@/types/groups";

const API_URL = process.env.NEXT_PUBLIC_API_URL;

export async function getGroups(): Promise<Group[]> {
  const response = await axios.get<Group[]>(
    `${API_URL}/groups`,
  );

  return response.data;
}

export async function getGroup(
  groupName: string,
): Promise<Group> {
  const response = await axios.get<Group>(
    `${API_URL}/groups/${encodeURIComponent(groupName)}`,
  );

  return response.data;
}

export async function createGroup(
  group: GroupFormData,
): Promise<Group> {
  const response = await axios.post<Group>(
    `${API_URL}/groups`,
    {
      name: group.name,
      description: group.description,
    },
  );

  return response.data;
}

export async function updateGroup(
  groupName: string,
  group: GroupFormData,
): Promise<Group> {
  const response = await axios.put<Group>(
    `${API_URL}/groups/${encodeURIComponent(groupName)}`,
    {
      name: group.name,
      description: group.description,
    },
  );

  return response.data;
}

export async function deleteGroup(
  groupName: string,
) {
  const response = await axios.delete(
    `${API_URL}/groups/${encodeURIComponent(groupName)}`,
  );

  return response.data;
}

export async function getGroupMembers(
  groupName: string,
): Promise<GroupMembersResponse> {
  const response = await axios.get<GroupMembersResponse>(
    `${API_URL}/groups/${encodeURIComponent(groupName)}/members`,
  );

  return response.data;
}

export async function addGroupMember(
  groupName: string,
  username: string,
) {
  const response = await axios.post(
    `${API_URL}/groups/${encodeURIComponent(groupName)}/members`,
    { username },
  );

  return response.data;
}

export async function removeGroupMember(
  groupName: string,
  username: string,
) {
  const response = await axios.delete(
    `${API_URL}/groups/${encodeURIComponent(groupName)}/members/${encodeURIComponent(username)}`,
  );

  return response.data;
}
