import type {
  SearchEntity,
  SearchRequest,
  SearchResult,
} from "@/types/search";

/**
 * Generic Search Service Contract
 *
 * Every searchable module must implement this contract.
 */
export interface SearchService<T extends SearchEntity> {
  search(request: SearchRequest): Promise<SearchResult<T>>;
}