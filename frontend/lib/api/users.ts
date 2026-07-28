import axios from "axios";

import type {
  User,
  UserFormData,
} from "@/types/users";

const API_URL = process.env.NEXT_PUBLIC_API_URL;

export async function createUser(
  user: UserFormData,
) {
  const response = await axios.post(
    `${API_URL}/users`,
    {
      username: user.username,
      fullName: user.fullName,
      email: user.email,
      unit: user.unit,
      password: user.password,
      enabled: user.enabled,
    },
  );

  return response.data;
}

export async function updateUser(
  username: string,
  user: User,
) {
  const response = await axios.put(
    `${API_URL}/users/${encodeURIComponent(username)}`,
    {
      fullName: user.fullName,
      email: user.email,
      unit: user.unit,
      enabled: user.enabled,
    },
  );

  return response.data;
}

export async function updateUserStatus(
  username: string,
  enabled: boolean,
) {
  const response = await axios.put(
    `${API_URL}/users/${encodeURIComponent(username)}/status`,
    {
      enabled,
    },
  );

  return response.data;
}

export async function deleteUser(
  username: string,
) {
  const response = await axios.delete(
    `${API_URL}/users/${encodeURIComponent(username)}`,
  );

  return response.data;
}

import type {
  SynologyImportPreview,
  SynologyImportResult,
  SynologyUploadResponse,
} from "@/types/user-import";

export async function uploadSynologyUserCsv(
  file: File,
): Promise<SynologyUploadResponse> {
  const formData = new FormData();

  formData.append("file", file);

  const response = await axios.post(
    `${API_URL}/users/import/synology/upload`,
    formData,
  );

  return response.data.data;
}

export async function previewSynologyUserCsv(
  uploadId: string,
): Promise<SynologyImportPreview> {
  const response = await axios.get(
    `${API_URL}/users/import/synology/uploads/${encodeURIComponent(uploadId)}/preview`,
  );

  return response.data.data;
}

export async function executeUploadedSynologyImport(
  uploadId: string,
  initialPassword: string,
): Promise<SynologyImportResult> {
  const response = await axios.post(
    `${API_URL}/users/import/synology/uploads/execute`,
    {
      uploadId,
      initialPassword,
    },
  );

  return response.data.data;
}
