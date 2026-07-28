import axios from "axios";

import type {
  SynologyImportPreview,
  SynologyImportResult,
  SynologyUploadResponse,
} from "@/types/user-import";

const API_URL =
  process.env.NEXT_PUBLIC_API_URL ??
  "http://192.168.100.120:8080/api";

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

export async function uploadSynologyCsv(
  file: File,
): Promise<SynologyUploadResponse> {
  const formData = new FormData();

  formData.append("file", file);

  const response = await axios.post<
    ApiResponse<SynologyUploadResponse>
  >(
    `${API_URL}/users/import/synology/upload`,
    formData,
  );

  return response.data.data;
}

export async function previewSynologyCsv(
  uploadId: string,
): Promise<SynologyImportPreview> {
  const response = await axios.get<
    ApiResponse<SynologyImportPreview>
  >(
    `${API_URL}/users/import/synology/uploads/${encodeURIComponent(
      uploadId,
    )}/preview`,
  );

  return response.data.data;
}

export async function executeSynologyCsvImport(
  uploadId: string,
  initialPassword: string,
): Promise<SynologyImportResult> {
  const response = await axios.post<
    ApiResponse<SynologyImportResult>
  >(
    `${API_URL}/users/import/synology/uploads/execute-upload`,
    {
      uploadId,
      initialPassword,
    },
  );

  return response.data.data;
}
