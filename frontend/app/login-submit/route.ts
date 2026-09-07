import { NextRequest, NextResponse } from "next/server";

export async function POST(request: NextRequest) {
  try {
    const formData = await request.formData();

    const username = String(formData.get("username") ?? "").trim();
    const password = String(formData.get("password") ?? "");

    if (!username || !password) {
      return NextResponse.redirect(
        new URL("/login?error=missing", request.url),
        303,
      );
    }

    const backendUrl =
      process.env.BDIP_INTERNAL_API_URL ??
      "http://backend:8080/api";

    const backendResponse = await fetch(
      `${backendUrl}/auth/login`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          username,
          password,
        }),
        cache: "no-store",
      },
    );

    if (!backendResponse.ok) {
      return NextResponse.redirect(
        new URL("/login?error=invalid", request.url),
        303,
      );
    }

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
      new URL("/dashboard", origin),
      303,
    );

    const setCookies = backendResponse.headers.getSetCookie();

    for (const cookie of setCookies) {
      response.headers.append("Set-Cookie", cookie);
    }

    return response;
  } catch (error) {
    console.error("LOGIN SUBMIT ERROR:", error);

    return NextResponse.redirect(
      new URL("/login?error=server", request.url),
      303,
    );
  }
}
