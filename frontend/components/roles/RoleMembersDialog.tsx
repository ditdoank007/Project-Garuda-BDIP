"use client";

import { useEffect, useMemo, useState } from "react";

import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";

import {
  addRoleMember,
  removeRoleMember,
} from "@/services/role.service";

import type {
  RoleMember,
  RoleMembersResponse,
} from "@/types/role";

interface UserOption {
  username: string;
  fullName: string;
  enabled: boolean;
}

interface RoleMembersDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  data: RoleMembersResponse | null;
  users: UserOption[];
  loading?: boolean;
  error?: string;
  onChanged: () => Promise<void>;
}

export default function RoleMembersDialog({
  open,
  onOpenChange,
  data,
  users,
  loading = false,
  error = "",
  onChanged,
}: RoleMembersDialogProps) {
  const [selectedUsername, setSelectedUsername] = useState("");
  const [saving, setSaving] = useState(false);
  const [actionError, setActionError] = useState("");

  const members: RoleMember[] = data?.members ?? [];
  const roleName = data?.roleName ?? "";

  useEffect(() => {
    if (!open) {
      setSelectedUsername("");
      setActionError("");
    }
  }, [open]);

  const memberUsernames = useMemo(
    () => new Set(members.map((member) => member.username)),
    [members],
  );

  const availableUsers = useMemo(
    () =>
      users.filter(
        (user) =>
          user.enabled &&
          !memberUsernames.has(user.username),
      ),
    [users, memberUsernames],
  );

  async function handleAddMember() {
    if (!roleName || !selectedUsername) {
      return;
    }

    try {
      setSaving(true);
      setActionError("");

      await addRoleMember(roleName, selectedUsername);

      setSelectedUsername("");
      await onChanged();
    } catch (err) {
      setActionError(
        err instanceof Error
          ? err.message
          : "Failed to add member.",
      );
    } finally {
      setSaving(false);
    }
  }

  async function handleRemoveMember(username: string) {
    if (!roleName) {
      return;
    }

    try {
      setSaving(true);
      setActionError("");

      await removeRoleMember(roleName, username);

      await onChanged();
    } catch (err) {
      setActionError(
        err instanceof Error
          ? err.message
          : "Failed to remove member.",
      );
    } finally {
      setSaving(false);
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent
      className="flex h-[82vh] max-h-[760px] !w-[min(92vw,1180px)] !max-w-[1180px] flex-col overflow-hidden p-0">
        <DialogHeader className="border-b px-6 py-5">
          <DialogTitle className="pr-8 text-lg">
            Members — {roleName || "Role"}
          </DialogTitle>
        </DialogHeader>

        <div className="flex min-h-0 flex-1 flex-col gap-5 overflow-hidden px-6 py-5">
          {error && (
            <div className="rounded-md border border-destructive/30 bg-destructive/10 px-4 py-3 text-sm text-destructive">
              {error}
            </div>
          )}

          {actionError && (
            <div className="rounded-md border border-destructive/30 bg-destructive/10 px-4 py-3 text-sm text-destructive">
              {actionError}
            </div>
          )}

          <section className="rounded-lg border bg-muted/20 p-4">
            <Label
              htmlFor="role-member-user"
              className="mb-3 block text-sm font-medium"
            >
              Add user to this role
            </Label>

            <div className="flex flex-col gap-3 sm:flex-row">
              <select
                id="role-member-user"
                value={selectedUsername}
                disabled={saving || loading || !roleName}
                onChange={(event) =>
                  setSelectedUsername(event.target.value)
                }
                className="h-10 w-full rounded-md border bg-background px-3 text-sm outline-none ring-offset-background focus:ring-2 focus:ring-ring focus:ring-offset-2"
              >
                <option value="">Select a user...</option>

                {availableUsers.map((user) => (
                  <option
                    key={user.username}
                    value={user.username}
                  >
                    {user.fullName
                      ? `${user.fullName} (${user.username})`
                      : user.username}
                  </option>
                ))}
              </select>

              <Button
                type="button"
                className="shrink-0"
                disabled={
                  !selectedUsername ||
                  saving ||
                  loading ||
                  !roleName
                }
                onClick={handleAddMember}
              >
                {saving ? "Saving..." : "Add Member"}
              </Button>
            </div>

            {!loading &&
              roleName &&
              availableUsers.length === 0 && (
                <p className="mt-3 text-sm text-muted-foreground">
                  All active users are already members of this role.
                </p>
              )}
          </section>

          <div className="flex items-center justify-between">
            <p className="text-sm text-muted-foreground">
              Total active members:{" "}
              <span className="font-semibold text-foreground">
                {members.length}
              </span>
            </p>
          </div>

          <section className="min-h-0 flex-1 rounded-lg border">
            <div className="h-full min-h-0 overflow-y-auto overflow-x-auto">
              <table className="w-full min-w-[680px] text-sm">
                <thead className="sticky top-0 z-10 bg-muted/95 backdrop-blur">
                  <tr className="border-b">
                    <th className="px-4 py-3 text-left font-medium">
                      Username
                    </th>
                    <th className="px-4 py-3 text-left font-medium">
                      Full Name
                    </th>
                    <th className="px-4 py-3 text-left font-medium">
                      Email
                    </th>
                    <th className="w-[120px] px-4 py-3 text-center font-medium">
                      Action
                    </th>
                  </tr>
                </thead>

                <tbody>
                  {loading ? (
                    <tr>
                      <td
                        colSpan={4}
                        className="px-4 py-10 text-center text-muted-foreground"
                      >
                        Loading members...
                      </td>
                    </tr>
                  ) : !roleName ? (
                    <tr>
                      <td
                        colSpan={4}
                        className="px-4 py-10 text-center text-muted-foreground"
                      >
                        Select a role to view its members.
                      </td>
                    </tr>
                  ) : members.length === 0 ? (
                    <tr>
                      <td
                        colSpan={4}
                        className="px-4 py-10 text-center text-muted-foreground"
                      >
                        This role has no members yet.
                      </td>
                    </tr>
                  ) : (
                    members.map((member) => (
                      <tr
                        key={member.username}
                        className="border-b last:border-b-0"
                      >
                        <td className="whitespace-nowrap px-4 py-3 font-mono text-xs">
                          {member.username}
                        </td>

                        <td className="px-4 py-3">
                          {member.fullName || "-"}
                        </td>

                        <td className="px-4 py-3 text-muted-foreground">
                          {member.email || "-"}
                        </td>

                        <td className="px-4 py-3 text-center">
                          <Button
                            type="button"
                            variant="destructive"
                            size="sm"
                            disabled={saving}
                            onClick={() =>
                              handleRemoveMember(
                                member.username,
                              )
                            }
                          >
                            Remove
                          </Button>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          </section>
        </div>

        <DialogFooter className="border-t px-6 py-4">
          <Button
            type="button"
            variant="outline"
            onClick={() => onOpenChange(false)}
          >
            Close
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
