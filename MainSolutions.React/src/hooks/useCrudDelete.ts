import { useState } from 'react';
import { CrudService } from '../types/crud';

interface EntityBase {
  id: number;
}

interface UseCrudDeleteResult<T> {
  deleteTarget: T | null;
  deleting: boolean;
  requestDelete: (item: T) => void;
  cancelDelete: () => void;
  confirmDelete: () => Promise<void>;
}

/**
 * Encapsulates the delete-confirmation modal and the delete request itself,
 * including the "step back a page if this was the last item" rule.
 * Isolated so the deletion workflow can be reused or tested without the
 * rest of CrudPage (Single Responsibility Principle).
 */
export function useCrudDelete<T extends EntityBase, TForm extends Record<string, any>>(
  service: CrudService<T, TForm>,
  entityName: string,
  currentPageItemCount: number,
  page: number,
  onDeleted: (nextPage: number) => Promise<void>,
  onError: (message: string) => void,
): UseCrudDeleteResult<T> {
  const [deleteTarget, setDeleteTarget] = useState<T | null>(null);
  const [deleting, setDeleting] = useState(false);

  const requestDelete = (item: T) => setDeleteTarget(item);
  const cancelDelete = () => {
    if (!deleting) setDeleteTarget(null);
  };

  const confirmDelete = async () => {
    if (!deleteTarget) return;
    setDeleting(true);
    try {
      await service.delete(deleteTarget.id);
      setDeleteTarget(null);
      const isLastItemOnPage = currentPageItemCount === 1 && page > 1;
      await onDeleted(isLastItemOnPage ? page - 1 : page);
    } catch (err) {
      onError(err instanceof Error ? err.message : `Failed to delete ${entityName}.`);
      setDeleteTarget(null);
    } finally {
      setDeleting(false);
    }
  };

  return { deleteTarget, deleting, requestDelete, cancelDelete, confirmDelete };
}
