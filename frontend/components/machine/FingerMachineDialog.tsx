"use client";

import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { Button } from "@/components/ui/button";

import type {
  FingerMachineFormData,
} from "@/types/finger-machine";

import FingerMachineForm from "./FingerMachineForm";

interface Props {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  machine: FingerMachineFormData;
  locations: {
    id: string;
    name: string;
  }[];
  onChange: (
    value: FingerMachineFormData,
  ) => void;
  onSave: () => void;
  saving: boolean;
  saveLabel: string;
  readOnlyCode?: boolean;
}

export default function FingerMachineDialog({
  open,
  onOpenChange,
  title,
  machine,
  locations,
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
      <DialogContent className="sm:max-w-xl">

        <DialogHeader>
          <DialogTitle>
            {title}
          </DialogTitle>
        </DialogHeader>

        <FingerMachineForm
          value={machine}
          locations={locations}
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
            Batal
          </Button>

          <Button
            onClick={onSave}
            disabled={saving}
          >
            {saving
              ? "Menyimpan..."
              : saveLabel}
          </Button>

        </DialogFooter>

      </DialogContent>
    </Dialog>
  );
}
