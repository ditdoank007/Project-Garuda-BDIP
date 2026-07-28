"use client";

import { useState } from "react";
import { toast } from "sonner";

import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

import {
  ActionMenu,
  StatusBadge,
} from "@/components/common";

import {
  deleteUser,
  updateUserStatus,
} from "@/lib/api/users";

import type { User } from "@/types/users";

import UserViewDialog from "./UserViewDialog";
import ResetPasswordDialog from "./ResetPasswordDialog";

interface UserTableProps {
  users: User[];
  onEdit: (user: User) => void;
}

function getErrorMessage(
  error: unknown,
  fallback: string,
): string {
  if (
    typeof error === "object" &&
    error !== null &&
    "response" in error
  ) {
    const response = (
      error as {
        response?: {
          data?: {
            message?: string;
            title?: string;
          };
        };
      }
    ).response;

    return (
      response?.data?.message ??
      response?.data?.title ??
      fallback
    );
  }

  return fallback;
}

export default function UserTable({
  users,
  onEdit,
}: UserTableProps) {
  const [selectedUser, setSelectedUser] =
    useState<User | null>(null);

  const [viewOpen, setViewOpen] =
    useState(false);

  const [resetPasswordOpen, setResetPasswordOpen] =
    useState(false);

  const [updatingUsername, setUpdatingUsername] =
    useState<string | null>(null);

  function handleView(user: User) {
    setSelectedUser(user);
    setViewOpen(true);
  }

  function handleResetPassword(user: User) {
    setSelectedUser(user);
    setResetPasswordOpen(true);
  }

  async function handleStatusChange(
    user: User,
    enabled: boolean,
  ) {
    try {
      setUpdatingUsername(user.username);

      await updateUserStatus(user.username, enabled);

      toast.success(
        enabled
          ? `User "${user.username}" enabled successfully.`
          : `User "${user.username}" disabled successfully.`,
      );

      window.location.reload();
    } catch (error) {
      console.error("Update user status failed:", error);

      toast.error(
        getErrorMessage(
          error,
          "Failed to update user status.",
        ),
      );
    } finally {
      setUpdatingUsername(null);
    }
  }

  async function handleDelete(user: User) {
    try {
      setUpdatingUsername(user.username);

      await deleteUser(user.username);

      toast.success(
        `User "${user.username}" deleted successfully.`,
      );

      window.location.reload();
    } catch (error) {
      console.error("Delete user failed:", error);

      toast.error(
        getErrorMessage(
          error,
          "Failed to delete user.",
        ),
      );
    } finally {
      setUpdatingUsername(null);
    }
  }

  return (
    <>
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Username</TableHead>
              <TableHead>Full Name</TableHead>
              <TableHead>Email</TableHead>
              <TableHead>Unit</TableHead>
              <TableHead>Status</TableHead>
              <TableHead className="w-20 text-right">
                Actions
              </TableHead>
            </TableRow>
          </TableHeader>

          <TableBody>
            {users.length === 0 ? (
              <TableRow>
                <TableCell
                  colSpan={6}
                  className="h-24 text-center text-muted-foreground"
                >
                  No users found.
                </TableCell>
              </TableRow>
            ) : (
              users.map((user) => {
                const isUpdating =
                  updatingUsername === user.username;

                return (
                  <TableRow key={user.uid}>
                    <TableCell className="font-medium">
                      {user.username}
                    </TableCell>

                    <TableCell>
                      {user.fullName || "-"}
                    </TableCell>

                    <TableCell>
                      {user.email || "-"}
                    </TableCell>

                    <TableCell>
                      {user.unit || "-"}
                    </TableCell>

                    <TableCell>
                      <StatusBadge active={user.enabled} />
                    </TableCell>

                    <TableCell className="text-right">
                      <ActionMenu
                        onView={() => handleView(user)}
                        onEdit={() => onEdit(user)}
                        onResetPassword={() =>
                          handleResetPassword(user)
                        }
                        onEnable={
                          !isUpdating && !user.enabled
                            ? () =>
                                handleStatusChange(
                                  user,
                                  true,
                                )
                            : undefined
                        }
                        onDisable={
                          !isUpdating && user.enabled
                            ? () =>
                                handleStatusChange(
                                  user,
                                  false,
                                )
                            : undefined
                        }
                        onDelete={
                          !isUpdating
                            ? () => handleDelete(user)
                            : undefined
                        }
                      />
                    </TableCell>
                  </TableRow>
                );
              })
            )}
          </TableBody>
        </Table>
      </div>

      <UserViewDialog
        open={viewOpen}
        onOpenChange={setViewOpen}
        user={selectedUser}
      />

      <ResetPasswordDialog
        open={resetPasswordOpen}
        onOpenChange={setResetPasswordOpen}
        user={selectedUser}
      />
    </>
  );
}
