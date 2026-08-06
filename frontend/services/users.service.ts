import {
  apiGet,
  apiPost,
  apiPut,
  apiDelete,
} from "./api";

import type {
  User,
  UserFormData,
  UserListResponse,
} from "@/types/users";

import type {
  SynologyImportPreview,
  SynologyImportResult,
  SynologyUploadResponse,
} from "@/types/user-import";

export async function getUsers() {
  return apiGet<UserListResponse>("/users");
}

export async function createUser(
  user: UserFormData,
) {
  return apiPost("/users", {
    username: user.username,
    fullName: user.fullName,
    email: user.email,
    unit: user.unit,
    password: user.password,
    enabled: user.enabled,
  });
}

export async function updateUser(
  username: string,
  user: User,
) {
  return apiPut(
    `/users/${encodeURIComponent(username)}`,
    {
      fullName: user.fullName,
      email: user.email,
      unit: user.unit,
      enabled: user.enabled,
    },
  );
}

export async function updateUserStatus(
  username: string,
  enabled: boolean,
) {
  return apiPut(
    `/users/${encodeURIComponent(username)}/status`,
    {
      enabled,
    },
  );
}

export async function deleteUser(
  username: string,
) {
  return apiDelete(
    `/users/${encodeURIComponent(username)}`,
  );
}

/*
 * Import CSV Synology
 * Masih menggunakan endpoint yang sama.
 * Kita migrasikan setelah upload helper
 * sudah dipusatkan di services/api.
 */

export async function uploadSynologyUserCsv(
  file: File,
): Promise<SynologyUploadResponse> {
  throw new Error(
    "uploadSynologyUserCsv belum dimigrasikan ke Unified API Client.",
  );
}

export async function previewSynologyUserCsv(
  uploadId: string,
): Promise<SynologyImportPreview> {
  return apiGet(
    `/users/import/synology/uploads/${encodeURIComponent(
      uploadId,
    )}/preview`,
  );
}

export async function executeUploadedSynologyImport(
  uploadId: string,
  initialPassword: string,
): Promise<SynologyImportResult> {
  return apiPost(
    "/users/import/synology/uploads/execute",
    {
      uploadId,
      initialPassword,
    },
  );
}