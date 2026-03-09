import { defineRule, configure } from "vee-validate";
import AllRules from "@vee-validate/rules";
import { localize } from "@vee-validate/i18n";
import i18n from "@/core/plugins/i18n";
import enUS from "@vee-validate/i18n/dist/locale/en.json";
import zhCN from "@vee-validate/i18n/dist/locale/zh_CN.json";
import zhHK from "@vee-validate/i18n/dist/locale/zh_TW.json";
import zhTW from "@vee-validate/i18n/dist/locale/zh_TW.json";
import viVN from "@vee-validate/i18n/dist/locale/vi.json";
import thTH from "@vee-validate/i18n/dist/locale/th.json";
import jpJP from "@vee-validate/i18n/dist/locale/ja.json";
// import mnMN from "@vee-validate/i18n/dist/locale/mn.json";
import idID from "@vee-validate/i18n/dist/locale/id.json";
import msMY from "@vee-validate/i18n/dist/locale/ms_MY.json";

const { t } = i18n.global;

const myRules = {
  minMax: (value, [min, max]) => {
    // The field is empty so it should pass
    if (!value || !value.length) {
      return true;
    }
    const numericValue = Number(value);
    if (numericValue < min) {
      return `This field must be greater than ${min}`;
    }
    if (numericValue > max) {
      return `This field must be less than ${max}`;
    }
    return true;
  },
  minLength: (value, [limit]) => {
    // The field is empty so it should pass
    if (!value || !value.length) {
      return true;
    }
    if (value.length < limit) {
      return `This field must be at least ${limit} characters`;
    }
    return true;
  },

  accountPassword: (value) => {
    const regex =
      /^(?=.*[A-Za-z])(?=.*\d)(?=.*[!@#$%^&*()_+])[A-Za-z\d!@#$%^&*()_+]{8,}$/;
    if (!regex.test(value)) {
      return t("tip.accountPasswordValidate");
    }
    return true;
  },
  ...AllRules,
};

const initGlobalValidators = () => {
  Object.keys(myRules).forEach((rule) => {
    defineRule(rule, myRules[rule]);
  });
};

configure({
  generateMessage: localize({
    ["en-us"]: enUS,
    ["zh-cn"]: zhCN,
    ["zh-hk"]: zhHK,
    ["zh-tw"]: zhTW,
    ["vi-vn"]: viVN,
    ["th-th"]: thTH,
    ["jp-jp"]: jpJP,
    // ["mn-mn"]: mnMN,
    ["id-id"]: idID,
    ["ms-my"]: msMY,
  }),
});

export default initGlobalValidators;
