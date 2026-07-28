"use client";

import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

import type { PolicyFormData } from "@/types/policy";

interface Props {
  policy: PolicyFormData;
  readOnly?: boolean;
  onChange?: (
    policy: PolicyFormData,
  ) => void;
}

export default function PolicyForm({
  policy,
  readOnly = false,
  onChange,
}: Props) {

  function update<K extends keyof PolicyFormData>(
    key: K,
    value: PolicyFormData[K],
  ) {
    onChange?.({
      ...policy,
      [key]: value,
    });
  }

  return (
    <div className="space-y-4">

      <div>
        <Label>Code</Label>

        <Input
          value={policy.code}
          disabled={readOnly}
          onChange={(e) =>
            update("code", e.target.value)
          }
        />
      </div>

      <div>
        <Label>Name</Label>

        <Input
          value={policy.name}
          disabled={readOnly}
          onChange={(e) =>
            update("name", e.target.value)
          }
        />
      </div>

      <div>
        <Label>Description</Label>

        <Input
          value={policy.description ?? ""}
          disabled={readOnly}
          onChange={(e) =>
            update("description", e.target.value)
          }
        />
      </div>

      <div>
        <Label>Priority</Label>

        <Input
          type="number"
          value={policy.priority}
          disabled={readOnly}
          onChange={(e) =>
            update(
              "priority",
              Number(e.target.value),
            )
          }
        />
      </div>

      <div className="flex items-center gap-3">

        <input
          id="enabled"
          type="checkbox"
          checked={policy.enabled}
          disabled={readOnly}
          onChange={(e) =>
            update(
              "enabled",
              e.target.checked,
            )
          }
        />

        <Label htmlFor="enabled">
          Enabled
        </Label>

      </div>

    </div>
  );
}
