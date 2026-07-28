"use client";

import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { Button } from "@/components/ui/button";

import type { LocationFormData } from "@/types/location";

import LocationForm from "./LocationForm";

interface Props {
  open: boolean;
  onOpenChange: (open: boolean) => void;

  title: string;

  location: LocationFormData;

  onChange: (
    value: LocationFormData,
  ) => void;

  onSave: () => void;

  saving: boolean;

  saveLabel: string;

  readOnlyName?: boolean;
}

export default function LocationDialog({
  open,
  onOpenChange,
  title,
  location,
  onChange,
  onSave,
  saving,
  saveLabel,
  readOnlyName = false,
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

        <LocationForm
          value={location}
          onChange={onChange}
          readOnlyName={readOnlyName}
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