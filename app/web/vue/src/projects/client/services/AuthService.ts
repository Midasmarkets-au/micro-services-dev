import { Router } from "vue-router";
import { Store } from "vuex";
import { Actions } from "@/store/enums/StoreEnums";
import ApiService from "@/core/services/ApiService";
import MsgPrompt from "@/core/plugins/MsgPrompt";

export interface LoginParams {
  email: string;
  password: string;
  tenantId?: string;
  twoFaCode?: string;
}

export interface LoginOptions {
  router: Router;
  store: Store<any>;
  wsSignalR?: any;
  onTwoFaRequired?: () => void;
  onMultipleTenants?: (tenantIds: any[]) => void;
  onError?: (errors: any) => void;
  t: (key: string) => string;
  redirectTo?: string; // 自定义登录成功后的跳转路径
}

/**
 * 设置登录请求参数
 */
export function setupLoginParams(params: LoginParams): URLSearchParams {
  const urlParams = new URLSearchParams();
  urlParams.append("client_id", "api");
  urlParams.append("grant_type", "password");
  urlParams.append("scopes", "api");
  urlParams.append("username", params.email);
  urlParams.append("password", params.password);

  if (params.tenantId) {
    urlParams.append("tenantId", params.tenantId);
  }

  if (params.twoFaCode) {
    urlParams.append("tf_code", params.twoFaCode);
  }

  return urlParams;
}

/**
 * 处理登录错误
 */
export function handleLoginError(
  errors: any,
  twoFaRequired: boolean,
  t: (key: string) => string,
  onEmailUnconfirmed?: () => void
): void {
  if (errors == "__EMAIL_UNCONFIRMED__") {
    if (onEmailUnconfirmed) {
      onEmailUnconfirmed();
    }
  } else if (errors == "__USER_IS_LOCKED_OUT__") {
    MsgPrompt.error(t("error.__USER_IS_LOCKED_OUT__"));
  } else if (errors == "__USER_UNDER_MAINTENANCE__") {
    MsgPrompt.warning(t("error.__USER_UNDER_MAINTENANCE__"));
  } else if (twoFaRequired) {
    MsgPrompt.error(t("error.verificationFailed"));
  } else {
    MsgPrompt.error(errors);
  }
}

/**
 * 处理双因素认证
 */
export function handleTwoFA(login: any): boolean {
  if (login?.data?.twoFactorRequired == true) {
    return true;
  }
  return false;
}

/**
 * 处理多租户
 */
export function handleMultipleTenants(login: any): {
  hasMultiple: boolean;
  tenantIds?: any[];
} {
  if (login?.data?.hasMultipleTenants == true) {
    return {
      hasMultiple: true,
      tenantIds: login.data.tenantIds,
    };
  }
  return { hasMultiple: false };
}

/**
 * 执行登录启动流程
 */
export async function loginStart(
  login: any,
  store: Store<any>,
  router: Router,
  wsSignalR?: any,
  redirectTo?: string
): Promise<void> {
  store.dispatch(Actions.LOGIN, login.data);
  // Token is in HttpOnly cookie — SignalR uses withCredentials to send it automatically
  wsSignalR?.setup(null);
  wsSignalR?.connection
    ?.start()
    .catch((err) => console.warn("SignalR start failed:", err));

  // 优先级：自定义redirectTo > query.redirect > dashboard
  if (redirectTo) {
    await router.push(redirectTo);
  } else if (router.currentRoute.value.query.redirect) {
    await router.push(router.currentRoute.value.query.redirect as string);
  } else {
    await router.push({ name: "dashboard" });
  }
}

/**
 * 完整的登录流程
 */
export async function performLogin(
  params: LoginParams,
  options: LoginOptions
): Promise<{
  success: boolean;
  requiresTwoFa?: boolean;
  multipleTenants?: any[];
}> {
  const {
    router,
    store,
    wsSignalR,
    onTwoFaRequired,
    onMultipleTenants,
    onError,
    t,
  } = options;

  // 清除当前登录状态
  await store.dispatch(Actions.LOGOUT);

  // 设置请求参数
  const urlParams = setupLoginParams(params);

  // 发送登录请求
  let errors: any = null;
  const login = await ApiService.postToken("connect/token", urlParams).catch(
    ({ response }) => {
      if (response.data.error != undefined) {
        // Prefer error_description (contains semantic code like __LOGIN_FAILED__)
        // over error (contains OAuth2 category like "invalid_grant")
        errors = [response.data.error_description ?? response.data.error];
      } else {
        errors = response.data.errors;
      }
    }
  );

  // 错误处理
  if (errors) {
    handleLoginError(errors, !!params.twoFaCode, t);
    if (onError) {
      onError(errors);
    }
    return { success: false };
  }

  // 处理多租户
  const multiTenantResult = handleMultipleTenants(login);
  if (multiTenantResult.hasMultiple && !params.tenantId) {
    if (onMultipleTenants) {
      onMultipleTenants(multiTenantResult.tenantIds || []);
    }
    return {
      success: false,
      multipleTenants: multiTenantResult.tenantIds,
    };
  }

  // 处理双因素认证
  if (handleTwoFA(login)) {
    if (onTwoFaRequired) {
      onTwoFaRequired();
    }
    return { success: false, requiresTwoFa: true };
  }

  // 登录成功
  await loginStart(login, store, router, wsSignalR, options.redirectTo);

  return { success: true };
}
