"use client";

import Image from "next/image";
import Link from "next/link";
import { useState } from "react";

export default function Logo() {
  const [imageFailed, setImageFailed] = useState(false);

  return (
    <Link
      href="/"
      className="flex items-center gap-3 text-white"
      aria-label="Basarnas Digital Identity Platform"
    >
      <div className="flex h-10 w-10 shrink-0 items-center justify-center overflow-hidden rounded-lg bg-white">
        {imageFailed ? (
          <span className="text-sm font-bold text-blue-700">B</span>
        ) : (
          <Image
            src="/images/basarnas-logo.png"
            alt="Logo Basarnas"
            width={40}
            height={40}
            className="h-9 w-9 object-contain"
            priority
            onError={() => setImageFailed(true)}
          />
        )}
      </div>

      <div className="min-w-0 leading-tight">
        <div className="text-sm font-semibold tracking-wide">
          Basarnas <span className="text-blue-400">BDIP</span>
        </div>
        <div className="text-xs text-slate-400">
          Digital Identity Platform
        </div>
      </div>
    </Link>
  );
}
