import { Category, CategoryFormData } from '../types/category';
import { BaseService } from './baseService';

class CategoryServiceClass extends BaseService<Category, CategoryFormData> {
  constructor() {
    super('categories');
  }

  // Add any category-specific methods here if needed in the future
}

export const categoryService = new CategoryServiceClass();
