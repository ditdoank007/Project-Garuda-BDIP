import type {
  CreateUnitRequest,
  Unit,
  UnitApiResponse,
  UpdateUnitRequest,
} from "@/types/unit";

const API_URL =
  process.env.NEXT_PUBLIC_API_URL ??
  "http://192.168.100.120:8080/api";

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
  const response = await fetch(`${API_URL}/units`, {
    cache: "no-store",
    credentials: "include",
  });

  const result =
    await parseResponse<Unit[]>(response);

  return result.data ?? [];
}

export async function createUnit(
  request: CreateUnitRequest
): Promise<Unit> {
  const response = await fetch(`${API_URL}/units`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    credentials: "include",
    body: JSON.stringify(request),
  });

  const result =
    await parseResponse<Unit>(response);

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
  const response = await fetch(
    `${API_URL}/units/${encodeURIComponent(currentName)}`,
    {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      credentials: "include",
      body: JSON.stringify(request),
    }
  );

  const result =
    await parseResponse<Unit>(response);

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
  const response = await fetch(
    `${API_URL}/units/${encodeURIComponent(name)}`,
    {
      method: "DELETE",
      credentials: "include",
    }
  );

  await parseResponse<never>(response);
}
