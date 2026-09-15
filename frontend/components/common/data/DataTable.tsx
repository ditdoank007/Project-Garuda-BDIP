import {
  Table,
  TableBody,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

import { ReactNode } from "react";

interface DataTableProps {
  headers: ReactNode;
  children: ReactNode;
}

export default function DataTable({
  headers,
  children,
}: DataTableProps) {
  return (
    <div className="h-full min-h-0 overflow-auto rounded-md border">

      <Table
        containerClassName="overflow-visible"
        className="min-w-[1400px]"
      >

        <TableHeader>
          <TableRow>
            {headers}
          </TableRow>
        </TableHeader>

        <TableBody>
          {children}
        </TableBody>

      </Table>

    </div>
  );
}