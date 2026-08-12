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

  function displayNumber(value: number | null | undefined) {
    return value === 0 || value == null
      ? ""
      : String(value);
  }

  function displayMbps(value: number | null | undefined) {
    return value === 0 || value == null
      ? ""
      : String(value / 1000);
  }

  function parseNumber(value: string) {
    return value === "" ? 0 : Number(value);
  }

  function parseMbps(value: string) {
    return value === "" ? 0 : Number(value) * 1000;
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
            value={displayNumber(policy.priority)}
            disabled={readOnly}
            onChange={(e) =>
              update("priority", parseNumber(e.target.value))
            }
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
            value={displayNumber(policy.simultaneousUse ?? 1)}
            disabled={readOnly}
            onChange={(e) =>
              update(
                "simultaneousUse",
                parseNumber(e.target.value),
              )
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
          <Label>Download Rate (Mbps)</Label>
          <Input
            type="number"
            step="0.1"
            min="0"
            value={displayMbps(policy.downloadRate)}
            disabled={readOnly}
            onChange={(e) =>
              update("downloadRate", parseMbps(e.target.value))
            }
          />
        </div>

        <div>
          <Label>Upload Rate (Mbps)</Label>
          <Input
            type="number"
            step="0.1"
            min="0"
            value={displayMbps(policy.uploadRate)}
            disabled={readOnly}
            onChange={(e) =>
              update("uploadRate", parseMbps(e.target.value))
            }
          />
        </div>

        <div>
          <Label>Burst Download (Mbps)</Label>
          <Input
            type="number"
            step="0.1"
            min="0"
            value={displayMbps(policy.burstDownload)}
            disabled={readOnly}
            onChange={(e) =>
              update("burstDownload", parseMbps(e.target.value))
            }
          />
        </div>

        <div>
          <Label>Burst Upload (Mbps)</Label>
          <Input
            type="number"
            step="0.1"
            min="0"
            value={displayMbps(policy.burstUpload)}
            disabled={readOnly}
            onChange={(e) =>
              update("burstUpload", parseMbps(e.target.value))
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
            value={displayNumber(policy.sessionTimeout)}
            disabled={readOnly}
            onChange={(e) =>
              update("sessionTimeout", parseNumber(e.target.value))
            }
          />
        </div>

        <div>
          <Label>Idle Timeout (Seconds)</Label>
          <Input
            type="number"
            value={displayNumber(policy.idleTimeout)}
            disabled={readOnly}
            onChange={(e) =>
              update("idleTimeout", parseNumber(e.target.value))
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
            value={displayNumber(policy.dailyQuota)}
            disabled={readOnly}
            onChange={(e) =>
              update("dailyQuota", parseNumber(e.target.value))
            }
          />
        </div>

        <div>
          <Label>Monthly Quota (MB)</Label>
          <Input
            type="number"
            value={displayNumber(policy.monthlyQuota)}
            disabled={readOnly}
            onChange={(e) =>
              update("monthlyQuota", parseNumber(e.target.value))
            }
          />
        </div>

        <div>
          <Label>Total Quota (MB)</Label>
          <Input
            type="number"
            value={displayNumber(policy.totalQuota)}
            disabled={readOnly}
            onChange={(e) =>
              update("totalQuota", parseNumber(e.target.value))
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