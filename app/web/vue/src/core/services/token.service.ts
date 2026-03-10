import { IJwtToken } from "@/core/models/JwtToken";

const ID_TOKEN_KEY = "jwt_token" as string;

/**
 * @description get token form localStorage
 */
export const getLocalAccessToken = (): IJwtToken | null => {
  return JSON.parse(window.localStorage.getItem(ID_TOKEN_KEY) ?? "{}");
};

/**
 * @description save token into localStorage
 * @TODO set expiration time
 * @param token: string
 */
export const updateLocalAccessToken = (token: IJwtToken): void => {
  window.localStorage.setItem(ID_TOKEN_KEY, JSON.stringify(token));
};

/**
 * @description remove token form localStorage
 */
export const removeLocalAccessToken = (): void => {
  window.localStorage.removeItem(ID_TOKEN_KEY);
};
