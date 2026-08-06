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
    <div className="rounded-md border">

      <Table>

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