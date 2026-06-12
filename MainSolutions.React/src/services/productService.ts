
import { Product, ProductFormData } from '../types/product';
import { BaseService } from './baseService';

class ProductServiceClass extends BaseService<Product, ProductFormData> {
  constructor() {
    super('products');
  }

  // Add any product-specific methods here if needed in the future
}

export const productService = new ProductServiceClass();
