enum Actions {
  // action types
  ADD_BODY_CLASSNAME = "addBodyClassName",
  REMOVE_BODY_CLASSNAME = "removeBodyClassName",
  ADD_BODY_ATTRIBUTE = "addBodyAttribute",
  REMOVE_BODY_ATTRIBUTE = "removeBodyAttribute",
  ADD_CLASSNAME = "addClassName",
  VERIFY_AUTH = "verifyAuth",
  TOKEN_LOGIN = "tokenLogin",
  LOGIN = "login",
  TWOFA = "2fa",
  TWOFA_VERIFIED = "2faVerified",
  LOGOUT = "logout",
  REGISTER = "register",
  EMAIL_CONFIRMATION = "emailConfirmation",
  UPDATE_USER = "updateUser",
  FORGOT_PASSWORD = "forgotPassword",
  SET_BREADCRUMB_ACTION = "setBreadcrumbAction",
  SET_THEME_MODE_ACTION = "setThemeModeAction",
  SET_LANG = "setLang",
  SET_AVATAR = "setAvatar",
  UPDATE_CONFIG = "updateConfig",
  PUT_IB_CURRENT_ACCOUNT = "putIBCurrentAccount",
  PUT_IB_ACCOUNTS = "putIBAccounts",
  GET_IB_ACCOUNT_LIST = "getIBAccountList",

  PUT_CURRENT_TRADE_ACCOUNT = "putCurrentTradeAccount",

  PUT_SALES_ACCOUNT = "putSalesAccount",
  PUT_REP_ACCOUNT = "putRepAccount",
  PUT_TENANT_ISTAT = "putTenantIStat",
}

enum Mutations {
  // mutation types
  SET_CLASSNAME_BY_POSITION = "appendBreadcrumb",
  GET_USER = "getUser",
  PURGE_AUTH = "logOut",
  SET_AUTH = "setAuth",
  SET_USER = "setUser",
  SET_USER_AVATAR = "setUserAvatar",
  SET_USER_LANG = "setUserLang",
  SET_PASSWORD = "setPassword",
  SET_ERROR = "setError",
  SET_BREADCRUMB_MUTATION = "setBreadcrumbMutation",
  SET_LAYOUT_CONFIG_PROPERTY = "setLayoutConfigProperty",
  RESET_LAYOUT_CONFIG = "resetLayoutConfig",
  OVERRIDE_LAYOUT_CONFIG = "overrideLayoutConfig",
  OVERRIDE_PAGE_LAYOUT_CONFIG = "overridePageLayoutConfig",
  SET_BACKEND_LAYOUT_CONFIG_PROPERTY = "setLayoutConfigProperty",
  RESET_BACKEND_LAYOUT_CONFIG = "resetLayoutConfig",
  OVERRIDE_BACKEND_LAYOUT_CONFIG = "overrideLayoutConfig",
  OVERRIDE_BACKEND_PAGE_LAYOUT_CONFIG = "overridePageLayoutConfig",
  SET_THEME_MODE_MUTATION = "setThemeModeMutation",
  SOCKET_ONOPEN = "socketOnOpen",
  SOCKET_ONCLOSE = "socketOnClose",
  SOCKET_ONERROR = "socketOnError",
  SOCKET_ONMESSAGE = "socketOnMessage",
  SOCKET_RECONNECT = "socketReconnect",
  SOCKET_RECONNECT_ERROR = "socketReconnectError",
  PURGE_TWOFA = "twoFA",
  GET_CURRENT_IB_ACCOUNT = "getCurrentIBAccount",

  SET_IB_CURRENT_ACCOUNT = "setIBCurrentAccount",
  SET_IB_ACCOUNTS = "setIBAccounts",

  SET_CURRENT_TRADE_ACCOUNT = "setCurrentTradeAccount",

  SET_SALES_ACCOUNT = "setSalesAccount",
  SET_REP_ACCOUNT = "setRepsAccount",
  SET_CONFIG = "setConfig",
  SET_HAS_USER_VERIFIED_2FA = "setHasUserVerified2Fa",

  SET_TENANT_ISTAT = "setTenantIStat",
}

export { Actions, Mutations };
