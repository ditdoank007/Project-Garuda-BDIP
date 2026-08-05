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
}

export default function UserPolicySelector({
  user,
  policies,
  initialPolicyId,
}: UserPolicySelectorProps) {
  const [selectedPolicy, setSelectedPolicy] =
    useState(initialPolicyId ?? "");

  const [saving, setSaving] = useState(false);

  useEffect(() => {
    setSelectedPolicy(initialPolicyId ?? "");
  }, [initialPolicyId]);

  useEffect(() => {
    console.log("UserPolicySelector", {
      user: user.username,
      policies,
      initialPolicyId,
      selectedPolicy,
    });
  }, [
    user.username,
    policies,
    initialPolicyId,
    selectedPolicy,
  ]);

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
        className="h-8 rounded-md border bg-background px-2 text-sm"
        value={selectedPolicy}
        onChange={(e) =>
          setSelectedPolicy(e.target.value)
        }
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
        disabled={saving}
      >
        {saving ? "Saving..." : "Save"}
      </Button>
    </div>
  );
}
