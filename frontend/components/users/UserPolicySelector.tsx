"use client";

import { useEffect, useState } from "react";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { updateUserPolicy } from "@/services/policy.service";

import type { Policy } from "@/types/policy";
import type { User } from "@/types/users";

interface UserPolicySelectorProps {
  user: User;
  policies: Policy[];
  initialPolicyId?: string;
  readOnly?: boolean;
}

export default function UserPolicySelector({
  user,
  policies,
  initialPolicyId,
  readOnly = false,
}: UserPolicySelectorProps) {
  const [selectedPolicy, setSelectedPolicy] =
    useState(initialPolicyId ?? "");

  const [saving, setSaving] = useState(false);

  useEffect(() => {
    setSelectedPolicy(initialPolicyId ?? "");
  }, [initialPolicyId]);

  async function handleSave() {
    if (!selectedPolicy) {
      toast.error("Please select a policy.");
      return;
    }

    try {
      setSaving(true);

      await updateUserPolicy(user.uid, {
        policyId: selectedPolicy,
      });

      toast.success(
        `Policy updated for ${user.username}.`,
      );
    } catch (error) {
      console.error(error);

      toast.error(
        "Failed to update NAP policy.",
      );
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="flex items-center gap-2">
      <select
        className={`h-8 rounded-md border bg-background px-2 text-sm ${
          selectedPolicy
            ? "bdip-data-complete"
            : "bdip-data-incomplete"
        }`}
        value={selectedPolicy}
        onChange={(e) =>
          setSelectedPolicy(e.target.value)
        }
        disabled={readOnly}
      >
        <option value="">
          -- Select Policy --
        </option>

        {policies.map((policy) => (
          <option
            key={policy.id}
            value={policy.id}
          >
            {policy.name}
          </option>
        ))}
      </select>

      <Button
        size="sm"
        onClick={handleSave}
        disabled={saving || readOnly}
      >
        {saving ? "Saving..." : "Save"}
      </Button>
    </div>
  );
}
