"use client";

import type { ComponentType, FormEvent } from "react";
import {
  Bar,
  BarChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import {
  Activity,
  ArrowDownToLine,
  ArrowUpToLine,
  Clock3,
  Download,
  Search,
  ShieldCheck,
  Users,
} from "lucide-react";
import {
  useEffect,
  useMemo,
  useRef,
  useState,
} from "react";

import {
  getAnalytics,
  searchAnalyticsUsers,
} from "@/services/analytics.service";
import type {
  AnalyticsData,
  AnalyticsUser,
} from "@/types/analytics";

function startOfMonth(date: Date) {
  return new Date(date.getFullYear(), date.getMonth(), 1);
}

function startOfNextMonth(date: Date) {
  return new Date(date.getFullYear(), date.getMonth() + 1, 1);
}

function toInputDate(date: Date) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");

  return `${year}-${month}-${day}`;
}

function dateToIso(value: string) {
  return new Date(`${value}T00:00:00`).toISOString();
}

function formatBytes(bytes: number) {
  if (!Number.isFinite(bytes) || bytes <= 0) return "0 B";

  const units = ["B", "KB", "MB", "GB", "TB"];
  let value = bytes;
  let unit = 0;

  while (value >= 1024 && unit < units.length - 1) {
    value /= 1024;
    unit += 1;
  }

  return `${value.toFixed(value >= 100 ? 0 : value >= 10 ? 1 : 2)} ${units[unit]}`;
}

function formatDuration(seconds: number) {
  if (!Number.isFinite(seconds) || seconds <= 0) return "0m";

  const totalMinutes = Math.floor(seconds / 60);
  const days = Math.floor(totalMinutes / 1440);
  const hours = Math.floor((totalMinutes % 1440) / 60);
  const minutes = totalMinutes % 60;

  if (days > 0) return `${days}d ${hours}h`;
  if (hours > 0) return `${hours}h ${minutes}m`;

  return `${minutes}m`;
}

function formatDate(value: string) {
  return new Date(value).toLocaleDateString("en-GB", {
    day: "2-digit",
    month: "short",
  });
}

function MetricCard({
  label,
  value,
  icon: Icon,
  detail,
}: {
  label: string;
  value: string;
  icon: ComponentType<{ size?: number }>;
  detail?: string;
}) {
  return (
    <div className="rounded-2xl border border-white/10 bg-[#111b2d] p-5 shadow-xl shadow-black/10">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-xs font-medium uppercase tracking-[0.16em] text-slate-400">
            {label}
          </p>
          <p className="mt-3 text-2xl font-semibold text-white">
            {value}
          </p>
          {detail && (
            <p className="mt-1 text-xs text-slate-500">{detail}</p>
          )}
        </div>

        <div className="rounded-xl bg-blue-500/10 p-3 text-blue-300">
          <Icon size={20} />
        </div>
      </div>
    </div>
  );
}

function UserTable({
  title,
  users,
  value,
  valueLabel,
}: {
  title: string;
  users: AnalyticsUser[];
  value: (user: AnalyticsUser) => string;
  valueLabel: string;
}) {
  return (
    <div className="rounded-2xl border border-white/10 bg-[#111b2d] p-5">
      <div className="mb-4 flex items-center justify-between">
        <h2 className="text-sm font-semibold text-white">{title}</h2>
        <span className="text-[11px] uppercase tracking-wider text-slate-500">
          {valueLabel}
        </span>
      </div>

      {users.length === 0 ? (
        <div className="py-8 text-center text-sm text-slate-500">
          No data for this period
        </div>
      ) : (
        <div className="space-y-2">
          {users.map((user, index) => (
            <div
              key={`${user.username}-${index}`}
              className="flex items-center justify-between rounded-xl border border-white/5 bg-slate-950/30 px-3 py-2.5"
            >
              <div className="flex min-w-0 items-center gap-3">
                <span className="w-5 text-center text-xs text-slate-500">
                  {index + 1}
                </span>
                <span className="truncate text-sm text-slate-200">
                  {user.username}
                </span>
              </div>

              <span className="ml-3 shrink-0 text-sm font-medium text-white">
                {value(user)}
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default function AnalyticsClient() {
  const now = new Date();

  const [preset, setPreset] = useState("this-month");
  const [fromDate, setFromDate] = useState(
    toInputDate(startOfMonth(now)),
  );
  const [toDate, setToDate] = useState(
    toInputDate(startOfNextMonth(now)),
  );
  const [username, setUsername] = useState("");
  const [selectedUsername, setSelectedUsername] = useState("");
  const [userSuggestions, setUserSuggestions] = useState<string[]>([]);
  const [showUserSuggestions, setShowUserSuggestions] = useState(false);
  const [searchingUsers, setSearchingUsers] = useState(false);
  const userSearchRequest = useRef(0);

  const [access, setAccess] = useState<
    "all" | "ovpn" | "hotspot"
  >("all");

  const [data, setData] = useState<AnalyticsData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  async function loadAnalytics(usernameOverride?: string) {
    setLoading(true);
    setError("");

    try {
      const response = await getAnalytics({
        from: dateToIso(fromDate),
        to: dateToIso(toDate),
        username: usernameOverride ?? username,
        access,
      });

      setData(response.data);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Failed to load analytics.",
      );
      setData(null);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadAnalytics();
  }, []);

  function applyPreset(value: string) {
    const current = new Date();

    if (value === "this-month") {
      setFromDate(toInputDate(startOfMonth(current)));
      setToDate(toInputDate(startOfNextMonth(current)));
    }

    if (value === "last-month") {
      setFromDate(
        toInputDate(
          new Date(current.getFullYear(), current.getMonth() - 1, 1),
        ),
      );
      setToDate(
        toInputDate(
          new Date(current.getFullYear(), current.getMonth(), 1),
        ),
      );
    }

    if (value === "last-3-months") {
      setFromDate(
        toInputDate(
          new Date(current.getFullYear(), current.getMonth() - 2, 1),
        ),
      );
      setToDate(toInputDate(startOfNextMonth(current)));
    }

    setPreset(value);
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    void loadAnalytics();
  }

  const dailyChart = useMemo(
    () =>
      (data?.daily ?? []).map((point) => ({
        ...point,
        dateLabel: formatDate(point.date),
        hours: Number((point.durationSeconds / 3600).toFixed(2)),
      })),
    [data],
  );

  const selectedUser = useMemo(() => {
    if (!selectedUsername || !data) return null;

    const ovpn = data.accessBreakdown.find(
      (item) => item.access === "OVPN",
    );
    const hotspot = data.accessBreakdown.find(
      (item) => item.access === "Hotspot",
    );

    return {
      username: selectedUsername,
      sessions: data.summary.totalSessions,
      durationSeconds: data.summary.totalDurationSeconds,
      downloadBytes: data.summary.totalDownloadBytes,
      uploadBytes: data.summary.totalUploadBytes,
      ovpnSessions: ovpn?.sessions ?? 0,
      ovpnDurationSeconds: ovpn?.durationSeconds ?? 0,
      hotspotSessions: hotspot?.sessions ?? 0,
      hotspotDurationSeconds: hotspot?.durationSeconds ?? 0,
    };
  }, [data, selectedUsername]);

  return (
    <main className="min-h-full bg-[#070d18] p-6 text-white md:p-8">
      <div className="mx-auto max-w-[1600px]">
        <div className="mb-6">
          <p className="text-xs font-semibold uppercase tracking-[0.2em] text-blue-400">
            Network Intelligence
          </p>
          <h1 className="mt-2 text-3xl font-semibold tracking-tight">
            Analytics
          </h1>
          <p className="mt-2 max-w-3xl text-sm text-slate-400">
            Understand who uses the network, how long they stay
            connected, and how much traffic moves through OVPN and
            Hotspot.
          </p>
        </div>

        <form
          onSubmit={handleSubmit}
          className="mb-6 rounded-2xl border border-white/10 bg-[#0d1626] p-4 shadow-xl shadow-black/10"
        >
          <div className="grid gap-3 lg:grid-cols-[1fr_180px_180px_160px_auto]">
            <label className="relative block">
              <span className="mb-1.5 block text-xs font-medium text-slate-400">
                Username
              </span>
              <Search
                size={17}
                className="absolute left-3 top-[31px] text-slate-500"
              />
              <div className="relative">
                <input
                  value={username}
                  onChange={(event) => {
                    const value = event.target.value;
                    setUsername(value);
                    setSelectedUsername("");
                    setShowUserSuggestions(true);

                    const requestId = ++userSearchRequest.current;

                    if (value.trim().length < 2) {
                      setUserSuggestions([]);
                      setSearchingUsers(false);
                      return;
                    }

                    setSearchingUsers(true);

                    window.setTimeout(async () => {
                      if (requestId !== userSearchRequest.current) return;

                      try {
                        const response = await searchAnalyticsUsers(
                          value.trim(),
                          10,
                        );

                        if (requestId === userSearchRequest.current) {
                          setUserSuggestions(response.data);
                        }
                      } catch {
                        if (requestId === userSearchRequest.current) {
                          setUserSuggestions([]);
                        }
                      } finally {
                        if (requestId === userSearchRequest.current) {
                          setSearchingUsers(false);
                        }
                      }
                    }, 250);
                  }}
                  onFocus={() => {
                    if (username.trim().length >= 2) {
                      setShowUserSuggestions(true);
                    }
                  }}
                  onBlur={() => {
                    window.setTimeout(
                      () => setShowUserSuggestions(false),
                      150,
                    );
                  }}
                  placeholder="Search user, e.g. dityo.mahendro"
                  className="h-10 w-full rounded-lg border border-white/10 bg-slate-950/60 pl-9 pr-3 text-sm text-white outline-none transition focus:border-blue-400/60"
                />

                {showUserSuggestions &&
                  username.trim().length >= 2 && (
                    <div className="absolute left-0 right-0 top-[44px] z-50 overflow-hidden rounded-xl border border-white/10 bg-[#111b2d] shadow-2xl shadow-black/30">
                      {searchingUsers ? (
                        <div className="px-4 py-3 text-xs text-slate-400">
                          Searching users...
                        </div>
                      ) : userSuggestions.length > 0 ? (
                        <div className="max-h-64 overflow-y-auto py-1">
                          {userSuggestions.map((suggestion) => (
                            <button
                              key={suggestion}
                              type="button"
                              onMouseDown={(event) => {
                                event.preventDefault();
                              }}
                              onClick={() => {
                                setUsername(suggestion);
                                setSelectedUsername(suggestion);
                                setUserSuggestions([]);
                                setShowUserSuggestions(false);
                                void loadAnalytics(suggestion);
                              }}
                              className="flex w-full items-center px-4 py-2.5 text-left text-sm text-slate-200 transition hover:bg-blue-500/10 hover:text-white"
                            >
                              <span className="truncate">
                                {suggestion}
                              </span>
                            </button>
                          ))}
                        </div>
                      ) : (
                        <div className="px-4 py-3 text-xs text-slate-500">
                          No matching users
                        </div>
                      )}
                    </div>
                  )}
              </div>
            </label>

            <label>
              <span className="mb-1.5 block text-xs font-medium text-slate-400">
                Period
              </span>
              <select
                value={preset}
                onChange={(event) => applyPreset(event.target.value)}
                className="h-10 w-full rounded-lg border border-white/10 bg-slate-950/60 px-3 text-sm text-white outline-none"
              >
                <option value="this-month">This month</option>
                <option value="last-month">Last month</option>
                <option value="last-3-months">Last 3 months</option>
                <option value="custom">Custom</option>
              </select>
            </label>

            <label>
              <span className="mb-1.5 block text-xs font-medium text-slate-400">
                Access
              </span>
              <select
                value={access}
                onChange={(event) =>
                  setAccess(
                    event.target.value as "all" | "ovpn" | "hotspot",
                  )
                }
                className="h-10 w-full rounded-lg border border-white/10 bg-slate-950/60 px-3 text-sm text-white outline-none"
              >
                <option value="all">All access</option>
                <option value="ovpn">OVPN</option>
                <option value="hotspot">Hotspot</option>
              </select>
            </label>

            <label>
              <span className="mb-1.5 block text-xs font-medium text-slate-400">
                From
              </span>
              <input
                type="date"
                value={fromDate}
                onChange={(event) => {
                  setPreset("custom");
                  setFromDate(event.target.value);
                }}
                className="h-10 w-full rounded-lg border border-white/10 bg-slate-950/60 px-3 text-sm text-white outline-none"
              />
            </label>

            <div className="flex items-end">
              <button
                type="submit"
                className="h-10 w-full rounded-lg bg-blue-500 px-5 text-sm font-semibold text-white transition hover:bg-blue-400 lg:w-auto"
              >
                Analyze
              </button>
            </div>
          </div>

          <div className="mt-3 flex flex-wrap items-center gap-3">
            <label className="flex items-center gap-2 text-xs text-slate-400">
              <span>To (exclusive)</span>
              <input
                type="date"
                value={toDate}
                onChange={(event) => {
                  setPreset("custom");
                  setToDate(event.target.value);
                }}
                className="h-9 rounded-lg border border-white/10 bg-slate-950/60 px-2 text-xs text-white outline-none"
              />
            </label>

            {username.trim() && (
              <button
                type="button"
                onClick={() => {
                  setUsername("");
                  setSelectedUsername("");
                  setUserSuggestions([]);
                  setShowUserSuggestions(false);
                  void loadAnalytics("");
                }}
                className="rounded-lg border border-white/10 px-3 py-2 text-xs text-slate-300 transition hover:bg-white/5"
              >
                Clear user filter
              </button>
            )}
          </div>
        </form>

        {loading && (
          <div className="rounded-2xl border border-white/10 bg-[#111b2d] p-10 text-center text-sm text-slate-400">
            Loading analytics...
          </div>
        )}

        {error && !loading && (
          <div className="rounded-2xl border border-red-400/20 bg-red-500/10 p-5 text-sm text-red-200">
            {error}
          </div>
        )}

        {!loading && !error && data && (
          <>
            {selectedUser && (
              <div className="mb-6 rounded-2xl border border-blue-400/20 bg-gradient-to-r from-blue-500/10 to-indigo-500/5 p-5">
                <p className="text-xs uppercase tracking-[0.18em] text-blue-300">
                  User Analytics
                </p>
                <div className="mt-1 flex flex-wrap items-center justify-between gap-3">
                  <h2 className="text-xl font-semibold text-white">
                    {selectedUser.username}
                  </h2>
                  <span className="rounded-full border border-blue-400/20 bg-blue-400/10 px-3 py-1 text-xs text-blue-200">
                    {selectedUser.sessions} sessions
                  </span>
                </div>
              </div>
            )}

            <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
              <MetricCard
                label="Total sessions"
                value={data.summary.totalSessions.toLocaleString("en-US")}
                icon={Activity}
                detail={`${data.summary.uniqueUsers} unique users`}
              />
              <MetricCard
                label="Connected time"
                value={formatDuration(data.summary.totalDurationSeconds)}
                icon={Clock3}
                detail={`${formatDuration(data.summary.ovpnDurationSeconds)} OVPN`}
              />
              <MetricCard
                label="Download"
                value={formatBytes(data.summary.totalDownloadBytes)}
                icon={ArrowDownToLine}
                detail="RADIUS accounting"
              />
              <MetricCard
                label="Upload"
                value={formatBytes(data.summary.totalUploadBytes)}
                icon={ArrowUpToLine}
                detail="RADIUS accounting"
              />
            </div>

            <div className="mt-4 grid gap-4 md:grid-cols-2 xl:grid-cols-4">
              <MetricCard
                label="OVPN sessions"
                value={data.summary.ovpnSessions.toLocaleString("en-US")}
                icon={ShieldCheck}
                detail={formatDuration(data.summary.ovpnDurationSeconds)}
              />
              <MetricCard
                label="Hotspot sessions"
                value={data.summary.hotspotSessions.toLocaleString("en-US")}
                icon={Activity}
                detail={formatDuration(data.summary.hotspotDurationSeconds)}
              />
              <MetricCard
                label="Total traffic"
                value={formatBytes(
                  data.summary.totalDownloadBytes +
                    data.summary.totalUploadBytes,
                )}
                icon={Download}
                detail="Download + Upload"
              />
              <MetricCard
                label="Users"
                value={data.summary.uniqueUsers.toLocaleString("en-US")}
                icon={Users}
                detail="Within selected period"
              />
            </div>

            <div className="mt-6 grid gap-4 xl:grid-cols-[1.6fr_1fr]">
              <div className="rounded-2xl border border-white/10 bg-[#111b2d] p-5">
                <h2 className="text-sm font-semibold text-white">
                  Connection Time by Day
                </h2>
                <p className="mt-1 text-xs text-slate-500">
                  Hours of overlapping connection time
                </p>

                <div className="mt-5 h-[320px]">
                  {dailyChart.length === 0 ? (
                    <div className="flex h-full items-center justify-center text-sm text-slate-500">
                      No connection data for this period
                    </div>
                  ) : (
                    <ResponsiveContainer width="100%" height="100%">
                      <BarChart data={dailyChart}>
                        <CartesianGrid
                          stroke="#263447"
                          strokeDasharray="4 4"
                          vertical={false}
                        />
                        <XAxis
                          dataKey="dateLabel"
                          stroke="#64748b"
                          tickLine={false}
                          axisLine={false}
                        />
                        <YAxis
                          stroke="#64748b"
                          tickLine={false}
                          axisLine={false}
                          width={45}
                        />
                        <Tooltip
                          contentStyle={{
                            background: "#0f172a",
                            border: "1px solid #334155",
                            borderRadius: "10px",
                          }}
                          formatter={(value) => [
                            `${value} hours`,
                            "Connected",
                          ]}
                        />
                        <Bar
                          dataKey="hours"
                          name="Connected"
                          fill="#3b82f6"
                          radius={[5, 5, 0, 0]}
                        />
                      </BarChart>
                    </ResponsiveContainer>
                  )}
                </div>
              </div>

              <div className="rounded-2xl border border-white/10 bg-[#111b2d] p-5">
                <h2 className="text-sm font-semibold text-white">
                  Access Breakdown
                </h2>
                <p className="mt-1 text-xs text-slate-500">
                  OVPN versus Hotspot
                </p>

                <div className="mt-5 space-y-3">
                  {data.accessBreakdown.map((item) => (
                    <div
                      key={item.access}
                      className="rounded-xl border border-white/5 bg-slate-950/30 p-4"
                    >
                      <div className="flex items-center justify-between">
                        <span className="font-medium text-white">
                          {item.access}
                        </span>
                        <span className="text-xs text-slate-400">
                          {item.sessions} sessions
                        </span>
                      </div>
                      <div className="mt-3 grid grid-cols-2 gap-3 text-xs">
                        <div>
                          <p className="text-slate-500">Duration</p>
                          <p className="mt-1 text-slate-200">
                            {formatDuration(item.durationSeconds)}
                          </p>
                        </div>
                        <div>
                          <p className="text-slate-500">Traffic</p>
                          <p className="mt-1 text-slate-200">
                            {formatBytes(
                              item.downloadBytes + item.uploadBytes,
                            )}
                          </p>
                        </div>
                      </div>
                    </div>
                  ))}

                  {data.accessBreakdown.length === 0 && (
                    <div className="py-8 text-center text-sm text-slate-500">
                      No access data for this period
                    </div>
                  )}
                </div>
              </div>
            </div>

            {!username.trim() && (
              <div className="mt-6 grid gap-4 xl:grid-cols-2">
                <UserTable
                  title="Top Download Users"
                  users={data.topDownloadUsers}
                  value={(user) => formatBytes(user.downloadBytes)}
                  valueLabel="Download"
                />
                <UserTable
                  title="Top Upload Users"
                  users={data.topUploadUsers}
                  value={(user) => formatBytes(user.uploadBytes)}
                  valueLabel="Upload"
                />
                <UserTable
                  title="Longest Connected Users"
                  users={data.topDurationUsers}
                  value={(user) => formatDuration(user.durationSeconds)}
                  valueLabel="Connected"
                />
                <UserTable
                  title="Most Frequent OVPN Users"
                  users={data.topOvpnUsers}
                  value={(user) => `${user.ovpnSessions} sessions`}
                  valueLabel="OVPN"
                />
              </div>
            )}

            {selectedUser && (
              <div className="mt-6 grid gap-4 md:grid-cols-2 xl:grid-cols-4">
                <MetricCard
                  label="User Download"
                  value={formatBytes(selectedUser.downloadBytes)}
                  icon={ArrowDownToLine}
                />
                <MetricCard
                  label="User Upload"
                  value={formatBytes(selectedUser.uploadBytes)}
                  icon={ArrowUpToLine}
                />
                <MetricCard
                  label="User OVPN"
                  value={`${selectedUser.ovpnSessions} sessions`}
                  icon={ShieldCheck}
                  detail={formatDuration(selectedUser.ovpnDurationSeconds)}
                />
                <MetricCard
                  label="User Hotspot"
                  value={`${selectedUser.hotspotSessions} sessions`}
                  icon={Activity}
                  detail={formatDuration(selectedUser.hotspotDurationSeconds)}
                />
              </div>
            )}
          </>
        )}
      </div>
    </main>
  );
}
