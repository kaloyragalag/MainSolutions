import { useEffect, useState } from 'react';
import { CrudService } from '../types/crud';

interface EntityBase {
  id: number;
}

interface UseCrudListResult<T> {
  items: T[];
  filteredItems: T[];
  loading: boolean;
  error: string | null;
  search: string;
  setSearch: (value: string) => void;
  page: number;
  totalPages: number;
  totalCount: number;
  goToPage: (target: number) => void;
  refetch: (targetPage?: number) => Promise<void>;
  selectedId: number | null;
  setSelectedId: (id: number | null) => void;
}

/**
 * Encapsulates fetching, pagination, search-filtering and selection state
 * for a paged list of entities. Keeping this separate from CrudPage means
 * the data-loading concern can be reused, tested, or swapped independently
 * of how the list is rendered (Single Responsibility Principle).
 */
export function useCrudList<T extends EntityBase, TForm extends Record<string, any>>(
  service: CrudService<T, TForm>,
  pageSize: number,
  pluralName: string,
  getItemTitle: (item: T) => string,
): UseCrudListResult<T> {
  const [items, setItems] = useState<T[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const [selectedId, setSelectedId] = useState<number | null>(null);

  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const refetch = async (targetPage: number = page) => {
    setLoading(true);
    setError(null);
    try {
      const result = await service.getAll(targetPage, pageSize);
      const list = result.items;
      setItems(list);
      setPage(result.page);
      setTotalPages(Math.max(result.totalPages, 1));
      setTotalCount(result.totalCount);
      if (list.length > 0) {
        setSelectedId((prev) =>
          prev && list.some((i) => i.id === prev) ? prev : list[0].id,
        );
      } else {
        setSelectedId(null);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : `Failed to load ${pluralName}.`);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    refetch(page);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page]);

  const goToPage = (target: number) => {
    if (target < 1 || target > totalPages || target === page) return;
    setPage(target);
  };

  const filteredItems = Array.isArray(items)
    ? items.filter((item) => getItemTitle(item).toLowerCase().includes(search.toLowerCase()))
    : [];

  return {
    items,
    filteredItems,
    loading,
    error,
    search,
    setSearch,
    page,
    totalPages,
    totalCount,
    goToPage,
    refetch,
    selectedId,
    setSelectedId,
  };
}
