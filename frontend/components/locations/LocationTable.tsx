import type { Location } from "@/types/location";

import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

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
    <div className="min-h-0 flex-1 overflow-auto rounded-xl border bg-white shadow-sm">
      <Table
        containerClassName="overflow-visible"
        className="min-w-[900px]"
      >
        <TableHeader>
          <TableRow>
            <TableHead className="sticky top-0 z-20 bg-slate-100 px-4 py-3 text-left">
              Name
            </TableHead>

            <TableHead className="sticky top-0 z-20 bg-slate-100 px-4 py-3 text-left">
              Type
            </TableHead>

            <TableHead className="sticky top-0 z-20 bg-slate-100 px-4 py-3 text-left">
              Description
            </TableHead>

            <TableHead className="sticky top-0 z-20 bg-slate-100 px-4 py-3 text-center">
              Units
            </TableHead>

            <TableHead className="sticky top-0 z-20 bg-slate-100 px-4 py-3 text-center">
              Actions
            </TableHead>
          </TableRow>
        </TableHeader>

        <TableBody>
          {locations.map((location) => (
            <TableRow
              key={location.name}
              className="border-t"
            >
              <TableCell className="px-4 py-3">
                {location.name}
              </TableCell>

              <TableCell className="px-4 py-3">
                {location.type || "-"}
              </TableCell>

              <TableCell className="px-4 py-3">
                {location.description || "-"}
              </TableCell>

              <TableCell className="px-4 py-3 text-center">
                {location.unitCount}
              </TableCell>

              <TableCell className="px-4 py-3 text-center">
                <button
                  onClick={() => onEdit(location)}
                  className="rounded-md border px-3 py-1 text-sm hover:bg-slate-100"
                >
                  Edit
                </button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
