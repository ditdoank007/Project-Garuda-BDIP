"use client";

import type {
  FingerMachineFormData,
} from "@/types/finger-machine";

interface Props {
  value: FingerMachineFormData;
  locations: {
    id: string;
    name: string;
  }[];
  onChange: (
    value: FingerMachineFormData,
  ) => void;
  readOnlyCode?: boolean;
}

export default function FingerMachineForm({
  value,
  locations,
  onChange,
  readOnlyCode = false,
}: Props) {
  function update(
    field: keyof FingerMachineFormData,
    fieldValue: string | number | boolean | null,
  ) {
    onChange({
      ...value,
      [field]: fieldValue,
    });
  }

  return (
    <div className="grid gap-4">

      <div>
        <label className="mb-2 block text-sm font-medium">
          Code Mesin
        </label>

        <input
          value={value.code}
          onChange={(e) =>
            update("code", e.target.value)
          }
          disabled={readOnlyCode}
          placeholder="Contoh: FINGER-001"
          className="w-full rounded-lg border px-3 py-2 text-sm disabled:bg-slate-100"
        />
      </div>

      <div>
        <label className="mb-2 block text-sm font-medium">
          Nama Mesin
        </label>

        <input
          value={value.name}
          onChange={(e) =>
            update("name", e.target.value)
          }
          placeholder="Nama mesin finger"
          className="w-full rounded-lg border px-3 py-2 text-sm"
        />
      </div>

      <div>
        <label className="mb-2 block text-sm font-medium">
          Lokasi
        </label>

        <select
          value={value.locationId ?? ""}
          onChange={(e) =>
            update(
              "locationId",
              e.target.value || null,
            )
          }
          className="w-full rounded-lg border px-3 py-2 text-sm"
        >
          <option value="">
            Pilih lokasi
          </option>

          {locations.map((location) => (
            <option
              key={location.id}
              value={location.id}
            >
              {location.name}
            </option>
          ))}
        </select>
      </div>

      <div className="grid gap-4 sm:grid-cols-2">

        <div>
          <label className="mb-2 block text-sm font-medium">
            IP Address
          </label>

          <input
            value={value.ipAddress}
            onChange={(e) =>
              update(
                "ipAddress",
                e.target.value,
              )
            }
            placeholder="192.168.1.100"
            className="w-full rounded-lg border px-3 py-2 text-sm"
          />
        </div>

        <div>
          <label className="mb-2 block text-sm font-medium">
            Port
          </label>

          <input
            type="number"
            value={value.port}
            onChange={(e) =>
              update(
                "port",
                Number(e.target.value),
              )
            }
            className="w-full rounded-lg border px-3 py-2 text-sm"
          />
        </div>

      </div>

      <div className="grid gap-4 sm:grid-cols-2">

        <div>
          <label className="mb-2 block text-sm font-medium">
            Serial Number
          </label>

          <input
            value={value.serialNumber}
            onChange={(e) =>
              update(
                "serialNumber",
                e.target.value,
              )
            }
            placeholder="Serial number mesin"
            className="w-full rounded-lg border px-3 py-2 text-sm"
          />
        </div>

        <div>
          <label className="mb-2 block text-sm font-medium">
            Device Name
          </label>

          <input
            value={value.deviceName}
            onChange={(e) =>
              update(
                "deviceName",
                e.target.value,
              )
            }
            placeholder="Nama device"
            className="w-full rounded-lg border px-3 py-2 text-sm"
          />
        </div>

      </div>

      <label className="flex items-center gap-3 text-sm">
        <input
          type="checkbox"
          checked={value.isActive}
          onChange={(e) =>
            update(
              "isActive",
              e.target.checked,
            )
          }
        />

        <span>
          Mesin Aktif
        </span>
      </label>

    </div>
  );
}
