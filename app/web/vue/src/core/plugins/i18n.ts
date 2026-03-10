import { createI18n } from "vue-i18n";
import messages from "@/locales/index";
import { LanguageCodes } from "@/core/types/LanguageTypes";

const localLang = localStorage.getItem("language") || LanguageCodes.enUS;

const i18n = createI18n({
  legacy: false,
  locale: localLang,
  globalInjection: true,
  messages,
  fallbackLocale: LanguageCodes.all,
});
export default i18n;
