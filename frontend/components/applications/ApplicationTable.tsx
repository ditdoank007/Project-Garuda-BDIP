import type { Application } from "@/services/application.service";

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
    <div className="overflow-x-auto rounded-xl border bg-white shadow-sm">
      <table className="min-w-full">
        <thead className="bg-slate-100">
          <tr>
            <th className="px-4 py-3 text-left">
              Code
            </th>

            <th className="px-4 py-3 text-left">
              Name
            </th>

            <th className="px-4 py-3 text-left">
              Description
            </th>

            <th className="px-4 py-3 text-left">
              Base URL
            </th>

            <th className="px-4 py-3 text-center">
              Status
            </th>

            <th className="px-4 py-3 text-center">
              Actions
            </th>
          </tr>
        </thead>

        <tbody>
          {applications.map((application) => (
            <tr
              key={application.id}
              className="border-t"
            >
              <td className="px-4 py-3 font-medium">
                {application.code}
              </td>

              <td className="px-4 py-3">
                {application.name}
              </td>

              <td className="px-4 py-3">
                {application.description || "-"}
              </td>

              <td className="px-4 py-3">
                <a
                  href={application.baseUrl}
                  target="_blank"
                  rel="noreferrer"
                  className="text-violet-600 hover:text-violet-800"
                >
                  {application.baseUrl}
                </a>
              </td>

              <td className="px-4 py-3 text-center">
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
              </td>

              <td className="px-4 py-3 text-center">
                <div className="flex justify-center gap-2">
                  <button
                    onClick={() =>
                      onEdit(application)
                    }
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
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
