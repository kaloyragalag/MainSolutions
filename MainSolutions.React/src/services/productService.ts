import { Product, ProductFormData } from '../types/product';
import { EntityImage } from '../types/entityImage';
import { BaseService } from './baseService';
import { tokenStorage } from './storage';

class ProductServiceClass extends BaseService<Product, ProductFormData> {
  constructor() {
    super('products');
  }

  // ── Multi-image API ──────────────────────────────────────────────────────

  async getImages(id: number): Promise<EntityImage[]> {
    const res = await fetch(`${this.baseUrl}/${id}/images`, {
      headers: { Authorization: `Bearer ${tokenStorage.getToken()}` },
    });
    if (!res.ok) throw new Error('Failed to load images.');
    return res.json();
  }

  /**
   * Upload one or more files, reporting upload progress (0–100) via
   * onProgress. Uses XMLHttpRequest instead of fetch because fetch has no
   * upload-progress event — only XHR exposes `upload.onprogress`.
   */
  uploadImages(
    id: number,
    files: File[],
    onProgress?: (percent: number) => void,
  ): Promise<EntityImage[]> {
    return new Promise((resolve, reject) => {
      const formData = new FormData();
      files.forEach((f) => formData.append('files', f));

      const xhr = new XMLHttpRequest();
      xhr.open('POST', `${this.baseUrl}/${id}/images`);
      xhr.setRequestHeader('Authorization', `Bearer ${tokenStorage.getToken()}`);

      xhr.upload.onprogress = (e) => {
        if (e.lengthComputable && onProgress) {
          onProgress(Math.round((e.loaded / e.total) * 100));
        }
      };

      xhr.onload = () => {
        if (xhr.status >= 200 && xhr.status < 300) {
          try {
            resolve(JSON.parse(xhr.responseText));
          } catch {
            reject(new Error('Failed to parse upload response.'));
          }
        } else {
          let message = 'Failed to upload images.';
          try {
            message = JSON.parse(xhr.responseText)?.message ?? message;
          } catch {
            /* ignore parse failure, use default message */
          }
          reject(new Error(message));
        }
      };

      xhr.onerror = () => reject(new Error('Network error while uploading images.'));
      xhr.send(formData);
    });
  }

  async deleteImage(productId: number, imageId: number): Promise<void> {
    const res = await fetch(`${this.baseUrl}/${productId}/images/${imageId}`, {
      method: 'DELETE',
      headers: { Authorization: `Bearer ${tokenStorage.getToken()}` },
    });
    if (!res.ok) {
      const body = await res.json().catch(() => null);
      throw new Error(body?.message ?? 'Failed to delete image.');
    }
  }

  async reorderImages(
    productId: number,
    items: { id: number; sortOrder: number }[],
  ): Promise<EntityImage[]> {
    const res = await fetch(`${this.baseUrl}/${productId}/images/reorder`, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${tokenStorage.getToken()}`,
      },
      body: JSON.stringify(items),
    });
    if (!res.ok) throw new Error('Failed to reorder images.');
    return res.json();
  }
}

export const productService = new ProductServiceClass();
