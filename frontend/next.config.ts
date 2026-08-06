import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  allowedDevOrigins: [
    "192.168.100.120",
    "bdip.sarsurabaya.id",
    "localhost",
    "127.0.0.1",
  ],
};

export default nextConfig;
