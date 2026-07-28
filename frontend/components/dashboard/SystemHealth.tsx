import {
  CheckCircle2,
  AlertCircle,
  Circle,
} from "lucide-react";

const services = [
  {
    name: "OpenLDAP",
    status: "Online",
    icon: CheckCircle2,
    color: "text-green-600",
  },
  {
    name: "Docker",
    status: "Running",
    icon: CheckCircle2,
    color: "text-green-600",
  },
  {
    name: "Keycloak",
    status: "Pending",
    icon: AlertCircle,
    color: "text-yellow-500",
  },
  {
    name: "FreeRADIUS",
    status: "Not Installed",
    icon: Circle,
    color: "text-slate-400",
  },
  {
    name: "PostgreSQL",
    status: "Running",
    icon: CheckCircle2,
    color: "text-green-600",
  },
];

export default function SystemHealth() {
  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
      <h2 className="text-xl font-bold">System Health</h2>

      <div className="mt-6 space-y-4">
        {services.map((service) => {
          const Icon = service.icon;

          return (
            <div
              key={service.name}
              className="flex items-center justify-between"
            >
              <div className="flex items-center gap-3">
                <Icon className={service.color} size={20} />
                <span>{service.name}</span>
              </div>

              <span className="text-sm text-slate-500">
                {service.status}
              </span>
            </div>
          );
        })}
      </div>
    </div>
  );
}