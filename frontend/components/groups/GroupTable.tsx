"use client";

import { Eye, Pencil, Trash2, Users } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

import type { Group } from "@/types/groups";

interface GroupTableProps {
  groups: Group[];
  onView: (group: Group) => void;
  onMembers: (group: Group) => void;
  onEdit: (group: Group) => void;
  onDelete: (group: Group) => void;
}

function shortDn(distinguishedName: string) {
  const parts = distinguishedName.split(",");

  if (parts.length <= 2) {
    return distinguishedName;
  }

  return `${parts.slice(0, 2).join(",")}, …`;
}

export default function GroupTable({
  groups,
  onView,
  onMembers,
  onEdit,
  onDelete,
}: GroupTableProps) {
  if (groups.length === 0) {
    return (
      <div className="rounded-lg border border-dashed p-10 text-center">
        <p className="font-medium">No groups found</p>
        <p className="mt-1 text-sm text-muted-foreground">
          Create a group or change the search keyword.
        </p>
      </div>
    );
  }

  return (
    <div className="min-h-0 flex-1 overflow-auto rounded-lg border">
      <Table containerClassName="overflow-visible">
        <TableHeader>
          <TableRow>
            <TableHead className="sticky top-0 z-20 bg-white">
              Group
            </TableHead>
            <TableHead className="sticky top-0 z-20 bg-white">
              Description
            </TableHead>
            <TableHead className="sticky top-0 z-20 bg-white">
              Members
            </TableHead>
            <TableHead className="sticky top-0 z-20 bg-white">
              GID
            </TableHead>
            <TableHead className="sticky top-0 z-20 bg-white">
              Distinguished Name
            </TableHead>
            <TableHead className="sticky top-0 z-20 bg-white text-right">
              Actions
            </TableHead>
          </TableRow>
        </TableHeader>

        <TableBody>
            {groups.map((group) => {
              const hasMembers = group.memberCount > 0;

              return (
                <TableRow key={group.name}>
                  <TableCell className="align-top text-white">
                    <p className="font-medium text-white">{group.name}</p>
                    <p className="mt-1 text-xs text-slate-400">
                      Created {group.createdAt || "-"}
                    </p>
                  </TableCell>

                  <TableCell className="max-w-[260px] align-top text-slate-200">
                    <span className="line-clamp-2">
                      {group.description || "-"}
                    </span>
                  </TableCell>

                  <TableCell className="align-top">
                    <Badge
                      variant={hasMembers ? "default" : "secondary"}
                      className="gap-1"
                    >
                      <Users className="h-3.5 w-3.5" />
                      {group.memberCount}
                    </Badge>
                  </TableCell>

                  <TableCell className="align-top font-mono text-xs text-white">
                    {group.gidNumber > 0 ? (
                  group.gidNumber
                ) : (
                  <span className="text-slate-400">Not assigned</span>
                )}
                  </TableCell>

                  <TableCell
                    className="max-w-[300px] align-top font-mono text-xs text-slate-300"
                    title={group.distinguishedName}
                  >
                    {shortDn(group.distinguishedName)}
                  </TableCell>

                  <TableCell className="align-top">
                    <div className="flex justify-end gap-2">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => onView(group)}
                        title="View group details"
                      >
                        <Eye className="h-4 w-4" />
                        <span className="ml-2">View</span>
                      </Button>

                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => onMembers(group)}
                        title="Manage group members"
                      >
                        <Users className="h-4 w-4" />
                        <span className="ml-2">
                          Members ({group.memberCount})
                        </span>
                      </Button>

                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => onEdit(group)}
                        title="Edit group"
                      >
                        <Pencil className="h-4 w-4" />
                        <span className="ml-2">Edit</span>
                      </Button>

                      <Button
                        variant="destructive"
                        size="sm"
                        onClick={() => {
                          const confirmed = window.confirm(
                            `Delete group "${group.name}"? This action cannot be undone.`,
                          );

                          if (confirmed) {
                            onDelete(group);
                          }
                        }}
                        title="Delete group"
                      >
                        <Trash2 className="h-4 w-4" />
                        <span className="ml-2">Delete</span>
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
    </div>
  );
}
