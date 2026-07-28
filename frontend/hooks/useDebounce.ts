/**
 * ================================================================
 * BDIP - Basarnas Digital Identity Platform
 * ================================================================
 *
 * File        : useDebounce.ts
 * Module      : Hooks
 * Description : Generic debounce hook for client-side search.
 *
 * Project     : Project Garuda
 * Version     : v0.1 Alpha
 *
 * Copyright (c) 2026 BASARNAS
 * ================================================================
 */

"use client";

import { useEffect, useState } from "react";

export default function useDebounce<T>(
  value: T,
  delay = 300,
): T {
  const [debouncedValue, setDebouncedValue] =
    useState(value);

  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);

    return () => clearTimeout(timer);
  }, [value, delay]);

  return debouncedValue;
}