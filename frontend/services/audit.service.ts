import { apiGet } from "./api";

export type AuditLog = {
  id: string;
  createdAt: string;
  username?: string | null;
  fullName?: string | null;
  role?: string | null;
  action: string;
  module: string;
  target?: string | null;
  result: string;
  ipAddress?: string | null;
  userAgent?: string | null;
  details?: string | null;
};

export type AuditLogQuery = {
  page?: number;
  pageSize?: number;
  username?: string;
  module?: string;
  action?: string;
  result?: string;
  search?: string;
  from?: string;
  to?: string;
};

export type AuditLogResponse = {
  success: boolean;
  message: string;
  data: {
    logs: AuditLog[];
    page: number;
    pageSize: number;
    total: number;
    totalPages: number;
  };
};

export async function getAuditLogs(
  query: AuditLogQuery = {},
): Promise<AuditLogResponse> {
  const params = new URLSearchParams();

  params.set("page", String(query.page ?? 1));
  params.set("pageSize", String(query.pageSize ?? 50));

  if (query.username?.trim()) {
    params.set("username", query.username.trim());
  }

  if (query.module?.trim()) {
    params.set("module", query.module.trim());
  }

  if (query.action?.trim()) {
    params.set("action", query.action.trim());
  }

  if (query.result) {
    params.set("result", query.result);
  }

  if (query.search?.trim()) {
    params.set("search", query.search.trim());
  }

  if (query.from) {
    params.set("from", query.from);
  }

  if (query.to) {
    params.set("to", query.to);
  }

  return apiGet<AuditLogResponse>(
    `/audit?${params.toString()}`,
  );
}
