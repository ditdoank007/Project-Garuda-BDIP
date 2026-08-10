"use client";

import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { Button } from "@/components/ui/button";

import type { ApplicationFormData } from "@/services/application.service";

import ApplicationForm from "./ApplicationForm";

interface Props {
  open: boolean;
  onOpenChange: (open: boolean) => void;

  title: string;

  application: ApplicationFormData;

  onChange: (value: ApplicationFormData) => void;

  onSave: () => void;

  saving: boolean;

  saveLabel: string;

  readOnlyCode?: boolean;
}

export default function ApplicationDialog({
  open,
  onOpenChange,
  title,
  application,
  onChange,
  onSave,
  saving,
  saveLabel,
  readOnlyCode = false,
}: Props) {
  return (
    <Dialog
      open={open}
      onOpenChange={onOpenChange}
    >
      <DialogContent>
        <DialogHeader>
          <DialogTitle>
            {title}
          </DialogTitle>
        </DialogHeader>

        <ApplicationForm
          value={application}
          onChange={onChange}
          readOnlyCode={readOnlyCode}
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
            onClick={onSave}
            disabled={saving}
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
