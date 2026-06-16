import { useState, useCallback } from 'react';
import { EntityImage } from '../types/entityImage';
import { productService } from '../services/productService';

interface UseProductImagesResult {
  images: EntityImage[];
  loading: boolean;
  error: string | null;
  uploading: boolean;
  /** 0–100 while uploading; null when no upload is in progress. */
  uploadProgress: number | null;
  /** id of the image currently being deleted, or null if none. */
  deletingId: number | null;
  fetch: (productId: number) => Promise<void>;
  upload: (productId: number, files: File[]) => Promise<void>;
  remove: (productId: number, imageId: number) => Promise<void>;
  reorder: (productId: number, ordered: EntityImage[]) => Promise<void>;
  clearError: () => void;
}

/**
 * Manages the full multi-image lifecycle for a single product.
 * Kept separate from useCrudList so image state doesn't trigger
 * full-list re-renders (Single Responsibility).
 */
export function useProductImages(): UseProductImagesResult {
  const [images, setImages] = useState<EntityImage[]>([]);
  const [loading, setLoading] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState<number | null>(null);
  const [deletingId, setDeletingId] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);

  const fetch = useCallback(async (productId: number) => {
    setLoading(true);
    setError(null);
    try {
      const data = await productService.getImages(productId);
      setImages(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load images.');
    } finally {
      setLoading(false);
    }
  }, []);

  const upload = useCallback(async (productId: number, files: File[]) => {
    setUploading(true);
    setUploadProgress(0);
    setError(null);
    try {
      const newImages = await productService.uploadImages(productId, files, (percent) =>
        setUploadProgress(percent),
      );
      setImages((prev) => [...prev, ...newImages].sort((a, b) => a.sortOrder - b.sortOrder));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to upload images.');
    } finally {
      setUploading(false);
      setUploadProgress(null);
    }
  }, []);

  const remove = useCallback(async (productId: number, imageId: number) => {
    setDeletingId(imageId);
    setError(null);
    try {
      await productService.deleteImage(productId, imageId);
      setImages((prev) => prev.filter((img) => img.id !== imageId));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete image.');
    } finally {
      setDeletingId(null);
    }
  }, []);

  const reorder = useCallback(async (productId: number, ordered: EntityImage[]) => {
    // Optimistic update
    setImages(ordered);
    try {
      const updated = await productService.reorderImages(
        productId,
        ordered.map((img, idx) => ({ id: img.id, sortOrder: idx })),
      );
      setImages(updated);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to reorder images.');
    }
  }, []);

  return {
    images,
    loading,
    error,
    uploading,
    uploadProgress,
    deletingId,
    fetch,
    upload,
    remove,
    reorder,
    clearError: () => setError(null),
  };
}