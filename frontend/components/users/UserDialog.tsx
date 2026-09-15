"use client";

import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";

import UserForm from "./UserForm";
import type { Unit } from "@/types/unit";
import type { UserFormData } from "@/types/users";

interface UserDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  user: UserFormData;
  units: Unit[];
  readOnly?: boolean;
  onChange?: (user: UserFormData) => void;
  onSave?: () => void;
  saving?: boolean;
  saveLabel?: string;
  showPasswordFields?: boolean;
  usernameReadOnly?: boolean;
  selfEmailOnly?: boolean;
  showSave?: boolean;
}

export default function UserDialog({
  open,
  onOpenChange,
  title,
  user,
  units,
  readOnly = false,
  onChange,
  onSave,
  saving = false,
  saveLabel = "Create User",
  showPasswordFields = true,
  usernameReadOnly = false,
  selfEmailOnly = false,
  showSave = true,
}: UserDialogProps) {
  function handleSave() {
    if (!saving) {
      onSave?.();
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-xl">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
        </DialogHeader>

        <UserForm
          user={user}
          units={units}
          readOnly={readOnly}
          onChange={onChange}
          showPasswordFields={showPasswordFields}
          usernameReadOnly={usernameReadOnly}
          selfEmailOnly={selfEmailOnly}
        />

        {!readOnly && showSave && (
          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              disabled={saving}
              onClick={() => onOpenChange(false)}
            >
              Cancel
            </Button>

            <Button
              type="button"
              disabled={saving}
              onClick={handleSave}
            >
              {saving ? "Saving..." : saveLabel}
            </Button>
          </DialogFooter>
        )}
      </DialogContent>
    </Dialog>
  );
}
