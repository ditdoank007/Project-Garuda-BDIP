/**
 * ================================================================
 * BDIP - Basarnas Digital Identity Platform
 * ================================================================
 *
 * File        : useSmartSearch.ts
 * Module      : Hooks
 * Description : Generic client-side smart search hook.
 *
 * Project     : Project Garuda
 * Version     : v0.1 Alpha
 *
 * Copyright (c) 2026 BASARNAS
 * ================================================================
 */

"use client";

import { useMemo } from "react";

export default function useSmartSearch<T>(
  items: T[],
  keyword: string,
  fields: (keyof T)[]
): T[] {
  return useMemo(() => {
    const search = keyword.trim().toLowerCase();

    if (!search) {
      return items;
    }

    return items.filter((item) =>
      fields.some((field) => {
        const value = item[field];

        if (value === undefined || value === null) {
          return false;
        }

        return String(value)
          .toLowerCase()
          .includes(search);
      })
    );
  }, [items, keyword, fields]);
}