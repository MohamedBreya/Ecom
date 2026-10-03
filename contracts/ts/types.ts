/**
 * Request/response types for the Ecom API.
 * Hand-maintained to mirror `../openapi.json` (the generated source of truth) - keep them in sync when the API changes.
 */

// ---------------------------------------------------------------------------
// Response envelopes
// ---------------------------------------------------------------------------

/** Every response carries a status code and a human readable message. */
export interface ApiResponse {
  statusCode: number;
  message: string;
}

/** Successful response that carries a payload. */
export interface ApiDataResponse<T> extends ApiResponse {
  data: T;
}

/** Body of every non-2xx response. */
export interface ApiErrorBody extends ApiResponse {
  /** Stack trace. Only present when the API runs in the Development environment. */
  details?: string;
  /** Field name (camelCase) to validation messages. Only present for 400 validation failures. */
  errors?: Record<string, string[]>;
}

// ---------------------------------------------------------------------------
// Categories
// ---------------------------------------------------------------------------

export interface Category {
  id: number;
  name: string;
  description: string;
}

/** Body for POST /api/categories and PUT /api/categories/{id}. */
export interface CategoryInput {
  /** Required, max 30 characters. */
  name: string;
  /** Required, max 500 characters. */
  description: string;
}

// ---------------------------------------------------------------------------
// Products
// ---------------------------------------------------------------------------

export interface Photo {
  id: number;
  /** Path relative to the API root, e.g. `Images/Phone/3f2c....jpg`. Resolve it with `client.imageUrl(path)`. */
  imageName: string;
}

export interface Product {
  id: number;
  name: string;
  description: string;
  newPrice: number;
  oldPrice: number;
  categoryId: number;
  categoryName: string;
  photos: Photo[];
}

/**
 * Input for POST /api/products and PUT /api/products/{id}.
 * Sent as `multipart/form-data` (the client builds the form for you).
 */
export interface ProductInput {
  /** Required, max 100 characters. */
  name: string;
  /** Required, max 2000 characters. */
  description: string;
  /** Must be at least 0.01. */
  newPrice: number;
  /** Must be 0 or more. */
  oldPrice: number;
  /** Must reference an existing category. */
  categoryId: number;
  /**
   * jpg, jpeg, png, gif or webp; max 5 MB each.
   * On update: omit to keep the current photos, provide to replace all of them.
   */
  photos?: Array<File | Blob>;
}
