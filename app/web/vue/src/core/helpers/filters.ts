import { LanguageCodes } from "@/core/types/LanguageTypes";
import { CurrencyTypes } from "@/core/types/CurrencyTypes";
import moment from "moment";

const filters = {
  toCurrency: (value: number, currencyId = 840, locale = "en-US") => {
    const hasDecimal = value % 1 !== 0; // 判断是否有小数部分
    const fractionDigits = hasDecimal ? 4 : 2;
    return new Intl.NumberFormat(
      LanguageCodes.activated.includes(locale) ? locale : LanguageCodes.enUS,
      {
        style: "currency",
        currency: CurrencyTypes[currencyId] ? CurrencyTypes[currencyId] : "USD",
        minimumFractionDigits: fractionDigits, // 至少
        maximumFractionDigits: fractionDigits, // 最多
      }
    ).format(value / 100);
  },
  toDateTime: (value: number, locale = "en-US") =>
    moment(value)
      .locale(
        LanguageCodes.activated.includes(locale) ? locale : LanguageCodes.enUS
      )
      .format("l LT"),

  getDateAndTimeFromISOString: (
    dateIsoString: string,
    format?: string,
    locale = "en-US"
  ) => {
    const date = moment(dateIsoString).locale(locale);

    if (date.year() === 1970 || date.year() === 1969) {
      return "-";
    }

    if (format) {
      return date.format(format);
    }
    if (date.year() === moment().year()) {
      return date.format("MM-DD HH:mm:ss");
    }
    return date.format("YYYY-MM-DD HH:mm:ss");
  },
};
export default filters;
