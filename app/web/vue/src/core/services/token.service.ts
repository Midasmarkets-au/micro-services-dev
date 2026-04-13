// Token is now stored as an HttpOnly cookie set by the auth service.
// localStorage-based token storage is disabled — do not read/write tokens from JS.

import { IJwtToken } from "@/core/models/JwtToken";

// const ID_TOKEN_KEY = "jwt_token" as string;

export const getLocalAccessToken = (): IJwtToken | null => {
  // return JSON.parse(window.localStorage.getItem(ID_TOKEN_KEY) ?? "{}");
  return null;
};

export const updateLocalAccessToken = (_token: IJwtToken): void => {
  // window.localStorage.setItem(ID_TOKEN_KEY, JSON.stringify(token));
};

export const removeLocalAccessToken = (): void => {
  // window.localStorage.removeItem(ID_TOKEN_KEY);
};
