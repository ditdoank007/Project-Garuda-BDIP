"use client";

import { useEffect, useState } from "react";
import { toast } from "sonner";

import useSmartSearch from "@/hooks/useSmartSearch";
import useDebounce from "@/hooks/useDebounce";

import UserToolbar from "./UserToolbar";
import UserTable from "./UserTable";
import UserDialog from "./UserDialog";
import ImportUsersDialog from "./ImportUsersDialog";

import {
  createUser,
  updateUser,
} from "@/lib/api/users";
import { defaultUserForm } from "@/constants/users";
import { getUnits } from "@/services/unit.service";
import type { Unit } from "@/types/unit";
import type {
  User,
  UserFormData,
} from "@/types/users";

interface UsersClientProps {
  users: User[];
}

type DialogMode = "create" | "edit";

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

function userToFormData(user: User): UserFormData {
  return {
    username: user.username,
    fullName: user.fullName ?? "",
    email: user.email ?? "",
    unit: user.unit ?? "",
    password: "",
    confirmPassword: "",
    enabled: user.enabled,
  };
}

export default function UsersClient({
  users,
}: UsersClientProps) {
  const [keyword, setKeyword] = useState("");
  const [dialogOpen, setDialogOpen] = useState(false);
  const [dialogMode, setDialogMode] =
    useState<DialogMode>("create");
  const [formData, setFormData] =
    useState<UserFormData>(defaultUserForm);
  const [saving, setSaving] = useState(false);
  const [importOpen, setImportOpen] = useState(false);
  const [units, setUnits] = useState<Unit[]>([]);

  useEffect(() => {
    let active = true;

    async function loadUnits() {
      try {
        const ldapUnits = await getUnits();

        if (active) {
          setUnits(ldapUnits);
        }
      } catch (error) {
        console.error("Load units failed:", error);

        if (active) {
          toast.error("Failed to load organizational units.");
        }
      }
    }

    void loadUnits();

    return () => {
      active = false;
    };
  }, []);

  const debouncedKeyword = useDebounce(keyword, 300);

  const filteredUsers = useSmartSearch(
    users,
    debouncedKeyword,
    ["username", "fullName", "email", "unit"],
  );

  async function handleCreateUser() {
    const username = formData.username.trim();
    const fullName = formData.fullName.trim();
    const email = formData.email.trim();
    const unit = formData.unit.trim();

    if (!username) {
      toast.error("Username is required.");
      return;
    }

    if (!/^[a-zA-Z0-9._-]+$/.test(username)) {
      toast.error(
        "Username may only contain letters, numbers, dots, underscores, and hyphens.",
      );
      return;
    }

    if (!fullName) {
      toast.error("Full name is required.");
      return;
    }

    if (!email) {
      toast.error("Email is required.");
      return;
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      toast.error("Enter a valid email address.");
      return;
    }

    if (!formData.password) {
      toast.error("Password is required.");
      return;
    }

    if (formData.password.length < 8) {
      toast.error("Password must contain at least 8 characters.");
      return;
    }

    if (formData.password !== formData.confirmPassword) {
      toast.error("Password confirmation does not match.");
      return;
    }

    try {
      setSaving(true);

      await createUser({
        ...formData,
        username,
        fullName,
        email,
        unit,
      });

      toast.success(`User "${username}" created successfully.`);
      setDialogOpen(false);
      setFormData(defaultUserForm);
      window.location.reload();
    } catch (error) {
      console.error("Create user failed:", error);
      toast.error(
        getErrorMessage(error, "Failed to create user."),
      );
    } finally {
      setSaving(false);
    }
  }

  async function handleUpdateUser() {
    const username = formData.username.trim();
    const fullName = formData.fullName.trim();
    const email = formData.email.trim();
    const unit = formData.unit.trim();

    if (!fullName) {
      toast.error("Full name is required.");
      return;
    }

    if (!email) {
      toast.error("Email is required.");
      return;
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      toast.error("Enter a valid email address.");
      return;
    }

    try {
      setSaving(true);

      await updateUser(username, {
        uid: username,
        username,
        fullName,
        email,
        unit,
        enabled: formData.enabled,
      });

      toast.success(`User "${username}" updated successfully.`);
      setDialogOpen(false);
      window.location.reload();
    } catch (error) {
      console.error("Update user failed:", error);
      toast.error(
        getErrorMessage(error, "Failed to update user."),
      );
    } finally {
      setSaving(false);
    }
  }

  function handleEditUser(user: User) {
    setDialogMode("edit");
    setFormData(userToFormData(user));
    setDialogOpen(true);
  }

  function handleDialogSave() {
    if (dialogMode === "edit") {
      void handleUpdateUser();
      return;
    }

    void handleCreateUser();
  }

  return (
    <div className="space-y-6">
      <UserToolbar
        keyword={keyword}
        onKeywordChange={setKeyword}
        onRefresh={() => window.location.reload()}
        onImportCsv={() => setImportOpen(true)}
        onCreateUser={() => {
          setDialogMode("create");
          setFormData(defaultUserForm);
          setDialogOpen(true);
        }}
      />

      <UserTable
        users={filteredUsers}
        onEdit={handleEditUser}
      />

      <ImportUsersDialog
        open={importOpen}
        onOpenChange={(open) => {
          setImportOpen(open);

          if (!open) {
            window.location.reload();
          }
        }}
      />

      <UserDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        title={
          dialogMode === "create"
            ? "Create User"
            : "Edit User"
        }
        user={formData}
        units={units}
        onChange={setFormData}
        onSave={handleDialogSave}
        saving={saving}
        saveLabel={
          dialogMode === "create"
            ? "Create User"
            : "Save Changes"
        }
        showPasswordFields={dialogMode === "create"}
        usernameReadOnly={dialogMode === "edit"}
      />
    </div>
  );
}
