"use client";

import {
  FormEvent,
  useMemo,
  useState,
} from "react";

import {
  Pencil,
  Plus,
  Search,
  ShieldCheck,
  Trash2,
  UsersRound,
} from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

import {
  createRole,
  deleteRole,
  getRoleMembers,
  updateRole,
} from "@/services/role.service";

import type {
  Role,
  RoleMembersResponse,
} from "@/types/role";

import RoleMembersDialog from "./RoleMembersDialog";

interface UserOption {
  username: string;
  fullName: string;
  enabled: boolean;
}

interface RolesClientProps {
  initialRoles: Role[];
  users: UserOption[];
}

interface RoleFormState {
  name: string;
  description: string;
}

const emptyForm: RoleFormState = {
  name: "",
  description: "",
};

export default function RolesClient({
  initialRoles,
  users,
}: RolesClientProps) {
  const [roles, setRoles] =
    useState<Role[]>(initialRoles);

  const [search, setSearch] =
    useState("");

  const [form, setForm] =
    useState<RoleFormState>(emptyForm);

  const [editingName, setEditingName] =
    useState<string | null>(null);

  const [showForm, setShowForm] =
    useState(false);

  const [loading, setLoading] =
    useState(false);

  const [error, setError] =
    useState("");

  const [membersOpen, setMembersOpen] =
    useState(false);

  const [membersData, setMembersData] =
    useState<RoleMembersResponse | null>(null);

  const [membersLoading, setMembersLoading] =
    useState(false);

  const [membersError, setMembersError] =
    useState("");

  const filteredRoles = useMemo(() => {
    const query =
      search.trim().toLowerCase();

    if (!query) {
      return roles;
    }

    return roles.filter((role) =>
      role.name
        .toLowerCase()
        .includes(query) ||
      role.description
        .toLowerCase()
        .includes(query),
    );
  }, [roles, search]);

  const totalMembers = useMemo(
    () =>
      roles.reduce(
        (total, role) =>
          total + role.memberCount,
        0,
      ),
    [roles],
  );

  function openCreateForm() {
    setEditingName(null);
    setForm(emptyForm);
    setError("");
    setShowForm(true);
  }

  function openEditForm(role: Role) {
    setEditingName(role.name);

    setForm({
      name: role.name,
      description: role.description,
    });

    setError("");
    setShowForm(true);
  }

  function closeForm() {
    setEditingName(null);
    setForm(emptyForm);
    setError("");
    setShowForm(false);
  }

  async function handleSubmit(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();

    const name = form.name.trim();
    const description =
      form.description.trim();

    if (!name) {
      setError("Role name is required.");
      return;
    }

    setLoading(true);
    setError("");

    try {
      if (editingName) {
        const updated =
          await updateRole(
            editingName,
            {
              name,
              description,
            },
          );

        setRoles((current) =>
          current
            .map((role) =>
              role.name === editingName
                ? updated
                : role,
            )
            .sort((a, b) =>
              a.name.localeCompare(b.name),
            ),
        );
      } else {
        const created =
          await createRole({
            name,
            description,
          });

        setRoles((current) =>
          [...current, created].sort(
            (a, b) =>
              a.name.localeCompare(b.name),
          ),
        );
      }

      closeForm();
    } catch (caughtError) {
      setError(
        caughtError instanceof Error
          ? caughtError.message
          : "Unable to save role.",
      );
    } finally {
      setLoading(false);
    }
  }

  async function handleOpenMembers(
    role: Role,
  ) {
    setMembersOpen(true);
    setMembersData(null);
    setMembersError("");
    setMembersLoading(true);

    try {
      const data =
        await getRoleMembers(role.name);

      setMembersData(data);
    } catch (caughtError) {
      setMembersError(
        caughtError instanceof Error
          ? caughtError.message
          : "Unable to load role members.",
      );
    } finally {
      setMembersLoading(false);
    }
  }

  async function handleMembersChanged() {
    if (!membersData?.roleName) {
      return;
    }

    const roleName = membersData.roleName;

    setMembersLoading(true);
    setMembersError("");

    try {
      const data =
        await getRoleMembers(roleName);

      setMembersData(data);

      setRoles((current) =>
        current.map((role) =>
          role.name === roleName
            ? {
                ...role,
                memberCount: data.total,
              }
            : role,
        ),
      );
    } catch (caughtError) {
      setMembersError(
        caughtError instanceof Error
          ? caughtError.message
          : "Unable to refresh role members.",
      );

      throw caughtError;
    } finally {
      setMembersLoading(false);
    }
  }

  async function handleDelete(
    role: Role,
  ) {
    const confirmed = window.confirm(
      `Delete role "${role.name}"?`,
    );

    if (!confirmed) {
      return;
    }

    setLoading(true);
    setError("");

    try {
      await deleteRole(role.name);

      setRoles((current) =>
        current.filter(
          (item) =>
            item.name !== role.name,
        ),
      );
    } catch (caughtError) {
      setError(
        caughtError instanceof Error
          ? caughtError.message
          : "Unable to delete role.",
      );
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight text-slate-950">
            Roles
          </h1>

          <p className="mt-1 text-sm text-slate-500">
            Manage LDAP-backed authorization roles.
          </p>
        </div>

        <Button
          type="button"
          onClick={openCreateForm}
        >
          <Plus className="mr-2 h-4 w-4" />
          Add Role
        </Button>
      </div>

      <div className="grid gap-4 md:grid-cols-3">
        <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-slate-500">
                Total Roles
              </p>

              <p className="mt-2 text-3xl font-semibold text-slate-950">
                {roles.length}
              </p>
            </div>

            <div className="rounded-xl bg-emerald-50 p-3 text-emerald-700">
              <ShieldCheck className="h-5 w-5" />
            </div>
          </div>
        </div>

        <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-slate-500">
                Assigned Members
              </p>

              <p className="mt-2 text-3xl font-semibold text-slate-950">
                {totalMembers}
              </p>
            </div>

            <div className="rounded-xl bg-blue-50 p-3 text-blue-700">
              <UsersRound className="h-5 w-5" />
            </div>
          </div>
        </div>

        <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
          <div>
            <p className="text-sm font-medium text-slate-500">
              Identity Source
            </p>

            <p className="mt-2 text-lg font-semibold text-slate-950">
              OpenLDAP
            </p>

            <p className="mt-1 text-sm text-slate-500">
              ou=Roles
            </p>
          </div>
        </div>
      </div>

      {showForm && (
        <form
          onSubmit={handleSubmit}
          className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm"
        >
          <div className="flex flex-col gap-4">
            <div>
              <h2 className="font-semibold text-slate-950">
                {editingName
                  ? "Edit Role"
                  : "Create Role"}
              </h2>

              <p className="mt-1 text-sm text-slate-500">
                Role data is written directly to OpenLDAP.
              </p>
            </div>

            <div className="grid gap-4 md:grid-cols-2">
              <div>
                <label className="mb-2 block text-sm font-medium text-slate-700">
                  Role Name
                </label>

                <Input
                  value={form.name}
                  onChange={(event) =>
                    setForm((current) => ({
                      ...current,
                      name: event.target.value,
                    }))
                  }
                  placeholder="Example: Network Administrator"
                  disabled={loading}
                />
              </div>

              <div>
                <label className="mb-2 block text-sm font-medium text-slate-700">
                  Description
                </label>

                <Input
                  value={form.description}
                  onChange={(event) =>
                    setForm((current) => ({
                      ...current,
                      description:
                        event.target.value,
                    }))
                  }
                  placeholder="Role description"
                  disabled={loading}
                />
              </div>
            </div>

            {error && (
              <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                {error}
              </div>
            )}

            <div className="flex items-center gap-2">
              <Button
                type="submit"
                disabled={loading}
              >
                {loading
                  ? "Saving..."
                  : editingName
                    ? "Save Changes"
                    : "Create Role"}
              </Button>

              <Button
                type="button"
                variant="outline"
                onClick={closeForm}
                disabled={loading}
              >
                Cancel
              </Button>
            </div>
          </div>
        </form>
      )}

      {!showForm && error && (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      )}

      <div className="rounded-xl border border-slate-200 bg-white shadow-sm">
        <div className="flex flex-col gap-4 border-b border-slate-200 p-5 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h2 className="font-semibold text-slate-950">
              Role Directory
            </h2>

            <p className="mt-1 text-sm text-slate-500">
              {filteredRoles.length} role
              {filteredRoles.length === 1
                ? ""
                : "s"}{" "}
              displayed
            </p>
          </div>

          <div className="relative w-full sm:w-80">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />

            <Input
              value={search}
              onChange={(event) =>
                setSearch(event.target.value)
              }
              placeholder="Search roles..."
              className="pl-9"
            />
          </div>
        </div>

        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Role</TableHead>
              <TableHead>Description</TableHead>
              <TableHead>Members</TableHead>
              <TableHead className="text-right">
                Actions
              </TableHead>
            </TableRow>
          </TableHeader>

          <TableBody>
            {filteredRoles.length === 0 ? (
              <TableRow>
                <TableCell
                  colSpan={4}
                  className="h-32 text-center text-slate-500"
                >
                  No roles found.
                </TableCell>
              </TableRow>
            ) : (
              filteredRoles.map((role) => (
                <TableRow key={role.name}>
                  <TableCell>
                    <div className="flex items-center gap-3">
                      <div className="rounded-lg bg-emerald-50 p-2 text-emerald-700">
                        <ShieldCheck className="h-4 w-4" />
                      </div>

                      <span className="font-medium text-slate-950">
                        {role.name}
                      </span>
                    </div>
                  </TableCell>

                  <TableCell className="text-slate-600">
                    {role.description || "-"}
                  </TableCell>

                  <TableCell>
                    {role.memberCount}
                  </TableCell>

                  <TableCell>
                    <div className="flex justify-end gap-2">
                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        onClick={() =>
                          handleOpenMembers(role)
                        }
                        disabled={loading}
                        title="Manage members"
                      >
                        <UsersRound className="h-4 w-4" />
                      </Button>

                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        onClick={() =>
                          openEditForm(role)
                        }
                        disabled={loading}
                      >
                        <Pencil className="h-4 w-4" />
                      </Button>

                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        onClick={() =>
                          handleDelete(role)
                        }
                        disabled={loading}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      <RoleMembersDialog
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
