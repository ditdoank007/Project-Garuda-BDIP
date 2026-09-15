"use client";

import {
  Pencil,
  Trash2,
} from "lucide-react";

import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

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
    <div className="min-h-0 flex-1 overflow-auto rounded-xl border bg-white shadow-sm">
      <Table
        containerClassName="overflow-visible"
        className="min-w-[760px]"
      >
        <TableHeader>
          <TableRow>
            <TableHead className="sticky top-0 z-20 bg-slate-50 px-5 py-3 text-left">
              Code
            </TableHead>

            <TableHead className="sticky top-0 z-20 bg-slate-50 px-5 py-3 text-left">
              Name
            </TableHead>

            <TableHead className="sticky top-0 z-20 bg-slate-50 px-5 py-3 text-center">
              Priority
            </TableHead>

            <TableHead className="sticky top-0 z-20 bg-slate-50 px-5 py-3 text-center">
              Status
            </TableHead>

            <TableHead className="sticky top-0 z-20 bg-slate-50 px-5 py-3 text-right">
              Actions
            </TableHead>
          </TableRow>
        </TableHeader>

        <TableBody>
          {policies.map((policy) => (
            <TableRow
              key={policy.id}
              className="border-t"
            >
              <TableCell className="px-5 py-4 font-medium">
                {policy.code}
              </TableCell>

              <TableCell className="px-5 py-4">
                {policy.name}
              </TableCell>

              <TableCell className="px-5 py-4 text-center">
                {policy.priority}
              </TableCell>

              <TableCell className="px-5 py-4 text-center">
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
              </TableCell>

              <TableCell className="px-5 py-4">
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
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
