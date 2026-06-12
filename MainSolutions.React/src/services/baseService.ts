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

/**
 * Generic base service providing common CRUD operations against a REST endpoint.
 *
 * @typeParam T - The entity shape returned by the API (e.g. Category)
 * @typeParam TFormData - The shape used for create/update payloads (e.g. CategoryFormData)
 */
export class BaseService<T, TFormData = Partial<T>> {
  protected readonly endpoint: string;

  /**
   * @param endpoint - Path segment under the API, e.g. 'categories' -> /api/categories
   */
  constructor(endpoint: string) {
    this.endpoint = endpoint;
  }

  protected get baseUrl(): string {
    return `${API_URL}/api/${this.endpoint}`;
  }

  async getAll(page: number = 1, pageSize: number = 10): Promise<PagedResult<T>> {
    const res = await fetch(`${this.baseUrl}?page=${page}&pageSize=${pageSize}`, {
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
      console.warn(`Unexpected ${this.endpoint} response shape:`, data);
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
  }

  async getById(id: number): Promise<T> {
    const res = await fetch(`${this.baseUrl}/${id}`, { headers: headers() });
    return handleResponse<T>(res);
  }

  async create(data: TFormData): Promise<T> {
    const res = await fetch(`${this.baseUrl}`, {
      method: 'POST',
      headers: headers(),
      body: JSON.stringify(data),
    });
    return handleResponse<T>(res);
  }

  async update(id: number, data: TFormData): Promise<T> {
    const res = await fetch(`${this.baseUrl}/${id}`, {
      method: 'PUT',
      headers: headers(),
      body: JSON.stringify({ id, ...data }),
    });
    return handleResponse<T>(res);
  }

  async delete(id: number): Promise<void> {
    const res = await fetch(`${this.baseUrl}/${id}`, {
      method: 'DELETE',
      headers: headers(),
    });
    return handleResponse<void>(res);
  }
}

export { headers, handleResponse, API_URL };