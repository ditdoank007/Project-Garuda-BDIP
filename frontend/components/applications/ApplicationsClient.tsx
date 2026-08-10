"use client";

import { useState } from "react";
import { Plus } from "lucide-react";

import type {
  Application,
  ApplicationFormData,
} from "@/services/application.service";

import {
  createApplication,
  updateApplication,
  deactivateApplication,
} from "@/services/application.service";

import ApplicationTable from "./ApplicationTable";
import ApplicationDialog from "./ApplicationDialog";

interface ApplicationsClientProps {
  applications: Application[];
}

type DialogMode = "create" | "edit";

const defaultApplicationForm: ApplicationFormData = {
  code: "",
  name: "",
  description: "",
  baseUrl: "",
};

function applicationToFormData(
  application: Application,
): ApplicationFormData {
  return {
    code: application.code,
    name: application.name,
    description: application.description ?? "",
    baseUrl: application.baseUrl,
  };
}

export default function ApplicationsClient({
  applications,
}: ApplicationsClientProps) {
  const [dialogOpen, setDialogOpen] = useState(false);

  const [dialogMode, setDialogMode] =
    useState<DialogMode>("create");

  const [saving, setSaving] = useState(false);

  const [editingApplication, setEditingApplication] =
    useState<Application | null>(null);

  const [formData, setFormData] =
    useState<ApplicationFormData>(
      defaultApplicationForm,
    );

  async function handleCreateApplication() {
    setSaving(true);

    try {
      await createApplication(formData);
      window.location.reload();
    } finally {
      setSaving(false);
    }
  }

  async function handleUpdateApplication() {
    if (!editingApplication) {
      return;
    }

    setSaving(true);

    try {
      await updateApplication(
        editingApplication.code,
        {
          name: formData.name,
          description: formData.description,
          baseUrl: formData.baseUrl,
        },
      );

      window.location.reload();
    } finally {
      setSaving(false);
    }
  }

  async function handleDeactivateApplication(
    application: Application,
  ) {
    const confirmed = window.confirm(
      `Deactivate application "${application.name}"?`,
    );

    if (!confirmed) {
      return;
    }

    await deactivateApplication(application.code);
    window.location.reload();
  }

  async function handleDialogSave() {
    if (dialogMode === "create") {
      await handleCreateApplication();
    } else {
      await handleUpdateApplication();
    }
  }

  function handleEditApplication(
    application: Application,
  ) {
    setDialogMode("edit");
    setEditingApplication(application);
    setFormData(
      applicationToFormData(application),
    );
    setDialogOpen(true);
  }

  function handleOpenCreate() {
    setDialogMode("create");
    setEditingApplication(null);
    setFormData({
      ...defaultApplicationForm,
    });
    setDialogOpen(true);
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">
            Applications
          </h1>

          <p className="text-slate-500">
            Manage applications integrated with BDIP.
          </p>
        </div>

        <button
          onClick={handleOpenCreate}
          className="flex items-center gap-2 rounded-lg bg-slate-900 px-4 py-2 text-white hover:bg-slate-800"
        >
          <Plus size={18} />
          Add Application
        </button>
      </div>

      <div className="rounded-xl border bg-white p-6 shadow-sm">
        <p className="text-sm text-slate-500">
          Total Applications
        </p>

        <p className="mt-2 text-4xl font-bold">
          {applications.length}
        </p>
      </div>

      <ApplicationTable
        applications={applications}
        onEdit={handleEditApplication}
        onDeactivate={handleDeactivateApplication}
      />

      <ApplicationDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        title={
          dialogMode === "create"
            ? "Create Application"
            : "Edit Application"
        }
        application={formData}
        onChange={setFormData}
        onSave={handleDialogSave}
        saving={saving}
        saveLabel={
          dialogMode === "create"
            ? "Create Application"
            : "Save Changes"
        }
        readOnlyCode={dialogMode === "edit"}
      />
    </div>
  );
}
