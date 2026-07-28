/**
 * ================================================================
 * BDIP - Basarnas Digital Identity Platform
 * ================================================================
 *
 * File        : UserToolbar.tsx
 * Module      : Users
 * Description : Toolbar component for the Users page.
 *
 * Project     : Project Garuda
 * Version     : v0.1 Alpha
 *
 * Copyright (c) 2026 BASARNAS
 * ================================================================
 */

"use client";

import { FileUp, Plus } from "lucide-react";

import SearchToolbar from "@/components/common/data/SearchToolbar";
import { Button } from "@/components/ui/button";

interface UserToolbarProps {
  keyword: string;

  loading?: boolean;

  onKeywordChange: (value: string) => void;

  onRefresh?: () => void;

  onCreateUser?: () => void;

  onImportCsv?: () => void;
}

export default function UserToolbar({
  keyword,
  loading = false,
  onKeywordChange,
  onRefresh,
  onCreateUser,
  onImportCsv,
}: UserToolbarProps) {
  return (
    <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
      <SearchToolbar
        value={keyword}
        loading={loading}
        placeholder="Search username, full name, email..."
        onChange={onKeywordChange}
        onRefresh={onRefresh}
      />

      <div className="flex items-center gap-2">
        <Button
          type="button"
          variant="outline"
          onClick={onImportCsv}
        >
          <FileUp className="mr-2 h-4 w-4" />

          Import CSV
        </Button>

        <Button
          type="button"
          onClick={onCreateUser}
        >
          <Plus className="mr-2 h-4 w-4" />

          Create User
        </Button>
      </div>
    </div>
  );
}
