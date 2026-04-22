import { axiosInstance, axiosInstance2 } from "@/core/services/api.client";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import store from "@/store";
import { Actions } from "@/store/enums/StoreEnums";
import router from "@/projects/client/config/router";
// Token expiry is managed entirely by the auth service (cookie Max-Age / JWT exp).
// The frontend no longer runs its own logout timer.
// import { setTimerForLogout } from "@/core/plugins/TimerService";
// import Can from "@/core/plugins/ICan";
// import { RoleTypes } from "@/core/types/RoleTypes";

const handleRequestFulfilled = (config) => {
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
  // 401: cookie expired or absent — redirect to sign-in without calling LOGOUT
  // (LOGOUT is only triggered by explicit user action)
  if (status === 401 && process.env.VUE_APP_DEV !== "local") {
    originalConfig._retry = true;
    router.push({ name: "sign-in" });
    return;
  }

  if (status === 500) {
    originalConfig._retry = true;
    MsgPrompt.error("Server error, please try again later");
    return;
  }
  if (status === 403 && process.env.VUE_APP_DEV !== "local") {
    return Promise.reject(error);
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
