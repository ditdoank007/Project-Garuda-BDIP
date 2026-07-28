"use client";

import {
  Building2,
  Pencil,
  Plus,
  Search,
  Trash2,
  Users,
} from "lucide-react";
import { useMemo, useState } from "react";
import { toast } from "sonner";

import {
  createUnit,
  deleteUnit,
  updateUnit,
} from "@/services/unit.service";

import type { Unit } from "@/types/unit";

import DeleteUnitDialog from "./DeleteUnitDialog";
import UnitFormDialog from "./UnitFormDialog";

type UnitsClientProps = {
  initialUnits: Unit[];
};

export default function UnitsClient({
  initialUnits,
}: UnitsClientProps) {
  const [units, setUnits] =
    useState<Unit[]>(initialUnits);

  const [search, setSearch] = useState("");

  const [formOpen, setFormOpen] =
    useState(false);

  const [selectedUnit, setSelectedUnit] =
    useState<Unit | null>(null);

  const [deleteTarget, setDeleteTarget] =
    useState<Unit | null>(null);

  const [loading, setLoading] =
    useState(false);

  const filteredUnits = useMemo(() => {
    const keyword = search
      .trim()
      .toLowerCase();

    if (!keyword) {
      return units;
    }

    return units.filter((unit) => {
      return (
        unit.name.toLowerCase().includes(keyword) ||
        unit.description
          .toLowerCase()
          .includes(keyword)
      );
    });
  }, [search, units]);

  const totalUsers = useMemo(() => {
    return units.reduce(
      (total, unit) =>
        total + unit.userCount,
      0
    );
  }, [units]);

  function openCreate() {
    setSelectedUnit(null);
    setFormOpen(true);
  }

  function openEdit(unit: Unit) {
    setSelectedUnit(unit);
    setFormOpen(true);
  }

  async function handleSubmit(data: {
    name: string;
    description: string;
  }) {
    setLoading(true);

    try {
      if (selectedUnit) {
        const updated = await updateUnit(
          selectedUnit.name,
          data
        );

        setUnits((current) =>
          current
            .map((unit) =>
              unit.name === selectedUnit.name
                ? updated
                : unit
            )
            .sort((a, b) =>
              a.name.localeCompare(b.name)
            )
        );

        toast.success(
          "Unit berhasil diperbarui."
        );
      } else {
        const created =
          await createUnit(data);

        setUnits((current) =>
          [...current, created].sort((a, b) =>
            a.name.localeCompare(b.name)
          )
        );

        toast.success(
          "Unit berhasil ditambahkan."
        );
      }

      setFormOpen(false);
      setSelectedUnit(null);
    } catch (error) {
      toast.error(
        error instanceof Error
          ? error.message
          : "Gagal menyimpan unit."
      );
    } finally {
      setLoading(false);
    }
  }

  async function handleDelete() {
    if (!deleteTarget) {
      return;
    }

    setLoading(true);

    try {
      await deleteUnit(deleteTarget.name);

      setUnits((current) =>
        current.filter(
          (unit) =>
            unit.name !== deleteTarget.name
        )
      );

      toast.success(
        "Unit berhasil dihapus."
      );

      setDeleteTarget(null);
    } catch (error) {
      toast.error(
        error instanceof Error
          ? error.message
          : "Gagal menghapus unit."
      );
    } finally {
      setLoading(false);
    }
  }

  return (
    <>
      <div className="space-y-6">
        <div className="flex flex-col gap-4 xl:flex-row xl:items-center xl:justify-between">
          <div>
            <h1 className="text-3xl font-bold tracking-tight text-slate-900">
              Units
            </h1>

            <p className="mt-1 text-sm text-slate-500">
              Manage organizational units and user
              assignments.
            </p>
          </div>

          <button
            type="button"
            onClick={openCreate}
            className="inline-flex items-center justify-center gap-2 rounded-xl bg-blue-600 px-5 py-3 text-sm font-medium text-white shadow-sm transition hover:bg-blue-700"
          >
            <Plus size={18} />
            Add Unit
          </button>
        </div>

        <div className="grid gap-4 md:grid-cols-2">
          <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-500">
                  Organizational Units
                </p>

                <p className="mt-2 text-3xl font-bold text-slate-900">
                  {units.length}
                </p>
              </div>

              <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-blue-100 text-blue-600">
                <Building2 size={24} />
              </div>
            </div>
          </div>

          <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-500">
                  Assigned Users
                </p>

                <p className="mt-2 text-3xl font-bold text-slate-900">
                  {totalUsers}
                </p>
              </div>

              <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-emerald-100 text-emerald-600">
                <Users size={24} />
              </div>
            </div>
          </div>
        </div>

        <div className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
          <div className="flex flex-col gap-4 border-b border-slate-200 p-5 md:flex-row md:items-center md:justify-between">
            <div>
              <h2 className="text-lg font-semibold text-slate-900">
                Organizational Units
              </h2>

              <p className="mt-1 text-sm text-slate-500">
                LDAP organizational structure managed
                by BDIP.
              </p>
            </div>

            <div className="relative w-full md:w-80">
              <Search
                size={18}
                className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
              />

              <input
                type="search"
                value={search}
                onChange={(event) =>
                  setSearch(event.target.value)
                }
                placeholder="Search units..."
                className="w-full rounded-xl border border-slate-300 py-2.5 pl-10 pr-4 text-sm text-slate-900 outline-none transition focus:border-blue-500 focus:ring-4 focus:ring-blue-100"
              />
            </div>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-slate-50">
                <tr className="border-b border-slate-200">
                  <th className="px-6 py-4 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
                    Unit
                  </th>

                  <th className="px-6 py-4 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
                    Description
                  </th>

                  <th className="px-6 py-4 text-center text-xs font-semibold uppercase tracking-wider text-slate-500">
                    Users
                  </th>

                  <th className="px-6 py-4 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">
                    Actions
                  </th>
                </tr>
              </thead>

              <tbody className="divide-y divide-slate-100">
                {filteredUnits.map((unit) => (
                  <tr
                    key={unit.name}
                    className="transition hover:bg-slate-50"
                  >
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-3">
                        <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-blue-100 text-blue-600">
                          <Building2 size={19} />
                        </div>

                        <div>
                          <p className="font-medium text-slate-900">
                            {unit.name}
                          </p>

                          <p className="text-xs text-slate-500">
                            LDAP Unit
                          </p>
                        </div>
                      </div>
                    </td>

                    <td className="px-6 py-4 text-sm text-slate-600">
                      {unit.description || "-"}
                    </td>

                    <td className="px-6 py-4 text-center">
                      <span className="inline-flex min-w-10 items-center justify-center rounded-full bg-slate-100 px-3 py-1 text-sm font-medium text-slate-700">
                        {unit.userCount}
                      </span>
                    </td>

                    <td className="px-6 py-4">
                      <div className="flex justify-end gap-2">
                        <button
                          type="button"
                          onClick={() =>
                            openEdit(unit)
                          }
                          title="Edit unit"
                          className="rounded-lg p-2 text-slate-500 transition hover:bg-blue-50 hover:text-blue-600"
                        >
                          <Pencil size={17} />
                        </button>

                        <button
                          type="button"
                          onClick={() =>
                            setDeleteTarget(unit)
                          }
                          title="Delete unit"
                          className="rounded-lg p-2 text-slate-500 transition hover:bg-red-50 hover:text-red-600"
                        >
                          <Trash2 size={17} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}

                {filteredUnits.length === 0 && (
                  <tr>
                    <td
                      colSpan={4}
                      className="px-6 py-16 text-center"
                    >
                      <Building2
                        size={36}
                        className="mx-auto text-slate-300"
                      />

                      <p className="mt-4 font-medium text-slate-700">
                        No units found
                      </p>

                      <p className="mt-1 text-sm text-slate-500">
                        Try another search keyword.
                      </p>
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <UnitFormDialog
        open={formOpen}
        unit={selectedUnit}
        loading={loading}
        onClose={() => {
          if (!loading) {
            setFormOpen(false);
            setSelectedUnit(null);
          }
        }}
        onSubmit={handleSubmit}
      />

      <DeleteUnitDialog
        unit={deleteTarget}
        loading={loading}
        onClose={() => {
          if (!loading) {
            setDeleteTarget(null);
          }
        }}
        onConfirm={handleDelete}
      />
    </>
  );
}
