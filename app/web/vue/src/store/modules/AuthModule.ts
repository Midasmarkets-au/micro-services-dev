import ApiService from "@/core/services/ApiService";
import { Actions, Mutations } from "@/store/enums/StoreEnums";

import { Module, Action, Mutation, VuexModule } from "vuex-module-decorators";
import { LanguageCodes } from "@/core/types/LanguageTypes";

import {
  getLocalAccessToken,
  removeLocalAccessToken,
  updateLocalAccessToken,
} from "@/core/services/token.service";
import store from "@/store";
import { setLocale } from "@vee-validate/i18n";
import ClientGlobalService from "@/projects/client/services/ClientGlobalService";
import MsgPrompt from "@/core/plugins/MsgPrompt";

const USER_DATA_KEY = "user" as string;

export interface User {
  uid: number;
  name: string;
  firstname: string;
  firstName: string;
  lastName: string;
  lastname: string;
  nativeName: string;
  email: string;
  phoneNumber: string;
  password: string;
  api_token: string;
  avatar: string;
  language: string;
  timezone: string;
  roles: string[];
  ccc: string;
  countryCode: string;
  permissions: string[];
  ibAccount: string[];
  twoFactorEnabled: boolean;
  twoFactorAuth: any;
  tenancy: string;
  configurations: any;
  defaultAgentAccount: string;
  defaultSalesAccount: string;
}

export interface UserAuthInfo {
  errors: unknown;
  user: User;
  isAuthenticated: boolean;
  hasUserVerified2Fa: boolean;
  config: any;
}

@Module
export default class AuthModule extends VuexModule implements UserAuthInfo {
  errors = {};
  user = {} as User;
  // Auth state is determined by calling /api/v1/user/me (cookie-based), not localStorage.
  isAuthenticated = false;
  hasUserVerified2Fa =
    window.localStorage.getItem("hasUserVerified2Fa") === "true";

  config = {} as any;

  /**
   * Get current user object
   * @returns User
   */
  get currentUser(): User {
    return this.user;
  }

  /**
   * Load local user object
   * @returns User
   */
  get loadUser(): User {
    return Object.assign(
      {},
      JSON.parse(window.localStorage.getItem(USER_DATA_KEY) || "{}")
    );
  }

  /**
   * Verify user authentication
   * @returns boolean
   */
  get isUserAuthenticated(): boolean {
    return this.isAuthenticated;
  }

  get roles(): string[] {
    return this.user.roles;
  }

  get permissions(): string[] {
    return this.user.permissions;
  }

  /**
   * Return whether user has verified
   * @returns boolean
   */
  get isUser2fa(): boolean {
    // return this.isAuthenticated;
    // return this.user.twoFactorEnabled;
    // console.log(
    //   'window.localStorage.getItem("hasUserVerified2Fa")',
    //   typeof window.localStorage.getItem("hasUserVerified2Fa")
    // );
    // console.log(
    //   "twoFactorEnabled,hasUserVerified2Fa ",
    //   this.user.twoFactorEnabled,
    //   this.hasUserVerified2Fa
    // );
    if (!this.user.twoFactorEnabled) return true;

    return this.hasUserVerified2Fa;
  }

  /**
   * Get authentification errors
   * @returns array
   */
  get getErrors() {
    return this.errors;
  }

  @Mutation
  [Mutations.SET_ERROR](error) {
    this.errors = { ...error };
  }

  @Mutation
  [Mutations.SET_AUTH](data) {
    this.isAuthenticated = true;
    updateLocalAccessToken(data);
  }

  @Mutation
  [Mutations.SET_USER](data) {
    this.user = data;
    this.user.language ??= LanguageCodes.enUS;
    this.user.timezone ??= "UTC";

    window.localStorage.setItem(USER_DATA_KEY, JSON.stringify(this.user));
    window.localStorage.setItem("language", this.user.language);
  }

  @Mutation
  [Mutations.SET_USER_LANG](data) {
    this.user.language = data ?? LanguageCodes.enUS;
    window.localStorage.setItem(USER_DATA_KEY, JSON.stringify(this.user));
    window.localStorage.setItem("language", this.user.language);
  }

  @Mutation
  [Mutations.SET_USER_AVATAR](data) {
    this.user.avatar = data ?? "";
    window.localStorage.setItem(USER_DATA_KEY, JSON.stringify(this.user));
  }

  @Mutation
  [Mutations.SET_PASSWORD](password) {
    this.user.password = password;
  }

  @Mutation
  [Mutations.PURGE_AUTH]() {
    removeLocalAccessToken();
    this.isAuthenticated = false;
    this.hasUserVerified2Fa = false;
    this.user = {} as User;
    this.errors = [];
    if (store.state.AgentModule) {
      store.commit(Mutations.SET_IB_CURRENT_ACCOUNT, null);
      store.commit(Mutations.SET_IB_ACCOUNTS, null);
    }

    if (store.state.SalesModule) {
      store.commit(Mutations.SET_SALES_ACCOUNT, null);
    }

    store.commit(Mutations.SET_CONFIG, null);
    window.localStorage.clear();
  }

  @Mutation
  [Mutations.PURGE_TWOFA]() {
    this.hasUserVerified2Fa = false;
  }

  @Mutation
  [Mutations.SET_CONFIG](data) {
    this.config = data;
    if (this.config) {
      window.localStorage.setItem("config", JSON.stringify(this.config));
    } else {
      window.localStorage.removeItem("config");
    }
  }

  @Mutation
  [Mutations.SET_HAS_USER_VERIFIED_2FA](data: boolean) {
    this.hasUserVerified2Fa = data;
    window.localStorage.setItem("hasUserVerified2Fa", data ? "true" : "false");
  }

  @Action
  [Actions.UPDATE_USER](data) {
    this.context.commit(Mutations.SET_USER, data);
  }

  @Action
  [Actions.LOGIN](data) {
    this.context.commit(Mutations.SET_AUTH, data);
    this.context.commit(Mutations.SET_HAS_USER_VERIFIED_2FA, true);
  }

  @Action
  [Actions.UPDATE_CONFIG](data) {
    this.context.commit(Mutations.SET_CONFIG, data);
  }

  @Action
  [Actions.TWOFA](credentials) {
    ApiService.setHeader();
    return ApiService.put("auth/token/verify", credentials)
      .then(() => {
        return;
      })
      .catch(({ response }) => {
        if (response.data.error != undefined) {
          this.context.commit(Mutations.SET_ERROR, [response.data.error]);
        } else {
          this.context.commit(Mutations.SET_ERROR, response.data.errors);
        }
      });
  }

  @Action
  [Actions.TWOFA_VERIFIED]() {
    this.context.commit(Mutations.SET_HAS_USER_VERIFIED_2FA, true);
  }

  @Action
  async [Actions.LOGOUT]() {
    // Call auth service to invalidate the access token (jti blacklist) and
    // clear the HttpOnly cookie server-side. Fire-and-forget — local state
    // is always cleared regardless of whether the request succeeds.
    try {
      await ApiService.post("api/v2/auth/logout", {});
    } catch {
      // Non-fatal: cookie and local state are cleared below regardless
    }
    this.context.commit(Mutations.PURGE_AUTH);
  }

  @Action
  [Actions.REGISTER](credentials) {
    return ApiService.post("api/v1/auth/register", credentials)
      .then(() => {
        // this.context.commit(Mutations.SET_AUTH, data);
      })
      .catch(({ response }) => {
        if (response.data.error != undefined) {
          this.context.commit(Mutations.SET_ERROR, [response.data.error]);
        } else {
          this.context.commit(Mutations.SET_ERROR, [response.data.message]);
        }
      });
  }

  @Action
  [Actions.FORGOT_PASSWORD](payload) {
    return ApiService.post("forgot_password", payload)
      .then(() => {
        this.context.commit(Mutations.SET_ERROR, {});
      })
      .catch(({ response }) => {
        this.context.commit(Mutations.SET_ERROR, response.data.errors);
      });
  }

  @Action
  async [Actions.VERIFY_AUTH]() {
    if (this.isAuthenticated && this.user.email != undefined) return true;

    // Token is in HttpOnly cookie — cannot read it from JS.
    // Attempt to fetch user info; a 401 response means the cookie is absent/expired.
    try {
      const meIdentity = await ClientGlobalService.getMeIdentity();
      this.context.commit(Mutations.SET_USER, meIdentity);
      this.context.commit(Mutations.SET_AUTH, null);
    } catch (error) {
      const response = (error as any).response;
      if (!response) return false;
      this.context.commit(Mutations.SET_ERROR, response.data.message);
      if (
        response.data.errors &&
        response.data.errors[0] == "__TOKEN_HAS_NOT_BEEN_VALIDATED__"
      ) {
        this.context.commit(Mutations.PURGE_TWOFA);
        return false;
      } else {
        this.context.commit(Mutations.PURGE_AUTH);
        return false;
      }
    }

    // Fetch configuration separately — a failure here must NOT invalidate the session.
    try {
      const config = await ClientGlobalService.getConfiguration();
      this.context.commit(Mutations.SET_CONFIG, config);
    } catch {
      // Non-fatal: configuration fetch failed, proceed with authenticated state.
    }

    return true;
  }

  @Action
  [Actions.SET_LANG](credentials) {
    setLocale(credentials.language);
    ApiService.setHeader();
    return ApiService.put("api/v1/user/profile/language", credentials)
      .then(({ data }) => {
        this.context.commit(Mutations.SET_USER_LANG, credentials.language);
        location.reload();
      })
      .catch(({ response }) => {
        this.context.commit(Mutations.SET_ERROR, response.data.errors);
      });
  }

  @Action
  [Actions.SET_AVATAR](data) {
    this.context.commit(Mutations.SET_USER_AVATAR, data);
  }
}
