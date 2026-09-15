"use client";

import { useEffect, useState } from "react";

type BdipAtmosphereProps = {
  children: React.ReactNode;
};

type Particle = {
  id: number;
  x: number;
  y: number;
  size: number;
  delay: number;
  duration: number;
};

export default function BdipAtmosphere({
  children,
}: BdipAtmosphereProps) {
  const [pointer, setPointer] = useState({
    x: -1000,
    y: -1000,
  });

  const [particles] = useState<Particle[]>(() =>
    Array.from({ length: 22 }, (_, id) => ({
      id,
      x: 4 + Math.random() * 92,
      y: 8 + Math.random() * 84,
      size: 1 + Math.random() * 2,
      delay: Math.random() * 5,
      duration: 4 + Math.random() * 5,
    })),
  );

  useEffect(() => {
    function handlePointerMove(event: PointerEvent) {
      setPointer({
        x: event.clientX,
        y: event.clientY,
      });
    }

    window.addEventListener("pointermove", handlePointerMove);

    return () => {
      window.removeEventListener("pointermove", handlePointerMove);
    };
  }, []);

  return (
    <div className="relative h-full min-h-0 overflow-hidden bg-[#061326]">
      <div className="pointer-events-none absolute inset-0 overflow-hidden">
        <div className="absolute -left-48 -top-48 h-[42rem] w-[42rem] rounded-full bg-blue-600/10 blur-3xl" />
        <div className="absolute -bottom-64 -right-48 h-[42rem] w-[42rem] rounded-full bg-cyan-500/10 blur-3xl" />
        <div className="absolute left-1/2 top-1/2 h-[32rem] w-[32rem] -translate-x-1/2 -translate-y-1/2 rounded-full bg-indigo-500/[0.06] blur-3xl" />

        <div
          className="absolute inset-0 opacity-20"
          style={{
            backgroundImage:
              "linear-gradient(rgba(96,165,250,.10) 1px, transparent 1px), linear-gradient(90deg, rgba(96,165,250,.10) 1px, transparent 1px)",
            backgroundSize: "48px 48px",
            maskImage:
              "linear-gradient(to bottom, transparent, black 15%, black 85%, transparent)",
          }}
        />

        <div
          className="absolute h-72 w-72 rounded-full bg-cyan-300/10 blur-3xl transition-[left,top] duration-150"
          style={{
            left: pointer.x - 144,
            top: pointer.y - 144,
          }}
        />

        {particles.map((particle) => (
          <span
            key={particle.id}
            className="absolute rounded-full bg-cyan-200"
            style={{
              left: `${particle.x}%`,
              top: `${particle.y}%`,
              width: particle.size,
              height: particle.size,
              opacity: 0.25,
              boxShadow: "0 0 10px 2px rgba(103,232,249,.35)",
              animation: `bdipParticle ${particle.duration}s ease-in-out ${particle.delay}s infinite`,
            }}
          />
        ))}

        <div className="absolute bottom-0 left-0 right-0 flex h-32 items-end gap-3 px-8 opacity-20">
          {[
            "h-14",
            "h-24",
            "h-10",
            "h-20",
            "h-28",
            "h-16",
            "h-24",
            "h-12",
            "h-32",
            "h-20",
            "h-28",
            "h-16",
            "h-24",
            "h-12",
          ].map((height, index) => (
            <div
              key={index}
              className={`${height} flex-1 rounded-t-sm bg-blue-400/20`}
            />
          ))}
        </div>
      </div>

      <div className="relative z-10 flex h-full min-h-0">
        {children}
      </div>

      <style jsx global>{`
        @keyframes bdipParticle {
          0%,
          100% {
            transform: translate3d(0, 0, 0) scale(0.7);
            opacity: 0.15;
          }

          50% {
            transform: translate3d(0, -12px, 0) scale(1.35);
            opacity: 0.75;
          }
        }
      `}</style>
    </div>
  );
}
