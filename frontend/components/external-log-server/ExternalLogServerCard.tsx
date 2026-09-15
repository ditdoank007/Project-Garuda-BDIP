"use client";

import { useEffect, useState } from "react";
import {
  apiGet,
  apiPut,
} from "@/services/api";

type ExternalLogServerConfig = {
  enabled: boolean;
  serverAddress: string | null;
  port: number;
  protocol: string;
  facility: number;
  updatedAt: string;
};

type ExternalLogServerResponse = {
  success: boolean;
  message?: string;
  data: ExternalLogServerConfig;
};

export default function ExternalLogServerCard() {
  const [enabled, setEnabled] = useState(false);
  const [serverAddress, setServerAddress] = useState("");
  const [port, setPort] = useState("514");
  const [protocol, setProtocol] = useState("UDP");
  const [facility, setFacility] = useState("3");

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState("");

  useEffect(() => {
    async function loadConfig() {
      try {
        const response =
          await apiGet<ExternalLogServerResponse>(
            "/external-log-server",
          );

        setEnabled(response.data.enabled);
        setServerAddress(
          response.data.serverAddress ?? "",
        );
        setPort(String(response.data.port));
        setProtocol(response.data.protocol);
        setFacility(String(response.data.facility));
      } catch (error) {
        setMessage(
          error instanceof Error
            ? error.message
            : "Gagal memuat konfigurasi external log server.",
        );
      } finally {
        setLoading(false);
      }
    }

    loadConfig();
  }, []);

  async function saveConfig() {
    setSaving(true);
    setMessage("");

    try {
      const response =
        await apiPut<ExternalLogServerResponse>(
          "/external-log-server",
          {
            enabled,
            serverAddress:
              serverAddress.trim() || null,
            port: Number(port),
            protocol,
            facility: Number(facility),
          },
        );

      setEnabled(response.data.enabled);
      setServerAddress(
        response.data.serverAddress ?? "",
      );
      setPort(String(response.data.port));
      setProtocol(response.data.protocol);
      setFacility(String(response.data.facility));

      setMessage(
        "Konfigurasi external log server berhasil disimpan.",
      );
    } catch (error) {
      setMessage(
        error instanceof Error
          ? error.message
          : "Gagal menyimpan konfigurasi.",
      );
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="rounded-xl border bg-card p-5">
      <div className="mb-5">
        <h2 className="text-base font-semibold">
          External Log Server
        </h2>

        <p className="mt-1 text-sm text-muted-foreground">
          Konfigurasi tujuan pengiriman audit log ke
          server syslog eksternal.
        </p>
      </div>

      {loading ? (
        <div className="py-4 text-sm text-muted-foreground">
          Memuat konfigurasi...
        </div>
      ) : (
        <>
          <div className="mb-5 flex items-center gap-3">
            <input
              id="external-log-enabled"
              type="checkbox"
              checked={enabled}
              onChange={(e) =>
                setEnabled(e.target.checked)
              }
              className="h-4 w-4"
            />

            <label
              htmlFor="external-log-enabled"
              className="text-sm font-medium"
            >
              Aktifkan External Log Server
            </label>
          </div>

          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
            <div className="lg:col-span-2">
              <label className="mb-1 block text-sm font-medium">
                Server Address
              </label>

              <input
                type="text"
                value={serverAddress}
                onChange={(e) =>
                  setServerAddress(e.target.value)
                }
                placeholder="192.168.33.200"
                className="w-full rounded-md border bg-background px-3 py-2 text-sm"
              />

              <p className="mt-1 text-xs text-muted-foreground">
                Bisa menggunakan IP atau hostname,
                misalnya logsys.sarsurabaya.id.
              </p>
            </div>

            <div>
              <label className="mb-1 block text-sm font-medium">
                Port
              </label>

              <input
                type="number"
                min={1}
                max={65535}
                value={port}
                onChange={(e) =>
                  setPort(e.target.value)
                }
                className="w-full rounded-md border bg-background px-3 py-2 text-sm"
              />
            </div>

            <div>
              <label className="mb-1 block text-sm font-medium">
                Protocol
              </label>

              <select
                value={protocol}
                onChange={(e) =>
                  setProtocol(e.target.value)
                }
                className="w-full rounded-md border bg-background px-3 py-2 text-sm"
              >
                <option value="UDP">UDP</option>
                <option value="TCP">TCP</option>
              </select>
            </div>

            <div>
              <label className="mb-1 block text-sm font-medium">
                Facility
              </label>

              <input
                type="number"
                min={0}
                max={23}
                value={facility}
                onChange={(e) =>
                  setFacility(e.target.value)
                }
                className="w-full rounded-md border bg-background px-3 py-2 text-sm"
              />

              <p className="mt-1 text-xs text-muted-foreground">
                BSD Syslog facility 0–23.
              </p>
            </div>
          </div>

          <div className="mt-5 flex items-center gap-3">
            <button
              type="button"
              onClick={saveConfig}
              disabled={saving}
              className="rounded-md border px-4 py-2 text-sm font-medium hover:bg-muted disabled:cursor-not-allowed disabled:opacity-50"
            >
              {saving
                ? "Menyimpan..."
                : "Simpan Konfigurasi"}
            </button>

            {message && (
              <span className="text-sm text-muted-foreground">
                {message}
              </span>
            )}
          </div>
        </>
      )}
    </div>
  );
}
