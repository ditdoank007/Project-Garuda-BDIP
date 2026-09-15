"use client";

import { useCallback, useEffect, useState } from "react";
import DataTable from "@/components/common/data/DataTable";
import PageHeader from "@/components/common/layout/PageHeader";
import {
  TableCell,
  TableHead,
  TableRow,
} from "@/components/ui/table";
import {
  getAuditLogs,
  type AuditLog,
} from "@/services/audit.service";

const PAGE_SIZE = 50;

const RESULT_OPTIONS = [
  "",
  "SUCCESS",
  "DENIED",
  "FAILED",
];

function resultClass(result: string) {
  switch (result.toUpperCase()) {
    case "SUCCESS":
      return "font-semibold text-green-600 dark:text-green-400";
    case "DENIED":
      return "font-semibold text-orange-600 dark:text-orange-400";
    case "FAILED":
      return "font-semibold text-red-600 dark:text-red-400";
    default:
      return "font-semibold";
  }
}

function formatTimestamp(value: string) {
  return new Date(value).toLocaleString("id-ID", {
    dateStyle: "medium",
    timeStyle: "medium",
  });
}

export default function AuditClient() {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [totalPages, setTotalPages] = useState(0);

  const [username, setUsername] = useState("");
  const [module, setModule] = useState("");
  const [action, setAction] = useState("");
  const [result, setResult] = useState("");
  const [search, setSearch] = useState("");
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const loadLogs = useCallback(async () => {
    setLoading(true);
    setError("");

    try {
      const response = await getAuditLogs({
        page,
        pageSize: PAGE_SIZE,
        username,
        module,
        action,
        result,
        search,
        from: from
          ? `${from}T00:00:00+07:00`
          : undefined,
        to: to
          ? `${to}T23:59:59.999+07:00`
          : undefined,
      });

      setLogs(response.data.logs);
      setTotal(response.data.total);
      setTotalPages(response.data.totalPages);
    } catch (err) {
      setLogs([]);
      setTotal(0);
      setTotalPages(0);

      setError(
        err instanceof Error
          ? err.message
          : "Gagal memuat Log Activity.",
      );
    } finally {
      setLoading(false);
    }
  }, [
    page,
    username,
    module,
    action,
    result,
    search,
    from,
    to,
  ]);

  useEffect(() => {
    loadLogs();
  }, [loadLogs]);

  function applyFilters() {
    setPage(1);
  }

  function resetFilters() {
    setUsername("");
    setModule("");
    setAction("");
    setResult("");
    setSearch("");
    setFrom("");
    setTo("");
    setPage(1);
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Log Activity"
        description="Riwayat aktivitas dan perubahan yang terjadi di BDIP."
      />

      <div className="rounded-xl border bg-card p-4">
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
          <div>
            <label className="mb-1 block text-sm font-medium">
              Dari Tanggal
            </label>
            <input
              type="date"
              value={from}
              onChange={(e) => setFrom(e.target.value)}
              className="w-full rounded-md border bg-background px-3 py-2 text-sm"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium">
              Sampai Tanggal
            </label>
            <input
              type="date"
              value={to}
              onChange={(e) => setTo(e.target.value)}
              className="w-full rounded-md border bg-background px-3 py-2 text-sm"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium">
              User
            </label>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              placeholder="Username"
              className="w-full rounded-md border bg-background px-3 py-2 text-sm"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium">
              Module
            </label>
            <input
              type="text"
              value={module}
              onChange={(e) => setModule(e.target.value)}
              placeholder="Contoh: Users"
              className="w-full rounded-md border bg-background px-3 py-2 text-sm"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium">
              Activity
            </label>
            <input
              type="text"
              value={action}
              onChange={(e) => setAction(e.target.value)}
              placeholder="Contoh: RESET_PASSWORD"
              className="w-full rounded-md border bg-background px-3 py-2 text-sm"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium">
              Result
            </label>
            <select
              value={result}
              onChange={(e) => setResult(e.target.value)}
              className="w-full rounded-md border bg-background px-3 py-2 text-sm"
            >
              {RESULT_OPTIONS.map((item) => (
                <option key={item} value={item}>
                  {item || "Semua"}
                </option>
              ))}
            </select>
          </div>

          <div className="md:col-span-2">
            <label className="mb-1 block text-sm font-medium">
              Search
            </label>
            <input
              type="text"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") {
                  applyFilters();
                }
              }}
              placeholder="Cari user, nama, activity, module, target, atau detail..."
              className="w-full rounded-md border bg-background px-3 py-2 text-sm"
            />
          </div>
        </div>

        <div className="mt-4 flex gap-2">
          <button
            type="button"
            onClick={applyFilters}
            className="rounded-md border px-4 py-2 text-sm font-medium hover:bg-muted"
          >
            Terapkan Filter
          </button>

          <button
            type="button"
            onClick={resetFilters}
            className="rounded-md border px-4 py-2 text-sm font-medium hover:bg-muted"
          >
            Reset
          </button>
        </div>
      </div>

      {error && (
        <div className="rounded-xl border border-red-300 bg-red-50 p-4 text-sm text-red-700 dark:border-red-900 dark:bg-red-950/30 dark:text-red-300">
          {error}
        </div>
      )}


      <div className="text-sm text-muted-foreground">
        {loading
          ? "Memuat Log Activity..."
          : `${total.toLocaleString("id-ID")} aktivitas`}
      </div>

      <DataTable
        headers={
          <>
            <TableHead className="sticky top-0 z-20 bg-white">Timestamp</TableHead>
            <TableHead className="sticky top-0 z-20 bg-white">Username</TableHead>
            <TableHead className="sticky top-0 z-20 bg-white">Nama Lengkap</TableHead>
            <TableHead className="sticky top-0 z-20 bg-white">Role</TableHead>
            <TableHead className="sticky top-0 z-20 bg-white">Activity</TableHead>
            <TableHead className="sticky top-0 z-20 bg-white">Module</TableHead>
            <TableHead className="sticky top-0 z-20 bg-white">Target</TableHead>
            <TableHead className="sticky top-0 z-20 bg-white">Result</TableHead>
            <TableHead className="sticky top-0 z-20 bg-white">IP Address</TableHead>
            <TableHead className="sticky top-0 z-20 bg-white">Details</TableHead>
          </>
        }
      >
        {loading ? (
          <TableRow>
            <TableCell
              colSpan={10}
              className="py-10 text-center"
            >
              Memuat data...
            </TableCell>
          </TableRow>
        ) : logs.length === 0 ? (
          <TableRow>
            <TableCell
              colSpan={10}
              className="py-10 text-center"
            >
              Tidak ada aktivitas.
            </TableCell>
          </TableRow>
        ) : (
          logs.map((log) => (
            <TableRow key={log.id}>
              <TableCell>
                {formatTimestamp(log.createdAt)}
              </TableCell>
              <TableCell>{log.username || "-"}</TableCell>
              <TableCell>{log.fullName || "-"}</TableCell>
              <TableCell>{log.role || "-"}</TableCell>
              <TableCell className="font-medium">
                {log.action}
              </TableCell>
              <TableCell>{log.module}</TableCell>
              <TableCell>{log.target || "-"}</TableCell>
              <TableCell>
                <span className={resultClass(log.result)}>
                  {log.result}
                </span>
              </TableCell>
              <TableCell>{log.ipAddress || "-"}</TableCell>
              <TableCell
                className="max-w-[360px] truncate"
                title={log.details || ""}
              >
                {log.details || "-"}
              </TableCell>
            </TableRow>
          ))
        )}
      </DataTable>

      <div className="flex items-center justify-between">
        <div className="text-sm text-muted-foreground">
          Halaman {page} dari {totalPages || 1}
        </div>

        <div className="flex gap-2">
          <button
            type="button"
            disabled={page <= 1 || loading}
            onClick={() =>
              setPage((value) => Math.max(1, value - 1))
            }
            className="rounded-md border px-4 py-2 text-sm disabled:cursor-not-allowed disabled:opacity-50"
          >
            Sebelumnya
          </button>

          <button
            type="button"
            disabled={
              loading ||
              totalPages === 0 ||
              page >= totalPages
            }
            onClick={() =>
              setPage((value) =>
                Math.min(totalPages, value + 1),
              )
            }
            className="rounded-md border px-4 py-2 text-sm disabled:cursor-not-allowed disabled:opacity-50"
          >
            Berikutnya
          </button>
        </div>
      </div>
    </div>
  );
}
