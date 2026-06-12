export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CrudService<T, TFormData> {
  getAll: (page: number, pageSize: number) => Promise<PagedResult<T>>;
  getById?: (id: number) => Promise<T>;
  create: (data: TFormData) => Promise<T>;
  update: (id: number, data: TFormData) => Promise<T>;
  delete: (id: number) => Promise<void>;
}
