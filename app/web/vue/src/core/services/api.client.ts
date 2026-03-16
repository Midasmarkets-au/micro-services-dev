import axios from "axios";

const api =
  process.env.VUE_APP_ENV === "Development" ? process.env.VUE_APP_API_URL : "/";

const axiosInstance = axios.create({
  withCredentials: true,
  headers: {
    "Content-Type": "application/json",
  },
  baseURL: api,
  // baseURL: "https://pro.t.api.mybcr.dev",
  paramsSerializer: paramsSerializerForDotnet,
});

const axiosInstance2 = axios.create({
  withCredentials: true,
  headers: {
    "Content-Type": "application/json",
  },
  baseURL: process.env.VUE_APP_API_V2_URL,
});

// axiosInstance.interceptors.request.use((config) => {
//   const api = window.localStorage.getItem("ServiceUrl");
//   if (api) {
//     config.baseURL = api;
//   }
//   return config;
// });

function parseParams(criteria: object | undefined): URLSearchParams {
  const params = new URLSearchParams();
  if (criteria == null) {
    return params;
  }
  Object.keys(criteria ?? {}).forEach((key) => {
    if (criteria[key] != null && criteria[key] != undefined) {
      params.append(key, criteria[key]);
    }
  });
  return params;
}

/**
 * .net core api params serializer, designed for parse array params
 * @param params
 */
function paramsSerializerForDotnet(params: any) {
  const paramStrArray = Array<string>();
  Object.keys(params).forEach((key: string) => {
    if (params[key] === null || params[key] === undefined || params[key] === "")
      return;

    // array params
    if (Array.isArray(params[key])) {
      params[key].forEach((item: any) => {
        if (
          params[key] === null ||
          params[key] === undefined ||
          params[key] === ""
        )
          return;
        paramStrArray.push(`${key}=${encodeURIComponent(item)}`);
      });
      return;
    }

    paramStrArray.push(`${key}=${encodeURIComponent(params[key])}`);
  });

  return paramStrArray.join("&");
}

function firstLetterToLower(str) {
  return str.charAt(0).toLowerCase() + str.slice(1);
}

export function processKeysToCamelCase(obj) {
  if (Array.isArray(obj)) {
    return obj.map((item) => processKeysToCamelCase(item));
  }

  if (typeof obj === "object" && obj !== null) {
    return Object.keys(obj).reduce((acc, key) => {
      acc[firstLetterToLower(key)] = processKeysToCamelCase(obj[key]);
      return acc;
    }, {});
  }

  return obj;
}

export {
  axiosInstance,
  axiosInstance2,
  parseParams,
  paramsSerializerForDotnet,
};
