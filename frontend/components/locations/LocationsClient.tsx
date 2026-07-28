"use client";

import { useState } from "react";
import { Plus } from "lucide-react";

import type {
  Location,
  LocationFormData,
} from "@/types/location";

import {
  createLocation,
  updateLocation,
} from "@/lib/api/locations";

import { defaultLocationForm } from "@/constants/locations";

import LocationTable from "./LocationTable";
import LocationDialog from "./LocationDialog";

interface LocationsClientProps {
  locations: Location[];
}

type DialogMode = "create" | "edit";

function locationToFormData(
  location: Location,
): LocationFormData {
  return {
    name: location.name,
    type: location.type ?? "",
    description:
      location.description ?? "",
  };
}

export default function LocationsClient({
  locations,
}: LocationsClientProps) {
  const [dialogOpen, setDialogOpen] =
    useState(false);

  const [dialogMode, setDialogMode] =
    useState<DialogMode>("create");

  const [saving, setSaving] =
    useState(false);

  const [editingLocation, setEditingLocation] =
    useState<Location | null>(null);

  const [formData, setFormData] =
    useState(defaultLocationForm);

  function handleEditLocation(
    location: Location,
  ) {
    setDialogMode("edit");
    setEditingLocation(location);
    setFormData(
      locationToFormData(location),
    );
    setDialogOpen(true);
  }

  async function handleCreateLocation() {
    setSaving(true);

    try {
      await createLocation(formData);

      window.location.reload();
    } finally {
      setSaving(false);
    }
  }

  async function handleUpdateLocation() {
    if (!editingLocation) {
      return;
    }

    setSaving(true);

    try {
      await updateLocation(
        editingLocation.name,
        formData,
      );

      window.location.reload();
    } finally {
      setSaving(false);
    }
  }

  async function handleDialogSave() {
    if (dialogMode === "create") {
      await handleCreateLocation();
    } else {
      await handleUpdateLocation();
    }
  }

  return (
    <div className="space-y-6">

      <div className="flex items-center justify-between">

        <div>

          <h1 className="text-3xl font-bold">
            Locations
          </h1>

          <p className="text-slate-500">
            Manage BDIP Locations.
          </p>

        </div>

        <button
          onClick={() => {
            setDialogMode("create");
            setEditingLocation(null);
            setFormData(
              defaultLocationForm,
            );
            setDialogOpen(true);
          }}
          className="flex items-center gap-2 rounded-lg bg-slate-900 px-4 py-2 text-white hover:bg-slate-800"
        >
          <Plus size={18} />
          Add Location
        </button>

      </div>

      <div className="rounded-xl border bg-white p-6 shadow-sm">

        <p>Total Locations</p>

        <p className="mt-2 text-4xl font-bold">
          {locations.length}
        </p>

      </div>

      <LocationTable
        locations={locations}
        onEdit={handleEditLocation}
      />

      <LocationDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        title={
          dialogMode === "create"
            ? "Create Location"
            : "Edit Location"
        }
        location={formData}
        onChange={setFormData}
        onSave={handleDialogSave}
        saving={saving}
        saveLabel={
          dialogMode === "create"
            ? "Create Location"
            : "Save Changes"
        }
        readOnlyName={
          dialogMode === "edit"
        }
      />

    </div>
  );
}