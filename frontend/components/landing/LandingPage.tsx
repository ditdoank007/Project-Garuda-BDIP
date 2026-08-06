import { CalendarDays, LogIn, Moon } from "lucide-react";
import Link from "next/link";
import KpiCard from "./KpiCard";
import GaugeCard from "@/components/dashboard/GaugeCard";
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

export default async function LandingPage() {

  const dashboard = await getDashboard();
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
              Today (31 July 2026)
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
            value={dashboard.stats.nasOnline.toLocaleString()}
            subtitle="RouterOS Connected"
            icon={Server}
            color="bg-sky-600"
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
            value={58}
            max={100}
            color="#f59e0b"
            suffix="%"
            subtitle="Network Usage"
          />

          <GaugeCard
            title="OVPN Usage"
            value={42}
            max={100}
            color="#a855f7"
            suffix="%"
            subtitle="Remote Access"
          />

          <GaugeCard
            title="NAS Online"
            value={97}
            max={100}
            color="#3b82f6"
            suffix="%"
            subtitle="29 of 30 Devices"
          />

          <GaugeCard
            title="Policies"
            value={45}
            max={100}
            color="#06b6d4"
            subtitle="Configured"
          />
        </div>
        <div className="mt-6 grid gap-6 xl:grid-cols-2">
          <LoginActivityChart />

          <ActiveSessionsChart />
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
