"use client";

import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { Button } from "@/components/ui/button";

import PolicyForm from "./PolicyForm";

import type { PolicyFormData } from "@/types/policy";

interface Props {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  policy: PolicyFormData;
  onChange?: (
    policy: PolicyFormData,
  ) => void;
  onSave?: () => void;
  saving?: boolean;
  saveLabel?: string;
}

export default function PolicyDialog({
  open,
  onOpenChange,
  title,
  policy,
  onChange,
  onSave,
  saving = false,
  saveLabel = "Save Policy",
}: Props) {

  function handleSave() {
    if (!saving) {
      onSave?.();
    }
  }

  return (
    <Dialog
      open={open}
      onOpenChange={onOpenChange}
    >
      <DialogContent className="sm:max-w-xl">

        <DialogHeader>
          <DialogTitle>
            {title}
          </DialogTitle>
        </DialogHeader>

        <PolicyForm
          policy={policy}
          onChange={onChange}
        />

        <DialogFooter>

          <Button
            variant="outline"
            onClick={() =>
              onOpenChange(false)
            }
          >
            Cancel
          </Button>

          <Button
            disabled={saving}
            onClick={handleSave}
          >
            {saving
              ? "Saving..."
              : saveLabel}
          </Button>

        </DialogFooter>

      </DialogContent>
    </Dialog>
  );
}