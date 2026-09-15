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
} from "@/services/users.service";
import { defaultUserForm } from "@/constants/users";
import { getUnits } from "@/services/unit.service";
import {
  getNapPolicies,
  getAllUserNap,
} from "@/services/policy.service";
import type { Policy } from "@/types/policy";
import type { Unit } from "@/types/unit";
import type {
  User,
  UserFormData,
} from "@/types/users";

interface UsersClientProps {
  users: User[];
}

type DialogMode = "create" | "edit";

interface UserNapInfo {
  policyId?: string;
  policyCode?: string;
}

function getErrorMessage(
  error: unknown,
  fallback: string,
): string {
  if (error instanceof Error && error.message) {
    return error.message;
  }

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
    nip: user.nip ?? "",
    fingerId: user.fingerId ?? "",
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
  const [originalUsername, setOriginalUsername] = useState("");
  const [saving, setSaving] = useState(false);
  const [importOpen, setImportOpen] = useState(false);
  const [units, setUnits] = useState<Unit[]>([]);
  const [policies, setPolicies] = useState<Policy[]>([]);
  const [userRows, setUserRows] = useState<User[]>(users);

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

    async function loadPolicies() {
      try {
        const response = await getNapPolicies();

        if (active) {
          setPolicies(response.data ?? []);
        }
      } catch (error) {
        console.error("Load policies failed:", error);

        if (active) {
          toast.error("Failed to load NAP policies.");
        }
      }
    }

    async function loadUserNap() {
      try {
        const response = await getAllUserNap();

        if (!active) return;

        const map = new Map<string, UserNapInfo>(
          (response.data ?? []).map(
            (item: any) => [
              item.uid,
              {
                policyId: item.policyId,
                policyCode: item.policyCode,
              },
            ],
          ),
        );

        setUserRows(
          users.map((user) => {
            const nap = map.get(user.uid);

            return {
              ...user,
              policyId: nap?.policyId,
              policyCode: nap?.policyCode,
            };
          }),
        );
      } catch (error) {
        console.error("Load user NAP failed:", error);
      }
    }

    void loadUnits();
    void loadPolicies();
    void loadUserNap();

    return () => {
      active = false;
    };
  }, []);

  const debouncedKeyword = useDebounce(keyword, 300);

  const filteredUsers = useSmartSearch(
    userRows,
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
      setOriginalUsername("");
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
    const oldUsername = originalUsername || username;

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

    try {
      setSaving(true);

      await updateUser(oldUsername, {
        uid: username,
        username,
        nip: formData.nip.trim(),
        fingerId: formData.fingerId.trim(),
        fullName,
        email,
        unit,
        enabled: formData.enabled,
      });

      toast.success(`User "${username}" updated successfully.`);
      setDialogOpen(false);
      setOriginalUsername("");
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
    setOriginalUsername(user.username);
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
    <div className="flex h-full min-h-0 flex-col">

      <div className="sticky top-0 z-20 shrink-0 bg-slate-100 pb-4">
        <UserToolbar
          keyword={keyword}
          onKeywordChange={setKeyword}
          onRefresh={() => window.location.reload()}
          onImportCsv={() => setImportOpen(true)}
          onCreateUser={() => {
            setDialogMode("create");
            setOriginalUsername("");
            setFormData(defaultUserForm);
            setDialogOpen(true);
          }}
        />
      </div>

      <div className="min-h-0 flex-1">
        <UserTable
          users={filteredUsers}
          policies={policies}
          onEdit={handleEditUser}
        />
      </div>

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
        usernameReadOnly={false}
      />
    </div>
  );
}
