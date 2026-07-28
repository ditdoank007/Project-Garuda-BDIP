"use client";

import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

const API_URL =
  process.env.NEXT_PUBLIC_API_URL ??
  "http://192.168.100.120:8080/api";

export default function LoginPage() {
  const router = useRouter();

  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!username.trim() || !password) {
      toast.error("Username dan password wajib diisi.");
      return;
    }

    try {
      setSubmitting(true);

      const response = await fetch(`${API_URL}/auth/login`, {
        method: "POST",
        credentials: "include",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          username: username.trim(),
          password,
        }),
      });

      const responseText = await response.text();

      let result: {
        success?: boolean;
        message?: string;
      } = {};

      if (responseText.trim()) {
        try {
          result = JSON.parse(responseText);
        } catch {
          throw new Error(
            `Respons server tidak valid (HTTP ${response.status}).`,
          );
        }
      }

      if (!response.ok || !result.success) {
        throw new Error(
          result.message ??
            `Login gagal (HTTP ${response.status}).`,
        );
      }

      toast.success("Login berhasil.");
      router.replace("/dashboard");
      router.refresh();
    } catch (error) {
      toast.error(
        error instanceof Error
          ? error.message
          : "Login gagal.",
      );
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <main className="min-h-screen bg-slate-950 px-4 py-10 text-slate-100">
      <div className="mx-auto flex min-h-[calc(100vh-5rem)] max-w-md items-center">
        <section className="w-full rounded-2xl border border-slate-700 bg-slate-900 p-7 shadow-xl">
          <div className="mb-8 text-center">
            <h1 className="text-2xl font-semibold">
              BDIP
            </h1>
            <p className="mt-2 text-sm text-slate-400">
              Basarnas Digital Identity Platform
            </p>
          </div>

          <form className="space-y-5" onSubmit={handleSubmit}>
            <div className="space-y-2">
              <Label htmlFor="username">Username LDAP</Label>
              <Input
                id="username"
                autoComplete="username"
                value={username}
                onChange={(event) => setUsername(event.target.value)}
                disabled={submitting}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="password">Password</Label>
              <Input
                id="password"
                type="password"
                autoComplete="current-password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                disabled={submitting}
              />
            </div>

            <Button
              className="w-full"
              type="submit"
              disabled={submitting}
            >
              {submitting ? "Memproses..." : "Masuk ke BDIP"}
            </Button>
          </form>
        </section>
      </div>
    </main>
  );
}
