export interface Category {
  id: number;
  name: string;
  description: string;
  isActive?: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface CategoryFormData {
  name: string;
  description: string;
}
