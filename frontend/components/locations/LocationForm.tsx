import type { LocationFormData } from "@/types/location";

interface Props {
  value: LocationFormData;
  onChange: (value: LocationFormData) => void;
  readOnlyName?: boolean;
}

const LOCATION_TYPES = [
  "Kantor Pusat",
  "Balai Diklat",
  "UPT",
];

export default function LocationForm({
  value,
  onChange,
  readOnlyName = false,
}: Props) {
  function update<K extends keyof LocationFormData>(
    key: K,
    val: LocationFormData[K],
  ) {
    onChange({
      ...value,
      [key]: val,
    });
  }

  return (
    <div className="space-y-5">

      <div>

        <label className="mb-2 block text-sm font-medium">
          Name
        </label>

        <input
          type="text"
          value={value.name}
          readOnly={readOnlyName}
          onChange={(e) =>
            update("name", e.target.value)
          }
          className="w-full rounded-lg border px-3 py-2"
        />

      </div>

      <div>

        <label className="mb-2 block text-sm font-medium">
          Type
        </label>

        <select
          value={value.type}
          onChange={(e) =>
            update("type", e.target.value)
          }
          className="w-full rounded-lg border px-3 py-2"
        >

          <option value="">
            -- Select Type --
          </option>

          {LOCATION_TYPES.map((type) => (

            <option
              key={type}
              value={type}
            >
              {type}
            </option>

          ))}

        </select>

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

    </div>
  );
}