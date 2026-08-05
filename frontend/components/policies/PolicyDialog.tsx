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
  onChange?: (policy: PolicyFormData) => void;
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
      <DialogContent
        className="
          sm:max-w-2xl
          max-h-[90vh]
          !grid
          grid-rows-[auto_minmax(0,1fr)_auto]
          overflow-hidden
          p-0
        "
      >
        <DialogHeader className="border-b px-6 py-4">
          <DialogTitle>{title}</DialogTitle>
        </DialogHeader>

        <div className="min-h-0 overflow-y-auto px-6 py-4">
          <PolicyForm
            policy={policy}
            onChange={onChange}
          />
        </div>

        <DialogFooter className="border-t px-6 py-4">
          <Button
            variant="outline"
            onClick={() => onOpenChange(false)}
          >
            Cancel
          </Button>

          <Button
            disabled={saving}
            onClick={handleSave}
          >
            {saving ? "Saving..." : saveLabel}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
