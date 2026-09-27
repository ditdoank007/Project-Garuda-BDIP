"use client";

import { FormEvent, useState } from "react";
import { Logo } from "@/components/common";

type LoginResponse = {
  success?: boolean;
  message?: string;
  data?: {
    sso_redirect?: string;
    ssoRedirect?: string;
  };
};

function Infunction InteractiveTitle() {
  return (
    <div className="select-none">
      <span className="block text-white">
        Satu identitas.
      </span>
      <span className="block bg-gradient-to-r from-cyan-200 via-blue-300 to-indigo-300 bg-clip-text text-transparent">
        Satu akses.
      </span>
    </div>
  );
}
