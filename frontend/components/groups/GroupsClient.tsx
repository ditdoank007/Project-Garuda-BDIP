"use client";

import { useMemo, useState } from "react";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

import GroupDialog from "./GroupDialog";
import GroupMembersDialog from "./GroupMembersDialog";
import GroupTable from "./GroupTable";

import {
  createGroup,
  deleteGroup,
  getGroupMembers,
  getGroups,
  updateGroup,
} from "@/lib/api/groups";

import type {
  Group,
  GroupFormData,
  GroupMembersResponse,
} from "@/types/groups";
import { defaultGroupForm } from "@/types/groups";

import type { User } from "@/types/users";

interface GroupsClientProps {
  initialGroups: Group[];
  users: User[];
}

type DialogMode = "create" | "view" | "edit";

function getErrorMessage(
  error: unknown,
  fallback: string,
): string {
  if (
    typeof error === "object" &&
    error !== null &&
    "response" in error
  ) {
    const response = error as {
      response?: {
        data?: {
          message?: string;
          title?: string;
        };
      };
    };

    return (
      response.response?.data?.message ||
      response.response?.data?.title ||
      fallback
    );
  }

  return fallback;
}

export default function GroupsClient({
  initialGroups,
  users,
}: GroupsClientProps) {
  const [groups, setGroups] =
    useState<Group[]>(initialGroups);

  const [search, setSearch] = useState("");

  const [dialogOpen, setDialogOpen] =
    useState(false);

  const [dialogMode, setDialogMode] =
    useState<DialogMode>("create");

  const [selectedGroup, setSelectedGroup] =
    useState<Group | null>(null);

  const [form, setForm] =
    useState<GroupFormData>(defaultGroupForm);

  const [saving, setSaving] = useState(false);

  const [membersOpen, setMembersOpen] =
    useState(false);

  const [membersLoading, setMembersLoading] =
    useState(false);

  const [membersError, setMembersError] =
    useState("");

  const [membersData, setMembersData] =
    useState<GroupMembersResponse | null>(null);

  const filteredGroups = useMemo(() => {
    const keyword = search.trim().toLowerCase();

    if (!keyword) return groups;

    return groups.filter((group) =>
      [
        group.name,
        group.description,
        group.distinguishedName,
      ]
        .filter(Boolean)
        .some((value) =>
          value!.toLowerCase().includes(keyword),
        ),
    );
  }, [groups, search]);

  function openCreateDialog() {
    setDialogMode("create");
    setSelectedGroup(null);
    setForm(defaultGroupForm);
    setDialogOpen(true);
  }

  function openViewDialog(group: Group) {
    setDialogMode("view");
    setSelectedGroup(group);

    setForm({
      name: group.name,
      description: group.description || "",
    });

    setDialogOpen(true);
  }

  function openEditDialog(group: Group) {
    setDialogMode("edit");
    setSelectedGroup(group);

    setForm({
      name: group.name,
      description: group.description || "",
    });

    setDialogOpen(true);
  }

  async function refreshGroups() {
    const refreshedGroups = await getGroups();
    setGroups(refreshedGroups);
  }

  async function handleSave() {
    try {
      setSaving(true);

      if (dialogMode === "create") {
        await createGroup(form);
        toast.success("Group created successfully.");
      }

      if (dialogMode === "edit" && selectedGroup) {
        await updateGroup(
          selectedGroup.name,
          form,
        );

        toast.success("Group updated successfully.");
      }

      await refreshGroups();
      setDialogOpen(false);
    } catch (error) {
      toast.error(
        getErrorMessage(
          error,
          "Failed to save group.",
        ),
      );
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(group: Group) {
    try {
      await deleteGroup(group.name);

      toast.success("Group deleted successfully.");

      await refreshGroups();
    } catch (error) {
      toast.error(
        getErrorMessage(
          error,
          "Failed to delete group.",
        ),
      );
    }
  }

  async function handleOpenMembers(group: Group) {
    setSelectedGroup(group);
    setMembersData(null);
    setMembersError("");
    setMembersLoading(true);
    setMembersOpen(true);

    try {
      const data = await getGroupMembers(group.name);
      setMembersData(data);
    } catch (error) {
      setMembersError(
        getErrorMessage(
          error,
          "Failed to load group members.",
        ),
      );
    } finally {
      setMembersLoading(false);
    }
  }

  async function handleMembersChanged() {
    if (!selectedGroup) return;

    try {
      setMembersLoading(true);
      setMembersError("");

      const data = await getGroupMembers(
        selectedGroup.name,
      );

      setMembersData(data);

      await refreshGroups();
    } catch (error) {
      setMembersError(
        getErrorMessage(
          error,
          "Failed to refresh group members.",
        ),
      );
    } finally {
      setMembersLoading(false);
    }
  }

  return (
    <div className="space-y-6 p-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            Groups
          </h1>

          <p className="mt-1 text-muted-foreground">
            Manage LDAP groups and access memberships.
          </p>
        </div>

        <Button onClick={openCreateDialog}>
          Create Group
        </Button>
      </div>

      <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
        <Input
          value={search}
          onChange={(event) =>
            setSearch(event.target.value)
          }
          placeholder="Search group, description, or DN..."
          className="max-w-xl"
        />

        <p className="text-sm text-muted-foreground">
          {filteredGroups.length} group(s)
        </p>
      </div>

      <GroupTable
        groups={filteredGroups}
        onView={openViewDialog}
        onMembers={handleOpenMembers}
        onEdit={openEditDialog}
        onDelete={handleDelete}
      />

      <GroupDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        mode={dialogMode}
        form={form}
        onChange={setForm}
        onSave={handleSave}
        saving={saving}
      />

      <GroupMembersDialog
        open={membersOpen}
        onOpenChange={setMembersOpen}
        data={membersData}
        users={users}
        loading={membersLoading}
        error={membersError}
        onChanged={handleMembersChanged}
      />
    </div>
  );
}
