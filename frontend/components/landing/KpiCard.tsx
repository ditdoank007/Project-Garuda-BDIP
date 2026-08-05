import { LucideIcon } from "lucide-react";

type Props = {
  title: string;
  value: string;
  subtitle: string;
  icon: LucideIcon;
  color: string;
};

export default function KpiCard({
  title,
  value,
  subtitle,
  icon: Icon,
  color,
}: Props) {
  return (
    <div className="rounded-xl border border-[#223249] bg-[#0F1B2D] px-6 py-5 transition-all duration-300 hover:border-slate-500 hover:bg-[#12213a]">

      <div className="flex items-start justify-between">

        <div>

          <p className="text-sm text-slate-400">
            {title}
          </p>

          <h2 className="mt-2 text-4xl font-bold tracking-tight">
            {value}
          </h2>

          <p className="mt-2 text-sm font-medium text-green-400">
            {subtitle}
          </p>

        </div>

        <div
          className={`rounded-xl p-3 ${color}`}
        >
          <Icon className="h-6 w-6 text-white" />
        </div>

      </div>

    </div>
  );
}
