// Token is now stored as an HttpOnly cookie set by the auth service.
// localStorage-based token storage is disabled — do not read/write tokens from JS.

// const ID_TOKEN_KEY = "id_token" as string;

export const getToken = (): string | null => {
  // return window.localStorage.getItem(ID_TOKEN_KEY);
  return null;
};

export const saveToken = (_token: string): void => {
  // window.localStorage.setItem(ID_TOKEN_KEY, token);
};

export const destroyToken = (): void => {
  // window.localStorage.removeItem(ID_TOKEN_KEY);
};

export default { getToken, saveToken, destroyToken };
