import i18n from "@/core/plugins/i18n";
import { is } from "@vee-validate/rules";
const { t } = i18n.global;

//lang equal to the position of the translated file
// for example, getOptions(obj, "action") then it will be t("action." + key.charAt(0).toLowerCase() + key.slice(1)
export function getOptions(obj, lang?: string, isKey = true) {
  const keys = Object.keys(obj);
  const values = Object.values(obj);
  const filteredKeys = keys.filter((item: string | number) =>
    isNaN(Number(item))
  );
  let res;
  if (isKey) {
    res = filteredKeys.map((key) => ({
      label:
        lang == undefined
          ? key
          : t(lang + "." + key.charAt(0).toLowerCase() + key.slice(1)),
      value: obj[key],
    }));
  } else {
    res = filteredKeys.map((key) => ({
      label: lang == undefined ? key : t(lang + "." + obj[key]),
      value: obj[key],
    }));
  }

  return res;
}
