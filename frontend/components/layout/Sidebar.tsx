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
    <aside className="w-72 bg-slate-900 text-white shadow-xl">
      <div className="border-b border-slate-800 p-6">
        <Logo />

        <div className="mt-4 flex items-center gap-2 border-t border-slate-800 pt-4 text-xs text-slate-400">
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
      </div>

      <nav className="mt-4">
        <Link
          href="/dashboard"
          className={`mx-3 mb-1 flex items-center gap-3 rounded-lg px-4 py-3 transition ${
            pathname === "/dashboard"
              ? "bg-slate-800 text-white"
              : "hover:bg-slate-800"
          }`}
        >
          <LayoutDashboard size={20} />
          <span>Dashboard</span>
        </Link>

        <Link
          href="/monitoring"
          className={`mx-3 mb-1 flex items-center gap-3 rounded-lg px-4 py-3 transition ${
            pathname === "/monitoring"
              ? "bg-slate-800 text-white"
              : "hover:bg-slate-800"
          }`}
        >
          <Activity size={20} />
          <span>Monitoring</span>
        </Link>

        {isAdministrator ? (
          <>
            <div className="mx-3 mt-1">
              <Link
                href="/users"
                className={`mb-1 flex items-center justify-between rounded-lg px-4 py-3 transition ${
                  administrationActive
                    ? "bg-slate-800 text-white"
                    : "hover:bg-slate-800"
                }`}
              >
                <span className="flex items-center gap-3">
                  <Shield size={20} />
                  <span>Administration</span>
                </span>

                <ChevronDown
                  size={17}
                  className={`transition-transform ${
                    administrationActive ? "rotate-0" : "-rotate-90"
                  }`}
                />
              </Link>

              {administrationActive && (
                <div className="mb-2 ml-4 border-l border-slate-700 pl-2">
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
                            ? "bg-blue-600 text-white"
                            : "text-slate-300 hover:bg-slate-800 hover:text-white"
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

            <div className="mt-6 px-6 pb-2 text-xs font-semibold uppercase tracking-wider text-slate-500">
              Machine
            </div>

            <div className="mx-3">
              <Link
                href="/attendance/master"
                className={`mb-1 flex items-center justify-between rounded-lg px-4 py-3 transition ${
                  attendanceActive
                    ? "bg-slate-800 text-white"
                    : "hover:bg-slate-800"
                }`}
              >
                <span className="flex items-center gap-3">
                  <ClipboardCheck size={20} />
                  <span>Attendance</span>
                </span>

                <ChevronDown
                  size={17}
                  className={`transition-transform ${
                    attendanceActive ? "rotate-0" : "-rotate-90"
                  }`}
                />
              </Link>

              {attendanceActive && (
                <div className="mb-2 ml-4 border-l border-slate-700 pl-2">
                  {attendanceMenus.map((menu) => {
                    const Icon = menu.icon;
                    const active = pathname === menu.href;

                    return (
                      <Link
                        key={menu.href}
                        href={menu.href}
                        className={`mb-1 flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm transition ${
                          active
                            ? "bg-blue-600 text-white"
                            : "text-slate-300 hover:bg-slate-800 hover:text-white"
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
                  ? "bg-slate-800 text-white"
                  : "hover:bg-slate-800"
              }`}
            >
              <Fingerprint size={20} />
              <span>Mesin Finger</span>
            </Link>
          </>
        ) : (
          <Link
            href="/users"
            className={`mx-3 mb-1 flex items-center gap-3 rounded-lg px-4 py-3 transition ${
              pathname === "/users"
                ? "bg-blue-600 text-white"
                : "hover:bg-slate-800"
            }`}
          >
            <Users size={20} />
            <span>Users</span>
          </Link>
        )}
      </nav>
    </aside>
  );
}
