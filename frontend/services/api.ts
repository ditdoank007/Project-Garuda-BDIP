const API_BASE =
  typeof window === "undefined"
    ? (process.env.BDIP_INTERNAL_API_URL ?? "http://backend:8080/api")
    : (process.env.NEXT_PUBLIC_API_URL ?? "/api");

export async function api<T>(
  endpoint: string,
  options?: RequestInit
): Promise<T> {
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
  };

  if (options?.headers) {
    const extraHeaders = new Headers(options.headers);
    extraHeaders.forEach((value, key) => {
      headers[key] = value;
    });
  }

  if (
    typeof window === "undefined" &&
    process.env.BDIP_INTERNAL_API_SECRET
  ) {
    headers["X-BDIP-Internal-Secret"] =
      process.env.BDIP_INTERNAL_API_SECRET;
  }

  const response = await fetch(`${API_BASE}${endpoint}`, {
    ...options,
    cache: "no-store",
    credentials: "include",
    headers,
  });

  if (!response.ok) {
    let message = `API Error: ${response.status}`;

    try {
      const error = await response.json();

      if (error?.message) {
        message = error.message;
      }
    } catch {
    }

    throw new Error(message);
  }

  return response.json();
}



export function apiGet<T>(
  endpoint: string,
  options?: RequestInit,
) {
  return api<T>(endpoint, {
    method: "GET",
    ...options,
  });
}

export function apiPost<T>(
  endpoint: string,
  body: unknown,
) {
  return api<T>(endpoint, {
    method: "POST",
    body: JSON.stringify(body),
  });
}

export function apiPut<T>(
  endpoint: string,
  body: unknown,
) {
  return api<T>(endpoint, {
    method: "PUT",
    body: JSON.stringify(body),
  });
}

export function apiDelete<T>(endpoint: string) {
  return api<T>(endpoint, {
    method: "DELETE",
  });
}
