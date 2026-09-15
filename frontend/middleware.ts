import { NextRequest, NextResponse } from "next/server";

const userAllowedPaths = [
  "/dashboard",
  "/monitoring",
  "/users",
];

const publicPaths = [
  "/login",
  "/login-submit",
  "/logout-submit",
];

function isPathAllowed(pathname: string, paths: string[]) {
  return paths.some(
    (path) => pathname === path || pathname.startsWith(`${path}/`),
  );
}

export async function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

  const requestHeaders = new Headers(request.headers);
  requestHeaders.set("x-bdip-pathname", pathname);

  const hasSession = Boolean(
    request.cookies.get("bdip_session")?.value,
  );

  if (isPathAllowed(pathname, publicPaths)) {
    if (pathname === "/login" && hasSession) {
      return NextResponse.redirect(
        new URL("/dashboard", request.url),
      );
    }

    return NextResponse.next({
      request: {
        headers: requestHeaders,
      },
    });
  }

  // Semua halaman aplikasi membutuhkan session.
  if (!hasSession) {
    const loginUrl = new URL("/login", request.url);
    loginUrl.searchParams.set("next", pathname);
    return NextResponse.redirect(loginUrl);
  }

  // Ambil role dari session melalui endpoint /auth/me.
  try {
    const backendUrl =
      process.env.BDIP_INTERNAL_API_URL ??
      "http://backend:8080/api";

    const response = await fetch(`${backendUrl}/auth/me`, {
      method: "GET",
      headers: {
        Cookie: request.headers.get("cookie") ?? "",
        ...(process.env.BDIP_INTERNAL_API_SECRET
          ? {
              "X-BDIP-Internal-Secret":
                process.env.BDIP_INTERNAL_API_SECRET,
            }
          : {}),
      },
      cache: "no-store",
    });

    if (!response.ok) {
      const loginUrl = new URL("/login", request.url);
      loginUrl.searchParams.set("next", pathname);
      return NextResponse.redirect(loginUrl);
    }

    const result = await response.json();
    const role = String(result?.data?.role ?? "")
      .trim()
      .toLowerCase();

    const isAdministrator = role === "administrator";

    if (
      !isAdministrator &&
      !isPathAllowed(pathname, userAllowedPaths)
    ) {
      return NextResponse.redirect(
        new URL("/dashboard?error=forbidden", request.url),
      );
    }
  } catch {
    return NextResponse.redirect(
      new URL("/dashboard?error=forbidden", request.url),
    );
  }

  return NextResponse.next({
    request: {
      headers: requestHeaders,
    },
  });
}

export const config = {
  matcher: [
    /*
     * Semua halaman aplikasi.
     * API tidak ikut middleware ini karena API sudah
     * dilindungi GlobalAuthorizationMiddleware di backend.
     */
    "/((?!api|_next/static|_next/image|images|favicon.ico).*)",
  ],
};
