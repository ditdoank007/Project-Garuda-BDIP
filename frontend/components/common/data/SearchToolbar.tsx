/**
 * ================================================================
 * BDIP - Basarnas Digital Identity Platform
 * ================================================================
 *
 * File        : SearchToolbar.tsx
 * Module      : Common/Data
 * Description : Generic search toolbar for BDIP modules.
 *
 * Project     : Project Garuda
 * Version     : v0.1 Alpha
 *
 * Copyright (c) 2026 BASARNAS
 * ================================================================
 */

"use client";

import { Search, RefreshCcw } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

export interface SearchToolbarProps {
  placeholder?: string;

  value: string;

  loading?: boolean;

  disabled?: boolean;

  onChange: (value: string) => void;

  onRefresh?: () => void;
}

export default function SearchToolbar({
  placeholder = "Search...",
  value,
  loading = false,
  disabled = false,
  onChange,
  onRefresh,
}: SearchToolbarProps) {
  return (
    <div className="flex items-center justify-between gap-4">
      <div className="relative w-full max-w-md">
        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />

        <Input
          value={value}
          disabled={disabled}
          placeholder={placeholder}
          onChange={(e) => onChange(e.target.value)}
          className="pl-10"
        />
      </div>

      {onRefresh && (
        <Button
          type="button"
          variant="outline"
          onClick={onRefresh}
          disabled={loading}
        >
          <RefreshCcw
            className={`mr-2 h-4 w-4 ${
              loading ? "animate-spin" : ""
            }`}
          />

          Refresh
        </Button>
      )}
    </div>
  );
}