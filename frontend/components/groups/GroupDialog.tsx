"use client";

import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

import type { GroupFormData } from "@/types/groups";

type DialogMode = "create" | "view" | "edit";

interface GroupDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  mode: DialogMode;
  form: GroupFormData;
  onChange: (form: GroupFormData) => void;
  onSave: () => Promise<void>;
  saving?: boolean;
}

export default function GroupDialog({
  open,
  onOpenChange,
  mode,
  form,
  onChange,
  onSave,
  saving = false,
}: GroupDialogProps) {
  const isView = mode === "view";
  const isCreate = mode === "create";

  const title = isCreate
    ? "Create Group"
    : isView
      ? "Group Details"
      : "Edit Group";

  function updateField(
    field: keyof GroupFormData,
    value: string,
  ) {
    onChange({
      ...form,
      [field]: value,
    });
  }

  async function handleSave() {
    await onSave();
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-lg">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
        </DialogHeader>

        <div className="space-y-4 py-2">
          <div className="space-y-2">
            <Label htmlFor="group-name">
              Group Name
            </Label>

            <Input
              id="group-name"
              value={form.name}
              readOnly={isView || !isCreate}
              disabled={saving}
              placeholder="Contoh: PUSDATIN"
              onChange={(event) =>
                updateField("name", event.target.value)
              }
            />

            {!isCreate && !isView && (
              <p className="text-xs text-muted-foreground">
                Group name tidak dapat diubah karena merupakan
                identitas LDAP group.
              </p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="group-description">
              Description
            </Label>

            <textarea
              id="group-description"
              value={form.description}
              readOnly={isView}
              disabled={saving}
              placeholder="Contoh: Akses layanan Pusdatin"
              rows={4}
              className="flex w-full rounded-md border border-input bg-background px-3 py-2 text-sm shadow-sm outline-none placeholder:text-muted-foreground focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
              onChange={(event) =>
                updateField(
                  "description",
                  event.target.value,
                )
              }
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
            {isView ? "Close" : "Cancel"}
          </Button>

          {!isView && (
            <Button
              type="button"
              onClick={handleSave}
              disabled={
                saving ||
                !form.name.trim()
              }
            >
              {saving
                ? "Saving..."
                : isCreate
                  ? "Create Group"
                  : "Save Changes"}
            </Button>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
