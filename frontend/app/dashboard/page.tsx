import AppShell from "@/components/layout/AppShell";
import StatsCard from "@/components/dashboard/StatsCard";
import HealthCard from "@/components/dashboard/HealthCard";
import ActivityCard from "@/components/dashboard/ActivityCard";
import WelcomeBanner from "@/components/dashboard/WelcomeBanner";

import { getDashboard } from "@/services/dashboard.service";

import {
  Users,
  UsersRound,
  ShieldCheck,
  AppWindow,
} from "lucide-react";

export default async function DashboardPage() {

  const dashboard = await getDashboard();

  return (
    <AppShell>

      <WelcomeBanner />

      <div className="mt-6 grid gap-6 md:grid-cols-2 xl:grid-cols-4">

        <StatsCard
          title="Users"
          value={dashboard.stats.users.toString()}
          subtitle="LDAP Accounts"
          icon={Users}
          color="bg-blue-600"
        />

        <StatsCard
          title="Groups"
          value={dashboard.stats.groups.toString()}
          subtitle="Security Groups"
          icon={UsersRound}
          color="bg-green-600"
        />

        <StatsCard
          title="Applications"
          value={dashboard.stats.applications.toString()}
          subtitle="Integrated Apps"
          icon={AppWindow}
          color="bg-violet-600"
        />

        <StatsCard
          title="LDAP Status"
          value={dashboard.stats.ldap}
          subtitle="Server Online"
          icon={ShieldCheck}
          color="bg-emerald-600"
        />

      </div>

      <div className="mt-6 grid gap-6 lg:grid-cols-2">

        <HealthCard />

        <ActivityCard />

      </div>

    </AppShell>
  );
}