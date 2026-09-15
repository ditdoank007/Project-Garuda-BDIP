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
} from "@/services/users.service"

import type { User } from "@/types/users";
import type { Policy } from "@/types/policy";
import UserPolicySelector from "./UserPolicySelector";

import UserViewDialog from "./UserViewDialog";
import ResetPasswordDialog from "./ResetPasswordDialog";

interface UserTableProps {
  users: User[];
  policies: Policy[];
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
    policies,
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
      <div className="h-full min-h-0 overflow-auto rounded-md border">
        <Table containerClassName="overflow-visible">
          <TableHeader>
            <TableRow>
              <TableHead className="sticky top-0 z-20 bg-white">Username</TableHead>
              <TableHead className="sticky top-0 z-20 bg-white">NIP</TableHead>
              <TableHead className="sticky top-0 z-20 bg-white">FingerID</TableHead>
              <TableHead className="sticky top-0 z-20 bg-white">Full Name</TableHead>
              <TableHead className="sticky top-0 z-20 bg-white">Email</TableHead>
              <TableHead className="sticky top-0 z-20 bg-white">Unit</TableHead>
              <TableHead className="sticky top-0 z-20 bg-white">NAP Policy</TableHead>
              <TableHead className="sticky top-0 z-20 bg-white">Status</TableHead>
              <TableHead className="sticky top-0 z-20 w-20 bg-white text-right">
                Actions
              </TableHead>
            </TableRow>
          </TableHeader>

          <TableBody>
            {users.length === 0 ? (
              <TableRow>
                <TableCell
                  colSpan={9}
                  className="h-24 text-center text-muted-foreground"
                >
                  No users found.
                </TableCell>
              </TableRow>
            ) : (
              users.map((user) => {
                const isUpdating =
                  updatingUsername === user.username;

                const dataClass = (value?: string | null) =>
                  value?.trim()
                    ? "bdip-data-complete"
                    : "bdip-data-incomplete";

                const policyClass = user.policyId
                  ? "bdip-data-complete"
                  : "bdip-data-incomplete";

                return (
                  <TableRow key={user.uid}>
                    <TableCell className="font-medium text-white bdip-data-complete">
                      {user.username}
                    </TableCell>

                    <TableCell className={`text-white ${dataClass(user.nip)}`}>
                      {user.nip || "-"}
                    </TableCell>

                    <TableCell className={`text-white ${dataClass(user.fingerId)}`}>
                      {user.fingerId || "-"}
                    </TableCell>

                    <TableCell className={`text-white ${dataClass(user.fullName)}`}>
                      {user.fullName || "-"}
                    </TableCell>

                    <TableCell className={`text-white ${dataClass(user.email)}`}>
                      {user.email || "-"}
                    </TableCell>

                    <TableCell className={`text-white ${dataClass(user.unit)}`}>
                      {user.unit || "-"}
                    </TableCell>

                    <TableCell
                      className={`min-w-[260px] text-white ${policyClass}`}
                    >
                      <UserPolicySelector
                        user={user}
                        policies={policies}
                        initialPolicyId={user.policyId}
                      />
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
