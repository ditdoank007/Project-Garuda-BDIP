import { CalendarDays, LogIn, Moon } from "lucide-react";
import Link from "next/link";
import KpiCard from "./KpiCard";
import GaugeCard from "@/components/dashboard/GaugeCard";
import SynologyStorageCard from "@/components/dashboard/SynologyStorageCard";
import SynologyHardwareCard from "@/components/dashboard/SynologyHardwareCard";
import SynologySystemHealthCard from "@/components/dashboard/SynologySystemHealthCard";
import LoginActivityChart from "@/components/dashboard/LoginActivityChart";
import ActiveSessionsChart from "@/components/dashboard/ActiveSessionsChart";
import {
  TopNasCard,
  HotspotVpnCard,
  TopPoliciesCard,
  NasStatusCard,
  RecentActivityCard,
  TerminateCauseCard,
  TodayStatisticsCard,
} from "@/components/dashboard/panels";

import { Users, Wifi, RadioTower, Shield, Server, Boxes } from "lucide-react";
import { getDashboard } from "@/services/dashboard.service";
import { getMonitoringServers } from "@/services/monitoring.service";
import ServerMonitoringCard from "./ServerMonitoringCard";
import InfrastructureHealthCard from "./InfrastructureHealthCard";

export default async function LandingPage() {

  const dashboard = await getDashboard();
  const monitoring = await getMonitoringServers();

  const serverMetrics = monitoring.data;

  const serverBdIp =
    serverMetrics.find(
      (server) => server.name === "SERVER-BDIP",
    ) ?? serverMetrics[0];

  const databaseServer =
    serverMetrics.find(
      (server) => server.name === "SERVER-Garuda-DB",
    ) ?? serverMetrics[1];

  const infrastructureHealth = [
    {
      label: "SERVER-BDIP",
      status: serverBdIp?.isOnline ? "ONLINE" : "OFFLINE",
      healthy: serverBdIp?.isOnline ?? false,
    },
    {
      label: "BDIP DATABASE",
      status: databaseServer?.isOnline ? "ONLINE" : "OFFLINE",
      healthy: databaseServer?.isOnline ?? false,
    },
    {
      label: "OPENLDAP",
      status: dashboard.stats.ldap,
      healthy: dashboard.stats.ldap === "Healthy",
    },
    {
      label: "SYNOLOGY",
      status: dashboard.synology.online ? "ONLINE" : "OFFLINE",
      healthy: dashboard.synology.online,
    },
  ];

  const monitoringLastUpdated =
    serverMetrics
      .map((server) => server.lastUpdated)
      .filter(Boolean)
      .sort()
      .at(-1);

  return (
    <main className="min-h-screen bg-[#08111f] text-white">
      {/* Header */}
      <header className="border-b border-slate-800">
        <div className="mx-auto flex max-w-[1800px] items-center justify-between px-8 py-5">
          <div>
            <h1 className="text-3xl font-bold tracking-wide">
              BASARNAS DIGITAL IDENTITY PLATFORM (BDIP)
            </h1>

            <p className="mt-1 text-slate-400">
              Kantor Pencarian dan Pertolongan Surabaya
            </p>
          </div>

          <div className="flex items-center gap-3">
            <button className="flex items-center gap-2 rounded-lg border border-slate-700 bg-slate-900 px-4 py-2">
              <CalendarDays className="h-4 w-4" />
              {`Today (${new Intl.DateTimeFormat("en-GB", { day: "2-digit", month: "long", year: "numeric" }).format(new Date())})`}
            </button>

            <Link
              href="/login"
              className="rounded-lg bg-blue-600 px-6 py-2 font-semibold hover:bg-blue-700"
            >
              <span className="flex items-center gap-2">
                <LogIn className="h-4 w-4" />
                LOGIN
              </span>
            </Link>

            <button className="rounded-lg border border-slate-700 bg-slate-900 p-2">
              <Moon className="h-5 w-5" />
            </button>
          </div>
        </div>
      </header>

      {/* Content */}

      <div className="mx-auto max-w-[1800px] px-8 pt-8 pb-6">
        <div className="grid gap-6 lg:grid-cols-3 xl:grid-cols-6">
          <KpiCard
            title="Total Pengguna"
            value={dashboard.stats.totalUsers.toLocaleString()}
            subtitle="Akun Terdaftar"
            icon={Users}
            color="bg-blue-600"
          />

          <KpiCard
            title="Active Sessions"
            value={dashboard.stats.activeSessions.toLocaleString()}
            subtitle="RADIUS Active Sessions"
            icon={Wifi}
            color="bg-green-600"
          />

          <KpiCard
            title="Hotspot Sessions"
            value={dashboard.stats.hotspotSessions.toLocaleString()}
            subtitle="RouterOS Hotspot"
            icon={RadioTower}
            color="bg-orange-500"
          />

          <KpiCard
            title="OVPN / PPP"
            value={dashboard.stats.vpnSessions.toLocaleString()}
            subtitle="PPP Active Sessions"
            icon={Shield}
            color="bg-purple-600"
          />

          <KpiCard
            title="NAS Online"
            value={dashboard.synology.online ? "ONLINE" : "OFFLINE"}
            subtitle={`${dashboard.synology.model} • ${dashboard.synology.usedPercent}% Used`}
            icon={Server}
            color={dashboard.synology.online ? "bg-sky-600" : "bg-red-600"}
          />

          <KpiCard
            title="Applications"
            value={dashboard.stats.applications.toLocaleString()}
            subtitle="Integrated Applications"
            icon={Boxes}
            color="bg-fuchsia-600"
          />
        </div>
      </div>

      <div className="mx-auto max-w-[1800px] px-8 pb-8">
        <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-5">
          <GaugeCard
            title="Active Sessions"
            value={dashboard.stats.activeSessions}
            max={1000}
            color="#22c55e"
            subtitle="RADIUS Active Sessions"
          />

          <GaugeCard
            title="Hotspot Usage"
            value={dashboard.stats.hotspotSessions}
            max={100}
            color="#f59e0b"
            suffix="%"
            subtitle="Network Usage"
          />

          <GaugeCard
            title="OVPN Usage"
            value={dashboard.stats.vpnSessions}
            max={100}
            color="#a855f7"
            suffix="%"
            subtitle="Remote Access"
          />

          <GaugeCard
            title="NAS Storage"
            value={dashboard.synology.usedPercent}
            max={100}
            color="#3b82f6"
            suffix="%"
            subtitle={`${dashboard.synology.volumeName} • ${dashboard.synology.status}`}
          />

          <GaugeCard
            title="Policies"
            value={dashboard.stats.totalPolicies}
            max={100}
            color="#06b6d4"
            subtitle="Configured"
          />
        </div>
        <div className="mt-6 grid gap-6 xl:grid-cols-3">
          {serverBdIp && (
            <ServerMonitoringCard
              server={serverBdIp}
              title="SERVER-BDIP"
              subtitle="BDIP Application Server"
            />
          )}

          {databaseServer && (
            <ServerMonitoringCard
              server={databaseServer}
              title="BDIP DATABASE"
              subtitle="PostgreSQL Database Server"
            />
          )}

          <InfrastructureHealthCard
            items={infrastructureHealth}
            lastUpdated={
              monitoringLastUpdated
                ? new Date(monitoringLastUpdated).toLocaleString(
                    "en-GB",
                  )
                : undefined
            }
          />
        </div>
<div className="mt-6 grid gap-6 xl:grid-cols-2">
          <SynologyStorageCard
            synology={dashboard.synology}
          />

          <SynologyHardwareCard
            hardware={dashboard.synology.hardware}
          />
        </div>

        <div className="mt-6">
          <SynologySystemHealthCard
            health={dashboard.synology.systemHealth}
          />
        </div>

        <div className="mt-6 grid gap-6 xl:grid-cols-4">
          <TopNasCard />

          <HotspotVpnCard />

          <TopPoliciesCard />

          <NasStatusCard />
        </div>

        <div className="mt-6 grid gap-6 xl:grid-cols-3">
          <RecentActivityCard />

          <TerminateCauseCard />

          <TodayStatisticsCard />
        </div>


        
      </div>


      {/* Footer */}

      <footer className="border-t border-slate-800 py-5 text-center text-sm text-slate-500">
        2026 - BDIP - Powered by DM
      </footer>
    </main>
  );
}
