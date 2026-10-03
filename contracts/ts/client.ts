import type {
  ApiDataResponse,
  ApiErrorBody,
  ApiResponse,
  Category,
  CategoryInput,
  Product,
  ProductInput,
} from "./types";

export interface ApiClientOptions {
  /** Origin of the API, e.g. `http://localhost:5159`. */
  baseUrl: string;
  /** Custom fetch (tests, SSR, interceptors). Defaults to the global `fetch`. */
  fetch?: typeof fetch;
  /** Extra headers sent with every request, e.g. `{ Authorization: "Bearer ..." }`. */
  headers?: Record<string, string>;
}

/** Thrown for every non-2xx response and for responses that are not valid API JSON. */
export class ApiError extends Error {
  constructor(
    public readonly statusCode: number,
    message: string,
    /** Field name to validation messages (400 validation failures only). */
    public readonly errors?: Record<string, string[]>,
    /** Stack trace (Development environment only). */
    public readonly details?: string,
  ) {
    super(message);
    this.name = "ApiError";
  }

  get isValidationError(): boolean {
    return this.statusCode === 400 && this.errors !== undefined;
  }
}

export function createApiClient(options: ApiClientOptions) {
  const baseUrl = options.baseUrl.replace(/\/+$/, "");
  const doFetch = options.fetch ?? ((...args: Parameters<typeof fetch>) => fetch(...args));

  async function request<T extends ApiResponse>(path: string, init: RequestInit = {}): Promise<T> {
    const response = await doFetch(`${baseUrl}${path}`, {
      ...init,
      headers: { Accept: "application/json", ...options.headers, ...init.headers },
    });

    let body: unknown;
    try {
      body = await response.json();
    } catch {
      throw new ApiError(response.status, response.statusText || "Unexpected response from the server.");
    }

    if (!response.ok) {
      const error = body as Partial<ApiErrorBody>;
      throw new ApiError(
        response.status,
        error.message ?? response.statusText,
        error.errors,
        error.details,
      );
    }
    return body as T;
  }

  const json = (method: string, payload: unknown): RequestInit => ({
    method,
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });

  function productForm(input: ProductInput): FormData {
    const form = new FormData();
    form.append("Name", input.name);
    form.append("Description", input.description);
    form.append("NewPrice", String(input.newPrice));
    form.append("OldPrice", String(input.oldPrice));
    form.append("CategoryId", String(input.categoryId));
    for (const photo of input.photos ?? []) {
      form.append("Photos", photo);
    }
    return form;
  }

  return {
    categories: {
      getAll: async (): Promise<Category[]> =>
        (await request<ApiDataResponse<Category[]>>("/api/categories")).data,

      getById: async (id: number): Promise<Category> =>
        (await request<ApiDataResponse<Category>>(`/api/categories/${id}`)).data,

      create: async (input: CategoryInput): Promise<Category> =>
        (await request<ApiDataResponse<Category>>("/api/categories", json("POST", input))).data,

      update: async (id: number, input: CategoryInput): Promise<Category> =>
        (await request<ApiDataResponse<Category>>(`/api/categories/${id}`, json("PUT", input))).data,

      /** Rejects with a 409 `ApiError` while the category still has products. */
      remove: (id: number): Promise<ApiResponse> =>
        request<ApiResponse>(`/api/categories/${id}`, { method: "DELETE" }),
    },

    products: {
      getAll: async (): Promise<Product[]> =>
        (await request<ApiDataResponse<Product[]>>("/api/products")).data,

      getById: async (id: number): Promise<Product> =>
        (await request<ApiDataResponse<Product>>(`/api/products/${id}`)).data,

      create: async (input: ProductInput): Promise<Product> =>
        (await request<ApiDataResponse<Product>>("/api/products", { method: "POST", body: productForm(input) })).data,

      update: async (id: number, input: ProductInput): Promise<Product> =>
        (await request<ApiDataResponse<Product>>(`/api/products/${id}`, { method: "PUT", body: productForm(input) })).data,

      remove: (id: number): Promise<ApiResponse> =>
        request<ApiResponse>(`/api/products/${id}`, { method: "DELETE" }),
    },

    /** Turns `Photo.imageName` into an absolute URL usable in `<img src>`. */
    imageUrl: (imageName: string): string =>
      `${baseUrl}/${imageName.split("/").map(encodeURIComponent).join("/")}`,
  };
}

export type ApiClient = ReturnType<typeof createApiClient>;
