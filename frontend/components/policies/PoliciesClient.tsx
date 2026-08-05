"use client";

import { useState } from "react";
import { Plus } from "lucide-react";

import type {
  Policy,
  PolicyFormData,
} from "@/types/policy";

import {
  createPolicy,
  updatePolicy,
  deletePolicy,
} from "@/lib/api/policies";

import PolicyTable from "./PolicyTable";
import PolicyDialog from "./PolicyDialog";

import { defaultPolicyForm } from "@/constants/policies";

interface PoliciesClientProps {
  policies: Policy[];
}

type DialogMode = "create" | "edit";

function policyToFormData(
  policy: Policy,
): PolicyFormData {
  return {
    code: policy.code,
    name: policy.name,
    description: policy.description ?? "",

    enabled: policy.enabled,
    priority: policy.priority,

    sessionTimeout: policy.sessionTimeout,
    idleTimeout: policy.idleTimeout,
    simultaneousUse: policy.simultaneousUse,

    downloadRate: policy.downloadRate,
    uploadRate: policy.uploadRate,

    burstDownload: policy.burstDownload,
    burstUpload: policy.burstUpload,

    dailyQuota: policy.dailyQuota,
    monthlyQuota: policy.monthlyQuota,
    totalQuota: policy.totalQuota,

    addressList: policy.addressList,
    vlanId: policy.vlanId,
    ipPool: policy.ipPool,

    expirationDate: policy.expirationDate,
    loginSchedule: policy.loginSchedule,
  };
}

export default function PoliciesClient({
  policies,
}: PoliciesClientProps) {
  const [dialogOpen, setDialogOpen] =
    useState(false);

  const [dialogMode, setDialogMode] =
    useState<DialogMode>("create");

  const [saving, setSaving] =
    useState(false);

  const [editingPolicy, setEditingPolicy] =
    useState<Policy | null>(null);

  const [formData, setFormData] =
    useState(defaultPolicyForm);

  function handleEditPolicy(
    policy: Policy,
  ) {
    setDialogMode("edit");
    setEditingPolicy(policy);
    setFormData(
      policyToFormData(policy),
    );
    setDialogOpen(true);
  }

  async function handleCreatePolicy() {
    setSaving(true);

    try {
      await createPolicy(formData);

      window.location.reload();
    } finally {
      setSaving(false);
    }
  }

  async function handleUpdatePolicy() {
    if (!editingPolicy) {
      return;
    }

    setSaving(true);

    try {
      await updatePolicy({
        ...editingPolicy,
        ...formData,
      });

      window.location.reload();
    } finally {
      setSaving(false);
    }
  }

  async function handleDeletePolicy(
    policy: Policy,
  ) {
    if (
      !confirm(
        `Delete policy "${policy.name}"?`,
      )
    ) {
      return;
    }

    await deletePolicy(policy.id);

    window.location.reload();
  }

  async function handleDialogSave() {
    if (dialogMode === "create") {
      await handleCreatePolicy();
    } else {
      await handleUpdatePolicy();
    }
  }

  return (
    <div className="space-y-6">

      <div className="flex items-center justify-between">

        <div>

          <h1 className="text-3xl font-bold">
            Network Access Policies
          </h1>

          <p className="text-slate-500">
            Manage RADIUS access policies.
          </p>

        </div>

        <button
          onClick={() => {
            setDialogMode("create");
            setEditingPolicy(null);
            setFormData(defaultPolicyForm);
            setDialogOpen(true);
          }}
          className="flex items-center gap-2 rounded-lg bg-slate-900 px-4 py-2 text-white hover:bg-slate-800"
        >
          <Plus size={18} />
          Add Policy
        </button>

      </div>

      <div className="rounded-xl border bg-white p-6 shadow-sm">

        <p>Total Policies</p>

        <p className="mt-2 text-4xl font-bold">
          {policies.length}
        </p>

      </div>

      <PolicyTable
        policies={policies}
        onEdit={handleEditPolicy}
        onDelete={handleDeletePolicy}
      />

      <PolicyDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        title={
          dialogMode === "create"
            ? "Create Policy"
            : "Edit Policy"
        }
        policy={formData}
        onChange={setFormData}
        onSave={handleDialogSave}
        saving={saving}
        saveLabel={
          dialogMode === "create"
            ? "Create Policy"
            : "Save Changes"
        }
      />

    </div>
  );
}
