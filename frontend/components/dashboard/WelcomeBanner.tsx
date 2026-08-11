import { headers } from "next/headers";
import { getCurrentUser } from "@/services/auth.service";

function getGreetingFromServerTime() {
  const jakartaTime = new Intl.DateTimeFormat("en-US", {
    timeZone: "Asia/Jakarta",
    hour: "2-digit",
    hourCycle: "h23",
  }).format(new Date());

  const hour = Number(jakartaTime);

  if (hour < 11) {
    return "Selamat Pagi";
  }

  if (hour < 15) {
    return "Selamat Siang";
  }

  if (hour < 18) {
    return "Selamat Sore";
  }

  return "Selamat Malam";
}

export default async function WelcomeBanner() {
  const greeting = getGreetingFromServerTime();
  const requestHeaders = await headers();
  const cookieHeader = requestHeaders.get("cookie");
  const user = await getCurrentUser(cookieHeader ?? undefined);

  const displayName = user?.fullName?.trim() || user?.username || "User";

  return (
    <section className="rounded-2xl bg-gradient-to-r from-blue-700 to-sky-500 px-8 py-7 text-white shadow-lg">
      <h2 className="text-4xl font-bold tracking-tight">
        {greeting}, {displayName} 👋
      </h2>

      <p className="mt-4 text-lg text-blue-50">
        Selamat datang di Basarnas Digital Identity Platform.
      </p>

      <p className="mt-3 text-sm text-blue-100">
        Central Authentication • LDAP • SSO • Identity Governance
      </p>
    </section>
  );
}
