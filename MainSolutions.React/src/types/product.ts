import { Category } from "./category";

export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  stock: number;
  categoryId: number;
  category?: Category;
  imagePath?: string | null;
  isActive?: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface ProductFormData {
  name: string;
  description: string;
  price: number;
  stock: number;
  categoryId: number;
}
