import { NextRequest, NextResponse } from "next/server";

const protectedPaths = [
  "/dashboard",
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

  return NextResponse.next();
}

export const config = {
  matcher: [
    "/dashboard/:path*",
    "/users/:path*",
    "/groups/:path*",
    "/login",
  ],
};
