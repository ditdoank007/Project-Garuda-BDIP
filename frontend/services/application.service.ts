import {
  apiDelete,
  apiGet,
  apiPost,
  apiPut,
} from "@/services/api";

export type Application = {
  id: string;
  code: string;
  name: string;
  description: string;
  baseUrl: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
};

export type ApplicationFormData = {
  code: string;
  name: string;
  description: string;
  baseUrl: string;
};

type ApplicationsResponse = {
  success: boolean;
  data: Application[];
};

export async function getApplications(): Promise<ApplicationsResponse> {
  return apiGet("/applications") as Promise<ApplicationsResponse>;
}

export async function createApplication(
  application: ApplicationFormData,
) {
  return apiPost("/applications", application);
}

export async function updateApplication(
  code: string,
  application: Omit<ApplicationFormData, "code">,
) {
  return apiPut(
    `/applications/${encodeURIComponent(code)}`,
    application,
  );
}

export async function deactivateApplication(
  code: string,
) {
  return apiDelete(
    `/applications/${encodeURIComponent(code)}`,
  );
}
