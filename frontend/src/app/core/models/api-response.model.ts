export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
}

export function unwrapApiData<T>(response: ApiResponse<T> | T, fallback: T): T {
  const apiResponse = response as ApiResponse<T>;
  if (
    apiResponse &&
    typeof apiResponse === 'object' &&
    'data' in apiResponse &&
    apiResponse.data !== null &&
    apiResponse.data !== undefined
  ) {
    return apiResponse.data;
  }
  return (response as T) ?? fallback;
}
