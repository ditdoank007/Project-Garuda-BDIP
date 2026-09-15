import { NextRequest, NextResponse } from "next/server";

const protectedPaths = [
  "/dashboard",
  "/monitoring",
  "/users",
  "/groups",
];

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

  const isProtected = protectedPaths.some(
    (path) => pathname === path || pathname.startsWith(`${path}/`),
  );

  const hasSession = Boolean(
    request.cookies.get("bdip_session")?.value,
  );

  if (isProtected && !hasSession) {
    const loginUrl = new URL("/login", request.url);

    loginUrl.searchParams.set("next", pathname);

    return NextResponse.redirect(loginUrl);
  }

  if (pathname === "/login" && hasSession) {
    return NextResponse.redirect(
      new URL("/dashboard", request.url),
    );
  }

  const requestHeaders = new Headers(request.headers);
  requestHeaders.set("x-bdip-pathname", pathname);

  return NextResponse.next({
    request: {
      headers: requestHeaders,
    },
  });
}

export const config = {
  matcher: [
    "/dashboard/:path*",
    "/monitoring/:path*",
    "/users/:path*",
    "/groups/:path*",
    "/login",
  ],
};
