"use client";

import { useMemo, useState } from "react";
import {
  Activity,
  Clock3,
  RefreshCcw,
  Search,
  Users,
  Wifi,
} from "lucide-react";

import type {
  RadiusSession,
  SessionsData,
} from "@/types/session";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

type SessionsClientProps = {
  initialData: SessionsData;
};

type SessionFilter = "all" | "active" | "history";

function formatDuration(seconds: number) {
  const safeSeconds = Math.max(0, seconds ?? 0);

  const days = Math.floor(safeSeconds / 86400);
  const hours = Math.floor((safeSeconds % 86400) / 3600);
  const minutes = Math.floor((safeSeconds % 3600) / 60);
  const secs = safeSeconds % 60;

  if (days > 0) {
    return `${days}d ${hours}h ${minutes}m`;
  }

  if (hours > 0) {
    return `${hours}h ${minutes}m ${secs}s`;
  }

  if (minutes > 0) {
    return `${minutes}m ${secs}s`;
  }

  return `${secs}s`;
}

function formatBytes(bytes: number) {
  const value = Math.max(0, bytes ?? 0);

  if (value === 0) {
    return "0 B";
  }

  const units = ["B", "KB", "MB", "GB", "TB"];
  const index = Math.min(
    Math.floor(Math.log(value) / Math.log(1024)),
    units.length - 1
  );

  const converted = value / Math.pow(1024, index);

  return `${converted.toFixed(index === 0 ? 0 : 2)} ${units[index]}`;
}

function formatDateTime(value: string | null) {
  if (!value) {
    return "-";
  }

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat("id-ID", {
    timeZone: "Asia/Jakarta",
    dateStyle: "medium",
    timeStyle: "medium",
  }).format(date);
}

function resolveAccessType(session: RadiusSession) {
  const values = [
    session.serviceType,
    session.framedProtocol,
    session.calledStationId,
    session.nasIdentifier,
  ]
    .filter(Boolean)
    .join(" ")
    .toLowerCase();

  if (
    values.includes("ppp") ||
    values.includes("ovpn") ||
    values.includes("vpn")
  ) {
    return "OVPN / PPP";
  }

  if (
    values.includes("hotspot") ||
    values.includes("wireless") ||
    values.includes("wifi")
  ) {
    return "Hotspot";
  }

  return session.serviceType || "RADIUS";
}

export default function SessionsClient({
  initialData,
}: SessionsClientProps) {
  const [query, setQuery] = useState("");
  const [filter, setFilter] = useState<SessionFilter>("all");

  const filteredSessions = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase();

    return initialData.sessions.filter((session) => {
      if (filter === "active" && !session.active) {
        return false;
      }

      if (filter === "history" && session.active) {
        return false;
      }

      if (!normalizedQuery) {
        return true;
      }

      const searchable = [
        session.username,
        session.nasIpAddress,
        session.nasIdentifier,
        session.framedIpAddress,
        session.callingStationId,
        session.calledStationId,
        session.serviceType,
        session.framedProtocol,
        session.terminateCause,
      ]
        .filter(Boolean)
        .join(" ")
        .toLowerCase();

      return searchable.includes(normalizedQuery);
    });
  }, [filter, initialData.sessions, query]);

  const summaryCards = [
    {
      title: "Total Sessions",
      value: initialData.summary.totalSessions,
      description: "Seluruh session accounting",
      icon: Activity,
    },
    {
      title: "Active Sessions",
      value: initialData.summary.activeSessions,
      description: "Session yang masih aktif",
      icon: Wifi,
    },
    {
      title: "Session History",
      value: initialData.summary.historicalSessions,
      description: "Session yang telah berakhir",
      icon: Clock3,
    },
    {
      title: "Unique Users",
      value: initialData.summary.uniqueUsers,
      description: "User unik pada accounting",
      icon: Users,
    },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight text-slate-900">
          Sessions
        </h1>

        <p className="mt-1 text-sm text-slate-500">
          Monitor RADIUS accounting, active sessions, and historical network access.
        </p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        {summaryCards.map((item) => {
          const Icon = item.icon;

          return (
            <Card key={item.title} className="p-5">
              <div className="flex items-start justify-between gap-4">
                <div>
                  <p className="text-sm font-medium text-slate-500">
                    {item.title}
                  </p>

                  <p className="mt-2 text-3xl font-bold text-slate-900">
                    {item.value}
                  </p>

                  <p className="mt-1 text-xs text-slate-500">
                    {item.description}
                  </p>
                </div>

                <div className="rounded-xl bg-slate-100 p-3 text-slate-700">
                  <Icon className="h-5 w-5" />
                </div>
              </div>
            </Card>
          );
        })}
      </div>

      <Card className="overflow-hidden">
        <div className="border-b border-slate-200 p-4">
          <div className="flex flex-col gap-3 xl:flex-row xl:items-center xl:justify-between">
            <div className="relative w-full xl:max-w-md">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />

              <Input
                value={query}
                onChange={(event) => setQuery(event.target.value)}
                placeholder="Search username, IP, NAS, MAC, service..."
                className="pl-9"
              />
            </div>

            <div className="flex flex-wrap items-center gap-2">
              <Button
                type="button"
                variant={filter === "all" ? "default" : "outline"}
                onClick={() => setFilter("all")}
              >
                All
              </Button>

              <Button
                type="button"
                variant={filter === "active" ? "default" : "outline"}
                onClick={() => setFilter("active")}
              >
                Active
              </Button>

              <Button
                type="button"
                variant={filter === "history" ? "default" : "outline"}
                onClick={() => setFilter("history")}
              >
                History
              </Button>

              <Button
                type="button"
                variant="outline"
                onClick={() => window.location.reload()}
              >
                <RefreshCcw className="mr-2 h-4 w-4" />
                Refresh
              </Button>
            </div>
          </div>

          <p className="mt-3 text-xs text-slate-500">
            Showing {filteredSessions.length} of {initialData.sessions.length} sessions
          </p>
        </div>

        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Status</TableHead>
                <TableHead>User</TableHead>
                <TableHead>Access</TableHead>
                <TableHead>Client IP</TableHead>
                <TableHead>NAS</TableHead>
                <TableHead>Started</TableHead>
                <TableHead>Duration</TableHead>
                <TableHead>Download</TableHead>
                <TableHead>Upload</TableHead>
              </TableRow>
            </TableHeader>

            <TableBody>
              {filteredSessions.length === 0 ? (
                <TableRow>
                  <TableCell
                    colSpan={9}
                    className="h-32 text-center text-slate-500"
                  >
                    No sessions found.
                  </TableCell>
                </TableRow>
              ) : (
                filteredSessions.map((session) => (
                  <TableRow key={session.id}>
                    <TableCell>
                      {session.active ? (
                        <Badge className="bg-emerald-100 text-emerald-700 hover:bg-emerald-100">
                          Active
                        </Badge>
                      ) : (
                        <Badge variant="secondary">
                          Closed
                        </Badge>
                      )}
                    </TableCell>

                    <TableCell>
                      <div>
                        <p className="font-medium text-slate-900">
                          {session.username}
                        </p>

                        <p className="text-xs text-slate-500">
                          {session.callingStationId || "-"}
                        </p>
                      </div>
                    </TableCell>

                    <TableCell>
                      {resolveAccessType(session)}
                    </TableCell>

                    <TableCell>
                      {session.framedIpAddress || "-"}
                    </TableCell>

                    <TableCell>
                      <div>
                        <p className="text-slate-900">
                          {session.nasIdentifier || "-"}
                        </p>

                        <p className="text-xs text-slate-500">
                          {session.nasIpAddress || "-"}
                        </p>
                      </div>
                    </TableCell>

                    <TableCell>
                      {formatDateTime(session.startTime)}
                    </TableCell>

                    <TableCell>
                      {formatDuration(session.sessionTimeSeconds)}
                    </TableCell>

                    <TableCell>
                      {formatBytes(session.outputBytes)}
                    </TableCell>

                    <TableCell>
                      {formatBytes(session.inputBytes)}
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </div>
      </Card>
    </div>
  );
}
