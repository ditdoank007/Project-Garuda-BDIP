"use client";

import { Eye, Pencil, Trash2, Users } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";

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
    <div className="overflow-hidden rounded-lg border">
      <div className="max-h-[calc(100vh-19rem)] overflow-auto">
        <table className="w-full min-w-[1040px] text-sm">
          <thead className="sticky top-0 z-10 bg-muted/80 backdrop-blur">
            <tr className="border-b text-left">
              <th className="px-4 py-3 font-medium">Group</th>
              <th className="px-4 py-3 font-medium">Description</th>
              <th className="px-4 py-3 font-medium">Members</th>
              <th className="px-4 py-3 font-medium">GID</th>
              <th className="px-4 py-3 font-medium">Distinguished Name</th>
              <th className="px-4 py-3 text-right font-medium">Actions</th>
            </tr>
          </thead>

          <tbody>
            {groups.map((group) => {
              const hasMembers = group.memberCount > 0;

              return (
                <tr
                  key={group.name}
                  className="border-b last:border-b-0 hover:bg-muted/40"
                >
                  <td className="px-4 py-3 align-top">
                    <p className="font-medium">{group.name}</p>
                    <p className="mt-1 text-xs text-muted-foreground">
                      Created {group.createdAt || "-"}
                    </p>
                  </td>

                  <td className="max-w-[260px] px-4 py-3 align-top text-muted-foreground">
                    <span className="line-clamp-2">
                      {group.description || "-"}
                    </span>
                  </td>

                  <td className="px-4 py-3 align-top">
                    <Badge
                      variant={hasMembers ? "default" : "secondary"}
                      className="gap-1"
                    >
                      <Users className="h-3.5 w-3.5" />
                      {group.memberCount}
                    </Badge>
                  </td>

                  <td className="px-4 py-3 align-top font-mono text-xs">
                    {group.gidNumber > 0 ? (
                  group.gidNumber
                ) : (
                  <span className="text-muted-foreground">Not assigned</span>
                )}
                  </td>

                  <td
                    className="max-w-[300px] px-4 py-3 align-top font-mono text-xs text-muted-foreground"
                    title={group.distinguishedName}
                  >
                    {shortDn(group.distinguishedName)}
                  </td>

                  <td className="px-4 py-3 align-top">
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
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
}
