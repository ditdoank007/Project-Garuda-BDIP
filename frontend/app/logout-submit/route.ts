import { NextRequest, NextResponse } from "next/server";

export async function POST(request: NextRequest) {
  try {
    const backendUrl =
      process.env.BDIP_INTERNAL_API_URL ??
      "http://backend:8080/api";

    const cookieHeader = request.headers.get("cookie") ?? "";

    const backendResponse = await fetch(
      `${backendUrl}/auth/logout`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          ...(cookieHeader
            ? { Cookie: cookieHeader }
            : {}),
        },
        body: "{}",
        cache: "no-store",
      },
    );

    const proto =
      request.headers.get("x-forwarded-proto") ?? "http";
    const host =
      request.headers.get("x-forwarded-host") ??
      request.headers.get("host");

    if (!host) {
      throw new Error("Missing request host.");
    }

    const origin = `${proto}://${host}`;

    const response = NextResponse.redirect(
      new URL("/", origin),
      303,
    );

    const setCookies = backendResponse.headers.getSetCookie();

    for (const cookie of setCookies) {
      response.headers.append("Set-Cookie", cookie);
    }

    return response;
  } catch (error) {
    console.error("LOGOUT SUBMIT ERROR:", error);

    const proto =
      request.headers.get("x-forwarded-proto") ?? "http";
    const host =
      request.headers.get("x-forwarded-host") ??
      request.headers.get("host");

    const origin = host
      ? `${proto}://${host}`
      : "http://192.168.100.124";

    return NextResponse.redirect(
      new URL("/", origin),
      303,
    );
  }
}
