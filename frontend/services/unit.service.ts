import {
  apiGet,
  apiPost,
  apiPut,
  apiDelete,
} from "@/services/api";

import type {
  CreateUnitRequest,
  Unit,
  UnitApiResponse,
  UpdateUnitRequest,
} from "@/types/unit";

async function parseResponse<T>(
  response: Response
): Promise<UnitApiResponse<T>> {
  const responseText = await response.text();

  let result: UnitApiResponse<T>;

  try {
    result = JSON.parse(responseText);
  } catch {
    throw new Error(
      responseText ||
        `Backend returned HTTP ${response.status}.`
    );
  }

  if (!response.ok || !result.success) {
    throw new Error(
      result.message ||
        `Request failed with HTTP ${response.status}.`
    );
  }

  return result;
}

export async function getUnits(): Promise<Unit[]> {
  const result =
    await apiGet<UnitApiResponse<Unit[]>>("/units");

  return result.data ?? [];
}

export async function createUnit(
  request: CreateUnitRequest
): Promise<Unit> {
  const result =
    await apiPost<UnitApiResponse<Unit>>(
      "/units",
      request,
    );

  if (!result.data) {
    throw new Error(
      "Backend did not return the created unit."
    );
  }

  return result.data;
}

export async function updateUnit(
  currentName: string,
  request: UpdateUnitRequest
): Promise<Unit> {
  const result =
    await apiPut<UnitApiResponse<Unit>>(
      `/units/${encodeURIComponent(currentName)}`,
      request,
    );

  if (!result.data) {
    throw new Error(
      "Backend did not return the updated unit."
    );
  }

  return result.data;
}

export async function deleteUnit(
  name: string
): Promise<void> {
  await apiDelete<UnitApiResponse<never>>(
    `/units/${encodeURIComponent(name)}`
  );
}
