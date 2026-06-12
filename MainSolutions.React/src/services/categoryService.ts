import { Category, CategoryFormData } from '../types/category';

const API_URL = process.env.REACT_APP_API_URL || 'https://localhost:5001';

const getToken = () => localStorage.getItem('token');

const headers = (): HeadersInit => {
  const token = getToken();
  return {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
};

const handleResponse = async <T,>(res: Response): Promise<T> => {
  if (!res.ok) {
    let message = `Request failed with status ${res.status}`;
    try {
      const data = await res.json();
      message = data?.message || data?.title || message;
    } catch {
      // ignore body parse errors
    }
    throw new Error(message);
  }
  if (res.status === 204) {
    return undefined as T;
  }
  return res.json() as Promise<T>;
};

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export const categoryService = {
  getAll: async (page: number = 1, pageSize: number = 10): Promise<PagedResult<Category>> => {
    const res = await fetch(`${API_URL}/api/categories?page=${page}&pageSize=${pageSize}`, {
      headers: headers(),
    });
    const data = await handleResponse<any>(res);

    // Handle plain arrays as well as wrapped/paginated responses
    if (Array.isArray(data)) {
      return {
        items: data,
        totalCount: data.length,
        page: 1,
        pageSize: data.length,
        totalPages: 1,
        hasPreviousPage: false,
        hasNextPage: false,
      };
    }

    const items = data?.items ?? data?.data ?? data?.results ?? [];
    if (!Array.isArray(items)) {
      console.warn('Unexpected categories response shape:', data);
    }

    return {
      items: Array.isArray(items) ? items : [],
      totalCount: data?.totalCount ?? items.length ?? 0,
      page: data?.page ?? page,
      pageSize: data?.pageSize ?? pageSize,
      totalPages: data?.totalPages ?? 1,
      hasPreviousPage: data?.hasPreviousPage ?? false,
      hasNextPage: data?.hasNextPage ?? false,
    };
  },

  getById: async (id: number): Promise<Category> => {
    const res = await fetch(`${API_URL}/api/categories/${id}`, { headers: headers() });
    return handleResponse<Category>(res);
  },

  create: async (data: CategoryFormData): Promise<Category> => {
    const res = await fetch(`${API_URL}/api/categories`, {
      method: 'POST',
      headers: headers(),
      body: JSON.stringify(data),
    });
    return handleResponse<Category>(res);
  },

  update: async (id: number, data: CategoryFormData): Promise<Category> => {
    const res = await fetch(`${API_URL}/api/categories/${id}`, {
      method: 'PUT',
      headers: headers(),
      body: JSON.stringify({ id, ...data }),
    });
    return handleResponse<Category>(res);
  },

  delete: async (id: number): Promise<void> => {
    const res = await fetch(`${API_URL}/api/categories/${id}`, {
      method: 'DELETE',
      headers: headers(),
    });
    return handleResponse<void>(res);
  },
};