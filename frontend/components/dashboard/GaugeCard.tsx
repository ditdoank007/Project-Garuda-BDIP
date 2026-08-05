"use client";

type GaugeCardProps = {
  title: string;
  value: number;
  max?: number;
  suffix?: string;
  color?: string;
  subtitle?: string;
};

export default function GaugeCard({
  title,
  value,
  max = 100,
  suffix = "",
  color = "#22c55e",
  subtitle,
}: GaugeCardProps) {
  const percentage = Math.min(Math.max((value / max) * 100, 0), 100);

  const radius = 70;
  const stroke = 12;
  const circumference = Math.PI * radius;

  const dashOffset =
    circumference - (percentage / 100) * circumference;

  return (
    <div className="rounded-xl border border-slate-800/40 bg-[#111c2f] p-5 shadow-md">
      <h3 className="mb-3 text-center text-xl font-semibold tracking-wide text-slate-100">
        {title}
      </h3>

      <div className="flex justify-center">
        <svg width="170" height="100" viewBox="0 0 180 110">
          <path
            d="M20 90 A70 70 0 0 1 160 90"
            fill="none"
            stroke="#2b3548"
            strokeWidth={stroke}
            strokeLinecap="round"
          />

          <path
            d="M20 90 A70 70 0 0 1 160 90"
            fill="none"
            stroke={color}
            strokeWidth={stroke}
            strokeLinecap="round"
            strokeDasharray={circumference}
            strokeDashoffset={dashOffset}
            style={{
              transition: "stroke-dashoffset 0.8s ease",
            }}
          />

          <text
            x="90"
            y="70"
            textAnchor="middle"
            className="fill-white"
            style={{
              fontSize: 20,
              fontWeight: 700,
            }}
          >
            {value}
            {suffix}
          </text>
        </svg>
      </div>

      {subtitle && (
        <p className="mt-1 text-center text-sm text-slate-500">
          {subtitle}
        </p>
      )}
    </div>
  );
}
