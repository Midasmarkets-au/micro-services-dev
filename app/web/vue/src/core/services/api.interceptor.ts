import { axiosInstance, axiosInstance2 } from "@/core/services/api.client";
import { getLocalAccessToken } from "@/core/services/token.service";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import store from "@/store";
import { Actions } from "@/store/enums/StoreEnums";
import router from "@/projects/client/config/router";
import { setTimerForLogout } from "@/core/plugins/TimerService";
import Can from "@/core/plugins/ICan";
import { RoleTypes } from "@/core/types/RoleTypes";

const handleRequestFulfilled = (config) => {
  const token = getLocalAccessToken()?.access_token;
  if (token) {
    config.headers["Authorization"] = "Bearer " + token; // for Spring Boot back-end
    if (!Can.cans([RoleTypes.TenantAdmin, RoleTypes.SuperAdmin])) {
      setTimerForLogout();
    }
    // config.headers["x-access-token"] = token; // for Node.js Express back-end
  }
  return config;
};

const handleRequestRejected = (error) => {
  return Promise.reject(error);
};

const handleResponseFulfilled = (response) => {
  const body = response.data;
  // Normalize gRPC JSON Transcoding list responses to legacy Result<T,C> envelope.
  // New format: { data: [...], meta: { page, size, total, pageCount, hasMore } }
  // Old format: { status: 1, data: [...], criteria: { page, size, total, ... } }
  if (
    body &&
    typeof body === "object" &&
    "meta" in body &&
    !("criteria" in body) &&
    !("status" in body)
  ) {
    response.data = {
      status: 1,
      data: body.data ?? [],
      criteria: body.meta,
      message: null,
    };
  }
  return response;
};

const handleResponseRejected = async (error) => {
  const originalConfig = error.config;
  if (
    originalConfig?.imageGuid === "/connect/token" ||
    !error.response ||
    originalConfig._retry
  )
    return Promise.reject(error);

  const { status } = error.response;
  // Access Token was expired
  if (status === 401 && process.env.VUE_APP_DEV !== "local") {
    originalConfig._retry = true;
    await store.dispatch(Actions.LOGOUT);
    router.push({ name: "sign-in" });
    return;
  }

  if (status === 500) {
    originalConfig._retry = true;
    MsgPrompt.error("Server error, please try again later");
    return;
  }
  if (status === 403 && process.env.VUE_APP_DEV !== "local") {
    await store.dispatch(Actions.LOGOUT);
    router.push({ name: "sign-in" });
    return;
    // MsgPrompt.error("No permission to access this resource.");
    // return Promise.reject("No permission to access this.");
  }

  if (status === 406) {
    MsgPrompt.error("No permission to access this resource.");
    return Promise.reject("No permission to access this.");
  }

  if (status === 422) {
    MsgPrompt.error(error.response.data.message);
    return;
  }

  return Promise.reject(error);
};

const setup = () => {
  axiosInstance.interceptors.request.use(
    handleRequestFulfilled,
    handleRequestRejected
  );

  axiosInstance.interceptors.response.use(
    handleResponseFulfilled,
    handleResponseRejected
  );

  axiosInstance2.interceptors.request.use(
    handleRequestFulfilled,
    handleRequestRejected
  );

  axiosInstance2.interceptors.response.use(
    handleResponseFulfilled,
    handleResponseRejected
  );
};

export default setup;
