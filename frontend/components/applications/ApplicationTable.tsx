import type { Application } from "@/services/application.service";

import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

interface Props {
  applications: Application[];
  onEdit: (application: Application) => void;
  onDeactivate: (application: Application) => void;
}

export default function ApplicationTable({
  applications,
  onEdit,
  onDeactivate,
}: Props) {
  return (
    <div className="min-h-0 flex-1 overflow-auto rounded-xl border bg-white shadow-sm">
      <Table
        containerClassName="overflow-visible"
        className="min-w-[1100px]"
      >
        <TableHeader>
          <TableRow>
            <TableHead className="sticky top-0 z-20 bg-slate-100 px-4 py-3 text-left">
              Code
            </TableHead>

            <TableHead className="sticky top-0 z-20 bg-slate-100 px-4 py-3 text-left">
              Name
            </TableHead>

            <TableHead className="sticky top-0 z-20 bg-slate-100 px-4 py-3 text-left">
              Description
            </TableHead>

            <TableHead className="sticky top-0 z-20 bg-slate-100 px-4 py-3 text-left">
              Base URL
            </TableHead>

            <TableHead className="sticky top-0 z-20 bg-slate-100 px-4 py-3 text-center">
              Status
            </TableHead>

            <TableHead className="sticky top-0 z-20 bg-slate-100 px-4 py-3 text-center">
              Actions
            </TableHead>
          </TableRow>
        </TableHeader>

        <TableBody>
          {applications.map((application) => (
            <TableRow
              key={application.id}
              className="border-t"
            >
              <TableCell className="px-4 py-3 font-medium">
                {application.code}
              </TableCell>

              <TableCell className="px-4 py-3">
                {application.name}
              </TableCell>

              <TableCell className="px-4 py-3">
                {application.description || "-"}
              </TableCell>

              <TableCell className="px-4 py-3">
                <a
                  href={application.baseUrl}
                  target="_blank"
                  rel="noreferrer"
                  className="text-violet-600 hover:text-violet-800"
                >
                  {application.baseUrl}
                </a>
              </TableCell>

              <TableCell className="px-4 py-3 text-center">
                <span
                  className={
                    application.isActive
                      ? "rounded-full bg-emerald-100 px-2.5 py-1 text-xs font-medium text-emerald-700"
                      : "rounded-full bg-slate-100 px-2.5 py-1 text-xs font-medium text-slate-600"
                  }
                >
                  {application.isActive
                    ? "ACTIVE"
                    : "INACTIVE"}
                </span>
              </TableCell>

              <TableCell className="px-4 py-3 text-center">
                <div className="flex justify-center gap-2">
                  <button
                    onClick={() => onEdit(application)}
                    className="rounded-md border px-3 py-1 text-sm hover:bg-slate-100"
                  >
                    Edit
                  </button>

                  {application.isActive && (
                    <button
                      onClick={() =>
                        onDeactivate(application)
                      }
                      className="rounded-md border border-red-200 px-3 py-1 text-sm text-red-600 hover:bg-red-50"
                    >
                      Deactivate
                    </button>
                  )}
                </div>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
