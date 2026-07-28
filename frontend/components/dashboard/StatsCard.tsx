import { LucideIcon } from "lucide-react";

type Props = {
  title: string;
  value: string;
  icon: LucideIcon;
  color: string;
  subtitle?: string;
};

export default function StatsCard({
  title,
  value,
  icon: Icon,
  color,
  subtitle,
}: Props) {
  return (
    <div className="rounded-2xl bg-white shadow-sm border border-slate-200 p-6 transition hover:shadow-xl hover:-translate-y-1">

      <div className="flex items-center justify-between">

        <div>

          <p className="text-sm text-slate-500">
            {title}
          </p>

          <h2 className="mt-2 text-4xl font-bold">
            {value}
          </h2>

          {subtitle && (
            <p className="mt-2 text-sm text-slate-400">
              {subtitle}
            </p>
          )}

        </div>

        <div
          className={`flex h-14 w-14 items-center justify-center rounded-xl ${color}`}
        >
          <Icon size={28} className="text-white" />
        </div>

      </div>

    </div>
  );
}
