"use client";

import { useEffect, useState } from "react";
import { Loader2, X } from "lucide-react";

import type { Unit } from "@/types/unit";

type UnitFormDialogProps = {
  open: boolean;
  unit?: Unit | null;
  loading?: boolean;
  onClose: () => void;
  onSubmit: (data: {
    name: string;
    description: string;
  }) => Promise<void>;
};

export default function UnitFormDialog({
  open,
  unit,
  loading = false,
  onClose,
  onSubmit,
}: UnitFormDialogProps) {
  const [name, setName] = useState("");
  const [description, setDescription] =
    useState("");

  useEffect(() => {
    if (!open) {
      return;
    }

    setName(unit?.name ?? "");
    setDescription(unit?.description ?? "");
  }, [open, unit]);

  if (!open) {
    return null;
  }

  async function handleSubmit(
    event: React.FormEvent<HTMLFormElement>
  ) {
    event.preventDefault();

    await onSubmit({
      name: name.trim(),
      description: description.trim(),
    });
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/50 p-4 backdrop-blur-sm">
      <div className="w-full max-w-lg overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-2xl">
        <div className="flex items-center justify-between border-b border-slate-200 px-6 py-5">
          <div>
            <h2 className="text-xl font-semibold text-slate-900">
              {unit ? "Edit Unit" : "Add Unit"}
            </h2>

            <p className="mt-1 text-sm text-slate-500">
              {unit
                ? "Update organizational unit information."
                : "Create a new organizational unit in LDAP."}
            </p>
          </div>

          <button
            type="button"
            onClick={onClose}
            disabled={loading}
            className="rounded-lg p-2 text-slate-500 transition hover:bg-slate-100 hover:text-slate-900 disabled:cursor-not-allowed"
          >
            <X size={20} />
          </button>
        </div>

        <form
          onSubmit={handleSubmit}
          className="space-y-5 p-6"
        >
          <div className="space-y-2">
            <label
              htmlFor="unit-name"
              className="text-sm font-medium text-slate-700"
            >
              Unit Name
            </label>

            <input
              id="unit-name"
              type="text"
              value={name}
              onChange={(event) =>
                setName(event.target.value)
              }
              placeholder="Example: Seksi Operasi"
              required
              disabled={loading}
              className="w-full rounded-xl border border-slate-300 px-4 py-3 text-sm text-slate-900 outline-none transition placeholder:text-slate-400 focus:border-blue-500 focus:ring-4 focus:ring-blue-100 disabled:bg-slate-100"
            />
          </div>

          <div className="space-y-2">
            <label
              htmlFor="unit-description"
              className="text-sm font-medium text-slate-700"
            >
              Description
            </label>

            <textarea
              id="unit-description"
              value={description}
              onChange={(event) =>
                setDescription(event.target.value)
              }
              placeholder="Description of the organizational unit"
              rows={4}
              disabled={loading}
              className="w-full resize-none rounded-xl border border-slate-300 px-4 py-3 text-sm text-slate-900 outline-none transition placeholder:text-slate-400 focus:border-blue-500 focus:ring-4 focus:ring-blue-100 disabled:bg-slate-100"
            />
          </div>

          <div className="flex justify-end gap-3 border-t border-slate-100 pt-5">
            <button
              type="button"
              onClick={onClose}
              disabled={loading}
              className="rounded-xl border border-slate-300 px-5 py-2.5 text-sm font-medium text-slate-700 transition hover:bg-slate-50 disabled:cursor-not-allowed"
            >
              Cancel
            </button>

            <button
              type="submit"
              disabled={
                loading || name.trim().length === 0
              }
              className="inline-flex items-center gap-2 rounded-xl bg-blue-600 px-5 py-2.5 text-sm font-medium text-white transition hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {loading && (
                <Loader2
                  size={17}
                  className="animate-spin"
                />
              )}

              {unit ? "Save Changes" : "Create Unit"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
