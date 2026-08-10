"use client";

import type { ApplicationFormData } from "@/services/application.service";

interface Props {
  value: ApplicationFormData;
  onChange: (value: ApplicationFormData) => void;
  readOnlyCode?: boolean;
}

export default function ApplicationForm({
  value,
  onChange,
  readOnlyCode = false,
}: Props) {
  function update<K extends keyof ApplicationFormData>(
    key: K,
    val: ApplicationFormData[K],
  ) {
    onChange({
      ...value,
      [key]: val,
    });
  }

  return (
    <div className="space-y-4">
      <div>
        <label className="mb-2 block text-sm font-medium">
          Code
        </label>

        <input
          type="text"
          value={value.code}
          readOnly={readOnlyCode}
          onChange={(e) =>
            update("code", e.target.value)
          }
          className="w-full rounded-lg border px-3 py-2"
        />
      </div>

      <div>
        <label className="mb-2 block text-sm font-medium">
          Name
        </label>

        <input
          type="text"
          value={value.name}
          onChange={(e) =>
            update("name", e.target.value)
          }
          className="w-full rounded-lg border px-3 py-2"
        />
      </div>

      <div>
        <label className="mb-2 block text-sm font-medium">
          Description
        </label>

        <textarea
          rows={3}
          value={value.description}
          onChange={(e) =>
            update("description", e.target.value)
          }
          className="w-full rounded-lg border px-3 py-2"
        />
      </div>

      <div>
        <label className="mb-2 block text-sm font-medium">
          Base URL
        </label>

        <input
          type="url"
          value={value.baseUrl}
          onChange={(e) =>
            update("baseUrl", e.target.value)
          }
          placeholder="https://example.sarsurabaya.id"
          className="w-full rounded-lg border px-3 py-2"
        />
      </div>
    </div>
  );
}
