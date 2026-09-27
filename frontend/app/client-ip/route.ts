import { NextRequest, NextResponse } from "next/server";

export async function GET(request: NextRequest) {
  const forwardedFor = request.headers.get("x-forwarded-for");

  const ip =
    forwardedFor
      ?.split(",")
      .map((value) => value.trim())
      .find(Boolean) ??
    request.headers.get("x-real-ip") ??
    request.headers.get("x-client-ip") ??
    "Tidak diketahui";

  return NextResponse.json(
    { ip },
    {
      headers: {
        "Cache-Control": "no-store, no-cache, must-revalidate",
      },
    },
  );
}
