import { Category } from '../types/category';
import { categoryService } from './categoryService';

const CACHE_TTL_MS = 5 * 60 * 1000; // 5 minutes

let cache: Category[] | null = null;
let cacheTimestamp = 0;
let pendingRequest: Promise<Category[]> | null = null;

const isStale = () => !cache || Date.now() - cacheTimestamp > CACHE_TTL_MS;

/**
 * Fetches all categories, caching the result for CACHE_TTL_MS.
 * Concurrent calls while a fetch is in-flight share the same request.
 * Pass `force: true` to bypass the cache (e.g. after creating/updating a category).
 */
export async function getCachedCategories(force = false): Promise<Category[]> {
  if (!force && !isStale()) {
    return cache as Category[];
  }

  if (!force && pendingRequest) {
    return pendingRequest;
  }

  pendingRequest = categoryService
    .getAll(1, 1000)
    .then(result => {
      cache = result.items;
      cacheTimestamp = Date.now();
      return cache;
    })
    .finally(() => {
      pendingRequest = null;
    });

  return pendingRequest;
}

/** Clears the cached categories, forcing the next call to refetch. */
export function invalidateCategoryCache(): void {
  cache = null;
  cacheTimestamp = 0;
}
