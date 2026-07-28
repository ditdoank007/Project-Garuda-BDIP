"use client";

import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";

import type { Unit } from "@/types/unit";
import type { UserFormData } from "@/types/users";

interface UserFormProps {
  user: UserFormData;
  units: Unit[];
  readOnly?: boolean;
  onChange?: (user: UserFormData) => void;
  showPasswordFields?: boolean;
  usernameReadOnly?: boolean;
}

export default function UserForm({
  user,
  units,
  readOnly = false,
  onChange,
  showPasswordFields = true,
  usernameReadOnly = false,
}: UserFormProps) {
  function updateField(
    field: keyof UserFormData,
    value: string | boolean,
  ) {
    if (!onChange) return;

    onChange({
      ...user,
      [field]: value,
    });
  }

  const inputReadOnly = readOnly || usernameReadOnly;

  return (
    <div className="grid grid-cols-2 gap-4 py-4">
      <div className="space-y-2">
        <Label>Username</Label>
        <Input
          value={user.username}
          readOnly={inputReadOnly}
          placeholder="Username"
          onChange={(e) =>
            updateField("username", e.target.value)
          }
        />
      </div>

      <div className="space-y-2">
        <Label>Full Name</Label>
        <Input
          value={user.fullName}
          readOnly={readOnly}
          placeholder="Full Name"
          onChange={(e) =>
            updateField("fullName", e.target.value)
          }
        />
      </div>

      <div className="space-y-2">
        <Label>Email</Label>
        <Input
          type="email"
          value={user.email}
          readOnly={readOnly}
          placeholder="Email"
          onChange={(e) =>
            updateField("email", e.target.value)
          }
        />
      </div>

      <div className="space-y-2">
        <Label>Unit</Label>

        {readOnly ? (
          <Input
            value={user.unit}
            readOnly
            placeholder="No unit assigned"
          />
        ) : (
          <select
            value={user.unit}
            onChange={(e) =>
              updateField("unit", e.target.value)
            }
            className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-xs transition-colors outline-none focus-visible:border-ring focus-visible:ring-[3px] focus-visible:ring-ring/50"
          >
            <option value="">No unit assigned</option>

            {units.map((unit) => (
              <option
                key={unit.name}
                value={unit.name}
              >
                {unit.name}
              </option>
            ))}
          </select>
        )}
      </div>

      {!readOnly && showPasswordFields && (
        <>
          <div className="space-y-2">
            <Label>Password</Label>
            <Input
              type="password"
              value={user.password}
              placeholder="Password"
              onChange={(e) =>
                updateField("password", e.target.value)
              }
            />
          </div>

          <div className="space-y-2">
            <Label>Confirm Password</Label>
            <Input
              type="password"
              value={user.confirmPassword}
              placeholder="Confirm Password"
              onChange={(e) =>
                updateField("confirmPassword", e.target.value)
              }
            />
          </div>
        </>
      )}

      {!readOnly && (
        <div className="col-span-2 flex items-center gap-3">
          <Checkbox
            checked={user.enabled}
            onCheckedChange={(checked) =>
              updateField("enabled", checked === true)
            }
          />

          <Label>Active User</Label>
        </div>
      )}
    </div>
  );
}
