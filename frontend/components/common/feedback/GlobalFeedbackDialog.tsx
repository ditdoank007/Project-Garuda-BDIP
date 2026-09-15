"use client";

import { useEffect, useState } from "react";

type FeedbackType = "denied" | "success";

export default function GlobalFeedbackDialog() {
  const [type, setType] = useState<FeedbackType | null>(null);

  useEffect(() => {
    if (!type) return;

    const timer = window.setTimeout(() => {
      setType(null);
    }, 2500);

    return () => window.clearTimeout(timer);
  }, [type]);

  useEffect(() => {
    const showDenied = () => {
      setType("denied");
    };

    const showSuccess = () => {
      setType("success");
    };

    window.addEventListener(
      "bdip:authorization-denied",
      showDenied,
    );

    window.addEventListener(
      "bdip:authorization-success",
      showSuccess,
    );

    /*
     * Global fetch interceptor.
     *
     * Semua HTTP 403 dari browser akan memunculkan
     * Global Access Denied Dialog, tanpa perlu
     * halaman/module membuat popup sendiri.
     */
    const originalFetch = window.fetch;

    window.fetch = async (
      input: RequestInfo | URL,
      init?: RequestInit,
    ) => {
      const response = await originalFetch(input, init);

      if (response.status === 403) {
        window.dispatchEvent(
          new CustomEvent("bdip:authorization-denied"),
        );
      }

      return response;
    };

    return () => {
      window.removeEventListener(
        "bdip:authorization-denied",
        showDenied,
      );

      window.removeEventListener(
        "bdip:authorization-success",
        showSuccess,
      );

      window.fetch = originalFetch;
    };
  }, []);

  if (!type) {
    return null;
  }

  const isSuccess = type === "success";

  return (
    <div
      className="fixed inset-0 flex items-center justify-center bg-black/30 p-4"
      style={{
        zIndex: 2147483647,
        pointerEvents: "auto",
      }}
      role="presentation"
      onPointerDown={(event) => {
        event.stopPropagation();
      }}
    >
      <div
        className="relative w-full max-w-md rounded-xl bg-white p-6 text-center shadow-2xl"
        style={{
          zIndex: 2147483647,
          pointerEvents: "auto",
        }}
        role="dialog"
        aria-modal="true"
        aria-labelledby="bdip-feedback-title"
        onPointerDown={(event) => {
          event.stopPropagation();
        }}
      >
        <div
          className={`mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full ${
            isSuccess
              ? "bg-green-100 text-green-600"
              : "bg-red-100 text-red-600"
          }`}
        >
          <span className="text-2xl font-bold">
            {isSuccess ? "✓" : "!"}
          </span>
        </div>

        <h2
          id="bdip-feedback-title"
          className="text-lg font-semibold text-gray-900"
        >
          {isSuccess
            ? "Perubahan Berhasil"
            : "Akses Ditolak"}
        </h2>

        <p className="mt-2 text-sm text-gray-600">
          {isSuccess ? (
            "Perubahan berhasil dilakukan."
          ) : (
            <>
              Anda tidak mempunyai hak akses.
              <br />
              
            </>
          )}
        </p>

        <button
          type="button"
          onPointerDown={(event) => {
            event.preventDefault();
            event.stopPropagation();
          }}
          onClick={(event) => {
            event.preventDefault();
            event.stopPropagation();
            setType(null);
          }}
          className="relative mt-6 cursor-pointer rounded-lg bg-gray-900 px-5 py-2.5 text-sm font-medium text-white hover:bg-gray-800"
          style={{
            zIndex: 2147483647,
            pointerEvents: "auto",
          }}
        >
          Tutup
        </button>
      </div>
    </div>
  );
}
