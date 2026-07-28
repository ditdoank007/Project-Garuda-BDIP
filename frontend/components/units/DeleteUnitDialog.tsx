"use client";

import { AlertTriangle, Loader2, X } from "lucide-react";

import type { Unit } from "@/types/unit";

type DeleteUnitDialogProps = {
  unit: Unit | null;
  loading?: boolean;
  onClose: () => void;
  onConfirm: () => Promise<void>;
};

export default function DeleteUnitDialog({
  unit,
  loading = false,
  onClose,
  onConfirm,
}: DeleteUnitDialogProps) {
  if (!unit) {
    return null;
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/50 p-4 backdrop-blur-sm">
      <div className="w-full max-w-md overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-2xl">
        <div className="flex items-center justify-between border-b border-slate-200 px-6 py-5">
          <h2 className="text-lg font-semibold text-slate-900">
            Delete Unit
          </h2>

          <button
            type="button"
            onClick={onClose}
            disabled={loading}
            className="rounded-lg p-2 text-slate-500 transition hover:bg-slate-100"
          >
            <X size={20} />
          </button>
        </div>

        <div className="p-6">
          <div className="flex h-12 w-12 items-center justify-center rounded-full bg-red-100 text-red-600">
            <AlertTriangle size={24} />
          </div>

          <h3 className="mt-5 text-lg font-semibold text-slate-900">
            Delete {unit.name}?
          </h3>

          <p className="mt-2 text-sm leading-6 text-slate-600">
            This organizational unit will be removed
            from LDAP. Units assigned to users cannot
            be deleted.
          </p>

          <div className="mt-6 flex justify-end gap-3">
            <button
              type="button"
              onClick={onClose}
              disabled={loading}
              className="rounded-xl border border-slate-300 px-5 py-2.5 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
            >
              Cancel
            </button>

            <button
              type="button"
              onClick={onConfirm}
              disabled={loading}
              className="inline-flex items-center gap-2 rounded-xl bg-red-600 px-5 py-2.5 text-sm font-medium text-white transition hover:bg-red-700 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {loading && (
                <Loader2
                  size={17}
                  className="animate-spin"
                />
              )}

              Delete Unit
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
