import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  allowedDevOrigins: [
      "192.168.100.124",
    "192.168.100.120",
    "bdip.sarsurabaya.id",
    "localhost",
    "127.0.0.1",
  ],

  async rewrites() {
    return [
      {
        source: "/api/:path*",
        destination: "http://backend:8080/api/:path*",
      },
    ];
  },
};

export default nextConfig;
