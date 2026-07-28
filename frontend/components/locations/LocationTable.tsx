import type { Location } from "@/types/location";

interface Props {
  locations: Location[];
  onEdit: (
    location: Location,
  ) => void;
}

export default function LocationTable({
  locations,
  onEdit,
}: Props) {
  return (
    <div className="overflow-hidden rounded-xl border bg-white shadow-sm">

      <table className="min-w-full">

        <thead className="bg-slate-100">

          <tr>

            <th className="px-4 py-3 text-left">
              Name
            </th>

            <th className="px-4 py-3 text-left">
              Type
            </th>

            <th className="px-4 py-3 text-left">
              Description
            </th>

            <th className="px-4 py-3 text-center">
              Units
            </th>

            <th className="px-4 py-3 text-center">
              Actions
            </th>

          </tr>

        </thead>

        <tbody>

          {locations.map((location) => (

            <tr
              key={location.name}
              className="border-t"
            >

              <td className="px-4 py-3">
                {location.name}
              </td>

              <td className="px-4 py-3">
                {location.type || "-"}
              </td>

              <td className="px-4 py-3">
                {location.description || "-"}
              </td>

              <td className="px-4 py-3 text-center">
                {location.unitCount}
              </td>

              <td className="px-4 py-3 text-center">

                <button
                  onClick={() =>
                    onEdit(location)
                  }
                  className="rounded-md border px-3 py-1 text-sm hover:bg-slate-100"
                >
                  Edit
                </button>

              </td>

            </tr>

          ))}

        </tbody>

      </table>

    </div>
  );
}
