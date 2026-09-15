"use client";

import { useEffect, useState } from "react";

type NotificationType = "denied" | "success";

export default function AuthorizationDeniedDialog() {
  const [type, setType] = useState<NotificationType | null>(null);

  useEffect(() => {
    const handleDenied = () => {
      setType("denied");
    };

    const handleSuccess = () => {
      setType("success");
    };

    window.addEventListener(
      "bdip:authorization-denied",
      handleDenied,
    );

    window.addEventListener(
      "bdip:authorization-success",
      handleSuccess,
    );

    return () => {
      window.removeEventListener(
        "bdip:authorization-denied",
        handleDenied,
      );

      window.removeEventListener(
        "bdip:authorization-success",
        handleSuccess,
      );
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
      onPointerDownCapture={(event) => {
        event.stopPropagation();
      }}
      onClickCapture={(event) => {
        event.stopPropagation();
      }}
    >
      <div
        className="relative w-full max-w-md rounded-xl bg-white p-6 text-center shadow-2xl"
        style={{
          zIndex: 2147483647,
          pointerEvents: "auto",
        }}
        onPointerDownCapture={(event) => {
          event.stopPropagation();
        }}
        onClickCapture={(event) => {
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

        <h2 className="text-lg font-semibold text-gray-900">
          {isSuccess ? "Perubahan Berhasil" : "Akses Ditolak"}
        </h2>

        <p className="mt-2 text-sm text-gray-600">
          {isSuccess ? (
            "Perubahan password berhasil dilakukan."
          ) : (
            <>
              Anda tidak mempunyai akses untuk mengubah.
              <br />
              Hubungi Administrator.
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
