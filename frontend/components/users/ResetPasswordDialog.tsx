"use client";

import { useEffect, useState } from "react";
import { toast } from "sonner";
import { apiPost } from "@/services/api";

import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

type User = {
  username: string;
};

interface ResetPasswordDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  user: User | null;
}

export default function ResetPasswordDialog({
  open,
  onOpenChange,
  user,
}: ResetPasswordDialogProps) {
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (open) {
      setNewPassword("");
      setConfirmPassword("");
    }
  }, [open, user?.username]);

  async function handleResetPassword() {
    if (!user) return;

    if (!newPassword) {
      toast.error("New password is required.");
      return;
    }

    if (newPassword.length < 8) {
      toast.error("Password must contain at least 8 characters.");
      return;
    }

    if (newPassword !== confirmPassword) {
      toast.error("Password confirmation does not match.");
      return;
    }

    try {
      setSaving(true);

      const result = await apiPost<{
        success?: boolean;
        message?: string;
      }>(
        `/users/${encodeURIComponent(user.username)}/reset-password`,
        {
          newPassword,
        },
      );

      if (result.success !== true) {
        throw new Error(
          result.message ?? "Password reset failed.",
        );
      }

      toast.success(`Password for "${user.username}" reset successfully.`);
      onOpenChange(false);
    } catch (error) {
      console.error("Reset password failed:", error);

      toast.error(
        error instanceof Error
          ? error.message
          : "Failed to reset password.",
      );
    } finally {
      setSaving(false);
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Reset Password</DialogTitle>
        </DialogHeader>

        <div className="grid gap-4 py-2">
          <div className="grid gap-2">
            <Label htmlFor="username">Username</Label>
            <Input id="username" value={user?.username ?? ""} disabled />
          </div>

          <div className="grid gap-2">
            <Label htmlFor="new-password">New Password</Label>
            <Input
              id="new-password"
              type="password"
              value={newPassword}
              onChange={(event) => setNewPassword(event.target.value)}
              disabled={saving}
            />
          </div>

          <div className="grid gap-2">
            <Label htmlFor="confirm-password">Confirm New Password</Label>
            <Input
              id="confirm-password"
              type="password"
              value={confirmPassword}
              onChange={(event) => setConfirmPassword(event.target.value)}
              disabled={saving}
            />
          </div>
        </div>

        <DialogFooter>
          <Button
            type="button"
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={saving}
          >
            Cancel
          </Button>
          <Button
            type="button"
            onClick={handleResetPassword}
            disabled={saving}
          >
            {saving ? "Saving..." : "Reset Password"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
