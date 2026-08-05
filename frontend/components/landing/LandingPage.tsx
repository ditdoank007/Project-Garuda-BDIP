import { CalendarDays, LogIn, Moon } from "lucide-react";
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

export default function LandingPage() {
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

            <button className="rounded-lg bg-blue-600 px-6 py-2 font-semibold hover:bg-blue-700">
              <span className="flex items-center gap-2">
                <LogIn className="h-4 w-4" />
                LOGIN
              </span>
            </button>

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
            title="Total Users"
            value="2.184"
            subtitle="▲ 12 today"
            icon={Users}
            color="bg-blue-600"
          />

          <KpiCard
            title="Active Sessions"
            value="31"
            subtitle="▲ 2 vs yesterday"
            icon={Wifi}
            color="bg-green-600"
          />

          <KpiCard
            title="Hotspot Sessions"
            value="18"
            subtitle="▲ 3 vs yesterday"
            icon={RadioTower}
            color="bg-orange-500"
          />

          <KpiCard
            title="OVPN / PPP"
            value="13"
            subtitle="▼ 1 vs yesterday"
            icon={Shield}
            color="bg-purple-600"
          />

          <KpiCard
            title="NAS Online"
            value="29/30"
            subtitle="96.7% Online"
            icon={Server}
            color="bg-sky-600"
          />

          <KpiCard
            title="Applications"
            value="12"
            subtitle="▲ 1 New"
            icon={Boxes}
            color="bg-fuchsia-600"
          />
        </div>
      </div>

      <div className="mx-auto max-w-[1800px] px-8 pb-8">
        <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-5">
          <GaugeCard
            title="Active Sessions"
            value={31}
            max={100}
            color="#22c55e"
            subtitle="Currently Active"
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
