import i18n from "@/core/plugins/i18n";
import { computed } from "vue";

const t = i18n.global.t;
const te = i18n.global.te;

const errorMessageMap = computed(() => ({
  ["__USER_IS_LOCKED_OUT__"]: t("tip.invalidEmailOrPassword"),
  ["invalid_grant"]: t("tip.invalidEmailOrPassword"),
}));

export const processErrorMessage = (error) => {
  const msgFromMap = errorMessageMap.value[error];
  if (msgFromMap) return msgFromMap;
  if (error.response) {
    if (error.response.data.message) error = error.response.data.message;
    else if (error.response.data) error = error.response.data;
  }
  error = checkCases(error);
  if (typeof error === "string") {
    if (te("error." + error) == true) {
      error = t("error." + error);
    } else {
      error = error.replace(/_/g, " ").replace(/__/g, "");
    }
  } else {
    error = t("error.unexpectedError");
  }
  return error;
};

const checkCases = (error) => {
  if (error == "Wallet address already exists") {
    return t("error.walletAddressAlreadyExists");
  } else return error;
};
