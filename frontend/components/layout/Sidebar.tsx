"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useEffect, useState } from "react";
import { Logo } from "../common";
import { getCurrentUser } from "@/services/auth.service";

import {
  LayoutDashboard,
  Activity,
  CalendarDays,
  Users,
  UsersRound,
  Building2,
  RadioTower,
  ShieldCheck,
  MapPinned,
  AppWindow,
  FileText,
  Settings,
  Network,
  Fingerprint,
  ClipboardCheck,
  ChevronDown,
  Database,
  Search,
  Eye,
  RefreshCw,
  Shield,
  Menu,
} from "lucide-react";

const administrationMenus = [
  {
    title: "Users",
    href: "/users",
    icon: Users,
  },
  {
    title: "Groups",
    href: "/groups",
    icon: UsersRound,
  },
  {
    title: "Units",
    href: "/units",
    icon: Building2,
  },
  {
    title: "Sessions",
    href: "/sessions",
    icon: RadioTower,
  },
  {
    title: "Roles",
    href: "/roles",
    icon: ShieldCheck,
  },
  {
    title: "Policies",
    href: "/policies",
    icon: Network,
  },
  {
    title: "Locations",
    href: "/locations",
    icon: MapPinned,
  },
  {
    title: "Applications",
    href: "/applications",
    icon: AppWindow,
  },
  {
    title: "Audit",
    href: "/audit",
    icon: FileText,
  },
  {
    title: "Settings",
    href: "/settings",
    icon: Settings,
  },
];

const attendanceMenus = [
  {
    title: "Master Attendance",
    href: "/attendance/master",
    icon: Database,
  },
  {
    title: "Master Mesin",
    href: "/attendance/master-machine",
    icon: Fingerprint,
  },
  {
    title: "Discovery",
    href: "/attendance/discovery",
    icon: Search,
  },
  {
    title: "Preview",
    href: "/attendance/preview",
    icon: Eye,
  },
  {
    title: "Sinkronisasi",
    href: "/attendance/synchronization",
    icon: RefreshCw,
  },
];

export default function Sidebar() {
  const pathname = usePathname();
  const [isAdministrator, setIsAdministrator] = useState(false);
  const [isCollapsed, setIsCollapsed] = useState(false);

  useEffect(() => {
    let mounted = true;

    getCurrentUser().then((user) => {
      if (mounted) {
        setIsAdministrator(
          user?.role?.trim().toLowerCase() === "administrator",
        );
      }
    });

    return () => {
      mounted = false;
    };
  }, []);

  const administrationActive = administrationMenus.some(
    (menu) => pathname === menu.href || pathname.startsWith(`${menu.href}/`),
  );

  const attendanceActive = pathname.startsWith("/attendance");

  return (
    <aside
      className={`relative z-20 h-full shrink-0 border-r border-white/10 bg-slate-950/75 text-white shadow-2xl shadow-black/20 backdrop-blur-xl transition-[width] duration-300 ease-in-out ${
        isCollapsed ? "w-16" : "w-72"
      }`}
    >
      <div
        className={`relative border-b border-white/10 transition-all duration-300 ${
          isCollapsed ? "h-16 p-2" : "p-6"
        }`}
      >
        {!isCollapsed && (
          <>
            <Logo />

            <div className="mt-4 flex items-center gap-2 border-t border-white/10 pt-4 text-xs text-slate-400">
              <CalendarDays size={15} />
              <span>
                Today (
                {new Intl.DateTimeFormat("en-GB", {
                  day: "2-digit",
                  month: "long",
                  year: "numeric",
                }).format(new Date())}
                )
              </span>
            </div>
          </>
        )}

        <button
          type="button"
          onClick={() => setIsCollapsed((value) => !value)}
          className={`flex h-10 w-10 items-center justify-center rounded-lg text-slate-300 transition hover:bg-white/10 hover:text-white ${
            isCollapsed ? "mx-auto" : "absolute right-3 top-3"
          }`}
          aria-label={isCollapsed ? "Tampilkan sidebar" : "Minimalkan sidebar"}
          title={isCollapsed ? "Tampilkan sidebar" : "Minimalkan sidebar"}
        >
          <Menu size={21} />
        </button>
      </div>

      <nav className="mt-4">
        <Link
          href="/dashboard"
          className={`mx-1 mb-1 flex items-center gap-3 rounded-lg px-3 py-3 transition ${
            pathname === "/dashboard"
              ? "bg-blue-500/15 text-white shadow-[inset_2px_0_0_rgba(96,165,250,.9)]"
              : "hover:bg-white/10"
          }`}
        >
          <LayoutDashboard size={20} />
          {!isCollapsed && <span>Dashboard</span>}
        </Link>

        <Link
          href="/monitoring"
          className={`mx-1 mb-1 flex items-center gap-3 rounded-lg px-3 py-3 transition ${
            pathname === "/monitoring"
              ? "bg-blue-500/15 text-white shadow-[inset_2px_0_0_rgba(96,165,250,.9)]"
              : "hover:bg-white/10"
          }`}
        >
          <Activity size={20} />
          {!isCollapsed && <span>Monitoring</span>}
        </Link>

        {isAdministrator ? (
          <>
            <div className={isCollapsed ? "mx-1 mt-1" : "mx-3 mt-1"}>
              <Link
                href="/users"
                className={`mb-1 flex items-center justify-between rounded-lg px-4 py-3 transition ${
                  administrationActive
                    ? "bg-blue-500/15 text-white shadow-[inset_2px_0_0_rgba(96,165,250,.9)]"
                    : "hover:bg-white/10"
                }`}
              >
                <span className="flex items-center gap-3">
                  <Shield size={20} />
                  {!isCollapsed && <span>Administration</span>}
                </span>

                {!isCollapsed && (
                  <ChevronDown
                    size={17}
                    className={`transition-transform ${
                      administrationActive ? "rotate-0" : "-rotate-90"
                    }`}
                  />
                )}
              </Link>

              {administrationActive && !isCollapsed && (
                <div className="mb-2 ml-4 border-l border-blue-400/20 pl-2">
                  {administrationMenus.map((menu) => {
                    const Icon = menu.icon;
                    const active =
                      pathname === menu.href ||
                      pathname.startsWith(`${menu.href}/`);

                    return (
                      <Link
                        key={menu.href}
                        href={menu.href}
                        className={`mb-1 flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm transition ${
                          active
                            ? "bg-blue-500/80 text-white shadow-lg shadow-blue-950/30"
                            : "text-slate-300 hover:bg-white/10 hover:text-white"
                        }`}
                      >
                        <Icon size={17} />
                        <span>{menu.title}</span>
                      </Link>
                    );
                  })}
                </div>
              )}
            </div>

            {!isCollapsed && (
              <div className="mt-6 px-6 pb-2 text-xs font-semibold uppercase tracking-wider text-slate-400">
                Machine
              </div>
            )}

            <div className={isCollapsed ? "mx-1" : "mx-3"}>
              <Link
                href="/attendance/master"
                className={`mb-1 flex items-center justify-between rounded-lg px-4 py-3 transition ${
                  attendanceActive
                    ? "bg-blue-500/15 text-white shadow-[inset_2px_0_0_rgba(96,165,250,.9)]"
                    : "hover:bg-white/10"
                }`}
              >
                <span className="flex items-center gap-3">
                  <ClipboardCheck size={20} />
                  {!isCollapsed && <span>Attendance</span>}
                </span>

                {!isCollapsed && (
                  <ChevronDown
                    size={17}
                    className={`transition-transform ${
                      attendanceActive ? "rotate-0" : "-rotate-90"
                    }`}
                  />
                )}
              </Link>

              {attendanceActive && !isCollapsed && (
                <div className="mb-2 ml-4 border-l border-blue-400/20 pl-2">
                  {attendanceMenus.map((menu) => {
                    const Icon = menu.icon;
                    const active = pathname === menu.href;

                    return (
                      <Link
                        key={menu.href}
                        href={menu.href}
                        className={`mb-1 flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm transition ${
                          active
                            ? "bg-blue-500/80 text-white shadow-lg shadow-blue-950/30"
                            : "text-slate-300 hover:bg-white/10 hover:text-white"
                        }`}
                      >
                        <Icon size={17} />
                        <span>{menu.title}</span>
                      </Link>
                    );
                  })}
                </div>
              )}
            </div>

            <Link
              href="/machine/finger"
              className={`mx-3 mb-1 flex items-center gap-3 rounded-lg px-4 py-3 transition ${
                pathname.startsWith("/machine/finger")
                  ? "bg-blue-500/15 text-white shadow-[inset_2px_0_0_rgba(96,165,250,.9)]"
                  : "hover:bg-white/10"
              }`}
            >
              <Fingerprint size={20} />
              {!isCollapsed && <span>Mesin Finger</span>}
            </Link>
          </>
        ) : (
          <Link
            href="/users"
            className={`mx-3 mb-1 flex items-center gap-3 rounded-lg px-4 py-3 transition ${
              pathname === "/users"
                ? "bg-blue-500/80 text-white shadow-lg shadow-blue-950/30"
                : "hover:bg-white/10"
            }`}
          >
            <Users size={20} />
            {!isCollapsed && <span>Users</span>}
          </Link>
        )}
      </nav>
    </aside>
  );
}
