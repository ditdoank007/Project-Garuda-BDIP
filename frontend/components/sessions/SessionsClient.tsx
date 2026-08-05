"use client";

import { useEffect, useMemo, useState } from "react";
import {
  Activity,
  Clock3,
  MoreVertical,
  Power,
  RefreshCcw,
  Search,
  Users,
  Wifi,
} from "lucide-react";

import { api } from "@/services/api";

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
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { getSessions } from "@/services/session.service";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";


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
  const [selectedSession, setSelectedSession] =
  useState<RadiusSession | null>(null);
  const [sessionsData, setSessionsData] = useState(initialData);

  useEffect(() => {
  const timer = setInterval(async () => {
    try {
  const response = await getSessions();

  setSessionsData(response.data);
    } catch (error) {
      console.error("Auto refresh gagal.", error);
    }
  }, 10000);

  return () => clearInterval(timer);
}, []);

    async function disconnectSession(
    sessionId: string,
    username: string
  ) {
    const confirmed = window.confirm(
      `Disconnect session milik ${username}?`
    );

    if (!confirmed) {
      return;
    }

    try {
      await api(
        `/routeros/disconnect/${encodeURIComponent(sessionId)}`,
        {
          method: "POST",
        }
      );

      alert("Session berhasil diputus.");
      window.location.reload();
    } catch {
      alert("Disconnect gagal.");
    }
  }

  const filteredSessions = useMemo(() => {
  const normalizedQuery = query.trim().toLowerCase();

    return sessionsData.sessions.filter((session) => {
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
  }, [filter, sessionsData.sessions, query]);

  const summaryCards = [
    {
      title: "Total Sessions",
      value: sessionsData.summary.totalSessions,
      description: "Seluruh session accounting",
      icon: Activity,
    },
    {
      title: "Active Sessions",
      value: sessionsData.summary.activeSessions,
      description: "Session yang masih aktif",
      icon: Wifi,
    },
    {
      title: "Session History",
      value: sessionsData.summary.historicalSessions,
      description: "Session yang telah berakhir",
      icon: Clock3,
    },
    {
      title: "Unique Users",
      value: sessionsData.summary.uniqueUsers,
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

      <Card className="overflow-hidden flex flex-col">
        <div
            className="
              sticky
              top-0
              z-30
              border-b
              border-slate-200
              bg-white
              p-4
            "
          >
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

              <Button
                type="button"
                variant="destructive"
              >
                <Power className="mr-2 h-4 w-4" />
                Disconnect All
              </Button>
            </div>
          </div>

          <p className="mt-3 text-xs text-slate-500">
            Showing {filteredSessions.length} of {sessionsData.sessions.length} sessions
          </p>
        </div>

        <div
          className="
            max-h-[65vh]
            overflow-x-auto
            overflow-y-auto
          "
        >
          <Table>
              <TableHeader
                className="
                  sticky
                  top-0
                  z-20
                  bg-white
                "
              >
              <TableRow>
                <TableHead>Status</TableHead>
                <TableHead>User</TableHead>

                <TableHead>Policy</TableHead>

                <TableHead>Bandwidth</TableHead>

                <TableHead>Login Limit</TableHead>

                <TableHead>Access</TableHead>
                <TableHead>Client IP</TableHead>
                <TableHead>NAS</TableHead>
                <TableHead>Started</TableHead>
                <TableHead>Duration</TableHead>

                <TableHead>Traffic Down</TableHead>
                <TableHead>Traffic Up</TableHead>
                <TableHead className="w-[70px] text-center">
                  Action
                </TableHead>
              </TableRow>
            </TableHeader>

            <TableBody>
              {filteredSessions.length === 0 ? (
                <TableRow>
                  <TableCell
                    colSpan={13}
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
                      <div>
                        <p className="font-medium">
                          {session.policyCode || "-"}
                        </p>

                        <p className="text-xs text-slate-500">
                          {session.policyName || "-"}
                        </p>
                      </div>
                    </TableCell>

                    <TableCell>
                      <div className="text-sm">
                        <div>
                          ↓ {session.downloadRate ?? 0} kbps
                        </div>

                        <div>
                          ↑ {session.uploadRate ?? 0} kbps
                        </div>
                      </div>
                    </TableCell>

                    <TableCell>
                      {session.simultaneousUse ?? "-"}
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
                    <TableCell className="text-center">
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button
                            variant="ghost"
                            size="icon"
                          >
                            <MoreVertical className="h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>

                      <DropdownMenuContent align="end">

                        <DropdownMenuItem
                          onClick={() => setSelectedSession(session)}
                        >
                          View Detail
                        </DropdownMenuItem>

                        <DropdownMenuItem
                          onClick={() =>
                            disconnectSession(
                              session.routerOsId,
                              session.username
                            )
                          }
                          className="text-red-600"
                        >
                          Disconnect
                        </DropdownMenuItem>

                      </DropdownMenuContent>

                      </DropdownMenu>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </div>
      </Card>
      <Dialog
        open={selectedSession !== null}
        onOpenChange={(open) => {
          if (!open) {
            setSelectedSession(null);
          }
        }}
      >
        <DialogContent className="max-w-3xl">
          <DialogHeader>
            <DialogTitle>
              Session Detail
            </DialogTitle>
          </DialogHeader>

          {selectedSession && (
            <div className="grid grid-cols-2 gap-4 text-sm">

              <div>
                <strong>Username</strong><br />
                {selectedSession.username}
              </div>

              <div>
                <strong>Status</strong><br />
                {selectedSession.active ? "Active" : "Closed"}
              </div>

              <div>
                <strong>Client IP</strong><br />
                {selectedSession.framedIpAddress}
              </div>

              <div>
                <strong>NAS</strong><br />
                {selectedSession.nasIdentifier}
              </div>

              <div>
                <strong>NAS IP</strong><br />
                {selectedSession.nasIpAddress}
              </div>

              <div>
                <strong>Access</strong><br />
                {resolveAccessType(selectedSession)}
              </div>

              <div>
                <strong>Started</strong><br />
                {formatDateTime(selectedSession.startTime)}
              </div>

              <div>
                <strong>Ended</strong><br />
                {formatDateTime(selectedSession.stopTime)}
              </div>

              <div>
                <strong>Duration</strong><br />
                {formatDuration(selectedSession.sessionTimeSeconds)}
              </div>

              <div>
                <strong>Traffic Download</strong><br />
                {formatBytes(selectedSession.inputBytes)}
              </div>

              <div>
                <strong>Traffic Upload</strong><br />
                {formatBytes(selectedSession.outputBytes)}
              </div>

              <div>
                <strong>Terminate Cause</strong><br />
                {selectedSession.terminateCause ?? "-"}
              </div>
              <div>
                <strong>Router Server</strong><br />
                {selectedSession.routerServer || "-"}
              </div>

              <div>
                <strong>Router Address</strong><br />
                {selectedSession.routerAddress || "-"}
              </div>

              <div>
                <strong>RouterOS ID</strong><br />
                {selectedSession.routerOsId || "-"}
              </div>

              <div>
                <strong>MAC Address</strong><br />
                {selectedSession.macAddress || "-"}
              </div>

              <div>
                <strong>Router Status</strong><br />
                {selectedSession.isRouterActive ? "🟢 Online" : "⚪ Offline"}
              </div>

              <div>
                <strong>Policy Code</strong><br />
                {selectedSession.policyCode || "-"}
              </div>

              <div>
                <strong>Policy Name</strong><br />
                {selectedSession.policyName || "-"}
              </div>

              <div>
                <strong>Download Rate</strong><br />
                {selectedSession.downloadRate
                  ? `${selectedSession.downloadRate} kbps`
                  : "-"}
              </div>

              <div>
                <strong>Upload Rate</strong><br />
                {selectedSession.uploadRate
                  ? `${selectedSession.uploadRate} kbps`
                  : "-"}
              </div>

              <div>
                <strong>Session Timeout</strong><br />
                {selectedSession.sessionTimeout || "-"}
              </div>

              <div>
                <strong>Idle Timeout</strong><br />
                {selectedSession.idleTimeout || "-"}
              </div>

              <div>
                <strong>Simultaneous Use</strong><br />
                {selectedSession.simultaneousUse || "-"}
              </div>

            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
    
  );
}
