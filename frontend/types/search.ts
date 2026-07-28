/**
 * ================================================================
 * BDIP - Basarnas Digital Identity Platform
 * ================================================================
 *
 * File        : search.ts
 * Module      : Search
 * Description : Generic search contracts used across BDIP.
 *
 * Project     : Project Garuda
 * Version     : v0.1 Alpha
 *
 * Copyright (c) 2026 BASARNAS
 * ================================================================
 */

export interface SearchEntity {
  id: string;
}

export interface SearchRequest {
  keyword: string;
  page?: number;
  limit?: number;
}

export interface SearchResult<T extends SearchEntity> {
  items: T[];
  total: number;
}