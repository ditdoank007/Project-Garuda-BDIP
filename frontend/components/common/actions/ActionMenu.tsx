"use client";

import { MoreHorizontal } from "lucide-react";

import { Button } from "@/components/ui/button";

import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

interface ActionMenuProps {
  onView?: () => void;
  onEdit?: () => void;
  onDelete?: () => void;
  onResetPassword?: () => void;
  onEnable?: () => void;
  onDisable?: () => void;
}

export default function ActionMenu({
  onView,
  onEdit,
  onDelete,
  onResetPassword,
  onEnable,
  onDisable,
}: ActionMenuProps) {
  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="icon">
          <MoreHorizontal className="h-5 w-5" />
        </Button>
      </DropdownMenuTrigger>

      <DropdownMenuContent align="end">

        {onView && (
          <DropdownMenuItem onClick={onView}>
            View
          </DropdownMenuItem>
        )}

        {onEdit && (
          <DropdownMenuItem onClick={onEdit}>
            Edit
          </DropdownMenuItem>
        )}

        {onResetPassword && (
          <DropdownMenuItem onClick={onResetPassword}>
            Reset Password
          </DropdownMenuItem>
        )}

        {onEnable && (
          <DropdownMenuItem onClick={onEnable}>
            Enable
          </DropdownMenuItem>
        )}

        {onDisable && (
          <DropdownMenuItem onClick={onDisable}>
            Disable
          </DropdownMenuItem>
        )}

        {onDelete && (
          <DropdownMenuItem
            onClick={onDelete}
            className="text-red-600"
          >
            Delete
          </DropdownMenuItem>
        )}

      </DropdownMenuContent>
    </DropdownMenu>
  );
}