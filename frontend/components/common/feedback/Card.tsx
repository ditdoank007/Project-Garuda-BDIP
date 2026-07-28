import { ReactNode } from "react";

type CardProps = {
  title?: string;
  children: ReactNode;
};

export default function Card({
  title,
  children,
}: CardProps) {
  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">

      {title && (
        <h2 className="mb-5 text-xl font-bold">
          {title}
        </h2>
      )}

      {children}

    </div>
  );
}