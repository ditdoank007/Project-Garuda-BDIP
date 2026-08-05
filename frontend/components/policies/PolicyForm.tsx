"use client";

import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

import type { PolicyFormData } from "@/types/policy";

interface Props {
  policy: PolicyFormData;
  readOnly?: boolean;
  onChange?: (policy: PolicyFormData) => void;
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
    <div className="space-y-6">

      <section className="space-y-4">
        <h3 className="font-semibold text-lg">General</h3>

        <div>
          <Label>Code</Label>
          <Input
            value={policy.code}
            disabled={readOnly}
            onChange={(e) => update("code", e.target.value)}
          />
        </div>

        <div>
          <Label>Name</Label>
          <Input
            value={policy.name}
            disabled={readOnly}
            onChange={(e) => update("name", e.target.value)}
          />
        </div>

        <div>
          <Label>Description</Label>
          <Input
            value={policy.description ?? ""}
            disabled={readOnly}
            onChange={(e) => update("description", e.target.value)}
          />
        </div>

        <div>
          <Label>Priority</Label>
          <Input
            type="number"
            value={policy.priority}
            disabled={readOnly}
            onChange={(e) => update("priority", Number(e.target.value))}
          />
        </div>

        <div className="flex items-center gap-3">
          <input
            id="enabled"
            type="checkbox"
            checked={policy.enabled}
            disabled={readOnly}
            onChange={(e) => update("enabled", e.target.checked)}
          />
          <Label htmlFor="enabled">Enabled</Label>
        </div>
      </section>

      <section className="space-y-4">
        <h3 className="font-semibold text-lg">Authentication</h3>

        <div>
          <Label>Simultaneous Use</Label>
          <Input
            type="number"
            value={policy.simultaneousUse ?? 1}
            disabled={readOnly}
            onChange={(e) =>
              update("simultaneousUse", Number(e.target.value))
            }
          />
        </div>

        <div>
          <Label>Expiration Date</Label>
          <Input
            type="datetime-local"
            value={policy.expirationDate ?? ""}
            disabled={readOnly}
            onChange={(e) =>
              update("expirationDate", e.target.value)
            }
          />
        </div>

        <div>
          <Label>Login Schedule</Label>
          <Input
            value={policy.loginSchedule ?? ""}
            disabled={readOnly}
            onChange={(e) =>
              update("loginSchedule", e.target.value)
            }
          />
        </div>
      </section>

      <section className="space-y-4">
        <h3 className="font-semibold text-lg">Bandwidth</h3>

        <div>
          <Label>Download Rate (Kbps)</Label>
          <Input
            type="number"
            value={policy.downloadRate ?? 0}
            disabled={readOnly}
            onChange={(e) =>
              update("downloadRate", Number(e.target.value))
            }
          />
        </div>

        <div>
          <Label>Upload Rate (Kbps)</Label>
          <Input
            type="number"
            value={policy.uploadRate ?? 0}
            disabled={readOnly}
            onChange={(e) =>
              update("uploadRate", Number(e.target.value))
            }
          />
        </div>

        <div>
          <Label>Burst Download</Label>
          <Input
            type="number"
            value={policy.burstDownload ?? 0}
            disabled={readOnly}
            onChange={(e) =>
              update("burstDownload", Number(e.target.value))
            }
          />
        </div>

        <div>
          <Label>Burst Upload</Label>
          <Input
            type="number"
            value={policy.burstUpload ?? 0}
            disabled={readOnly}
            onChange={(e) =>
              update("burstUpload", Number(e.target.value))
            }
          />
        </div>
      </section>

      <section className="space-y-4">
        <h3 className="font-semibold text-lg">Session</h3>

        <div>
          <Label>Session Timeout (Seconds)</Label>
          <Input
            type="number"
            value={policy.sessionTimeout ?? 0}
            disabled={readOnly}
            onChange={(e) =>
              update("sessionTimeout", Number(e.target.value))
            }
          />
        </div>

        <div>
          <Label>Idle Timeout (Seconds)</Label>
          <Input
            type="number"
            value={policy.idleTimeout ?? 0}
            disabled={readOnly}
            onChange={(e) =>
              update("idleTimeout", Number(e.target.value))
            }
          />
        </div>
      </section>

            <section className="space-y-4">
        <h3 className="font-semibold text-lg">Quota</h3>

        <div>
          <Label>Daily Quota (MB)</Label>
          <Input
            type="number"
            value={policy.dailyQuota ?? 0}
            disabled={readOnly}
            onChange={(e) =>
              update("dailyQuota", Number(e.target.value))
            }
          />
        </div>

        <div>
          <Label>Monthly Quota (MB)</Label>
          <Input
            type="number"
            value={policy.monthlyQuota ?? 0}
            disabled={readOnly}
            onChange={(e) =>
              update("monthlyQuota", Number(e.target.value))
            }
          />
        </div>

        <div>
          <Label>Total Quota (MB)</Label>
          <Input
            type="number"
            value={policy.totalQuota ?? 0}
            disabled={readOnly}
            onChange={(e) =>
              update("totalQuota", Number(e.target.value))
            }
          />
        </div>
      </section>

      <section className="space-y-4">
        <h3 className="font-semibold text-lg">
          Network
        </h3>

      <div>
        <Label>Address List</Label>
        <Input
          value={policy.addressList ?? ""}
          disabled={readOnly}
          onChange={(e) =>
            update("addressList", e.target.value)
          }
        />
      </div>

      <div>
        <Label>VLAN ID</Label>
        <Input
          type="number"
          value={policy.vlanId ?? 0}
          disabled={readOnly}
          onChange={(e) =>
            update("vlanId", Number(e.target.value))
          }
        />
      </div>

      <div>
        <Label>IP Pool</Label>
        <Input
          value={policy.ipPool ?? ""}
          disabled={readOnly}
          onChange={(e) =>
            update("ipPool", e.target.value)
          }
        />
      </div>
    </section>

    </div>
  );
}