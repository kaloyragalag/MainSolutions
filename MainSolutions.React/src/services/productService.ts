
import { Product, ProductFormData } from '../types/product';
import { BaseService } from './baseService';
import { tokenStorage } from './storage';

class ProductServiceClass extends BaseService<Product, ProductFormData> {
  constructor() {
    super('products');
  }

  // Add any product-specific methods here if needed in the future
  async uploadImage(id: number, file: File): Promise<Product> {
    const formData = new FormData();
    formData.append('file', file);

    const res = await fetch(`${this.baseUrl}/${id}/image`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${tokenStorage.getToken()}` },
      body: formData,
    });

    if (!res.ok) {
      const body = await res.json().catch(() => null);
      throw new Error(body?.message ?? 'Failed to upload image.');
    }
    return res.json();
  }

  async deleteImage(id: number): Promise<Product> {
    const res = await fetch(`${this.baseUrl}/${id}/image`, {
      method: 'DELETE',
      headers: { Authorization: `Bearer ${tokenStorage.getToken()}` },
    });

    if (!res.ok) {
      const body = await res.json().catch(() => null);
      throw new Error(body?.message ?? 'Failed to remove image.');
    }
    return res.json();
  }
}

export const productService = new ProductServiceClass();
