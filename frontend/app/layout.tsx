import type { Metadata } from "next";
import { Inter, Plus_Jakarta_Sans } from "next/font/google";

import "./globals.css";
import { Toaster } from "@/components/ui/sonner";
import GlobalFeedbackDialog from "@/components/common/feedback/GlobalFeedbackDialog";
import AppShell from "@/components/layout/AppShell";

const inter = Inter({
  subsets: ["latin"],
  variable: "--font-inter",
});

const plusJakarta = Plus_Jakarta_Sans({
  subsets: ["latin"],
  variable: "--font-heading",
});

export const metadata: Metadata = {
  title: "BDIP",
  description: "Basarnas Digital Identity Platform",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html
      lang="id"
      className={`${inter.variable} ${plusJakarta.variable} h-full overflow-hidden antialiased`}
    >
    <body
      className="h-full overflow-hidden flex flex-col font-sans"
    >
        <AppShell>{children}</AppShell>
        <GlobalFeedbackDialog />
        <Toaster richColors position="top-right" />
      </body>
    </html>
  );
}