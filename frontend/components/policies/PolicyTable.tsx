"use client";

import {
  Pencil,
  Trash2,
} from "lucide-react";

import type { Policy } from "@/types/policy";

interface PolicyTableProps {
  policies: Policy[];
  onEdit: (policy: Policy) => void;
  onDelete: (policy: Policy) => void;
}

export default function PolicyTable({
  policies,
  onEdit,
  onDelete,
}: PolicyTableProps) {
  return (
    <div className="overflow-hidden rounded-xl border bg-white shadow-sm">

      <table className="min-w-full">

        <thead className="bg-slate-50">

          <tr>

            <th className="px-5 py-3 text-left">
              Code
            </th>

            <th className="px-5 py-3 text-left">
              Name
            </th>

            <th className="px-5 py-3 text-center">
              Priority
            </th>

            <th className="px-5 py-3 text-center">
              Status
            </th>

            <th className="px-5 py-3 text-right">
              Actions
            </th>

          </tr>

        </thead>

        <tbody>

          {policies.map((policy) => (

            <tr
              key={policy.id}
              className="border-t"
            >

              <td className="px-5 py-4 font-medium">
                {policy.code}
              </td>

              <td className="px-5 py-4">
                {policy.name}
              </td>

              <td className="px-5 py-4 text-center">
                {policy.priority}
              </td>

              <td className="px-5 py-4 text-center">

                <span
                  className={
                    policy.enabled
                      ? "rounded bg-green-100 px-3 py-1 text-sm text-green-700"
                      : "rounded bg-red-100 px-3 py-1 text-sm text-red-700"
                  }
                >
                  {policy.enabled
                    ? "Enabled"
                    : "Disabled"}
                </span>

              </td>

              <td className="px-5 py-4">

                <div className="flex justify-end gap-2">

                  <button
                    onClick={() => onEdit(policy)}
                    className="rounded border p-2 hover:bg-slate-100"
                  >
                    <Pencil size={16} />
                  </button>

                  <button
                    onClick={() => onDelete(policy)}
                    className="rounded border p-2 hover:bg-slate-100"
                  >
                    <Trash2 size={16} />
                  </button>

                </div>

              </td>

            </tr>

          ))}

        </tbody>

      </table>

    </div>
  );
}
