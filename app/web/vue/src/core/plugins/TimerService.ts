import MsgPrompt from "@/core/plugins/MsgPrompt";
import router from "@/projects/client/config/router";
import { Actions } from "@/store/enums/StoreEnums";
import i18n from "@/core/plugins/i18n";
import store from "@/store";
import momentTimezone from "moment-timezone";
import moment from "moment/moment";

const { t } = i18n.global;

let timer: null | number = null;

const TimerService = {
  startTimer(callback, minutes?) {
    if (!minutes) {
      minutes = parseFloat(process.env.VUE_APP_TOKEN_VALIDITY_PERIOD || "5");
    }

    // console.log(minutes);
    // minutes = 0.03;

    if (timer !== null) {
      clearInterval(timer);
    }

    if (minutes <= 0) return;
    /**
     * store.state.AuthModule?.user?.roles.includes("TenantAdmin")
     */
    timer = setTimeout(() => {
      callback();
      this.clearTimer();
    }, minutes * 60 * 1000);
  },

  clearTimer() {
    if (timer !== null) {
      clearInterval(timer);
      timer = null;
    }
  },
};
// #VUE_APP_API_URL="https://demo.localhost:5100"
export const setTimerForLogout = (minutes?: number) => {
  TimerService.startTimer(async () => {
    await router.push({ name: "sign-in" });
    await store.dispatch(Actions.LOGOUT);
    TimerService.clearTimer();
    MsgPrompt.warning(t("tip.sessionExpiredLoginAgain"));
  }, minutes);
};

export const TimeZoneService = {
  getTimeZoneArea: () => {
    return momentTimezone.tz.guess();
  },
  getTimeZoneOffsetInMinutes: () => {
    return momentTimezone().utcOffset();
  },
  getTimeZoneOffsetInHours: () => {
    return momentTimezone().utcOffset() / 60;
  },
  parsePeriodIntoLocalTime: (period: Array<any>) => {
    let val = period;
    if (val && val.length > 0 && typeof val[0] !== "string") {
      val = [
        moment(val[0]).local().toISOString(),
        moment(val[1]).local().toISOString(),
      ];
    }
    return [val ? val[0] : null, val ? val[1] : null];
  },
  adjustDateToKeepsYearMonthDate: (
    dateToAdjust: Date,
    referenceDate?: Date
  ) => {
    referenceDate ??= new Date(dateToAdjust);
    dateToAdjust.setHours(referenceDate.getHours());
    dateToAdjust.setMinutes(referenceDate.getMinutes());
    dateToAdjust.setSeconds(referenceDate.getSeconds());
    dateToAdjust.setMilliseconds(referenceDate.getMilliseconds());
    return moment(dateToAdjust).local().toISOString();
  },
};

export function convertToLocalTime(utcDate, dstTimeZone) {
  const date = new Date(utcDate);
  const isDST = checkIsDST(date, dstTimeZone);
  const offsetHours = isDST ? 3 : 2;
  return new Date(date.getTime() + offsetHours * 60 * 60 * 1000).toISOString();
}
export function convertToLocalGMT(
  utcDate: string | null | undefined,
  dstTimeZone: string
) {
  if (!utcDate) return "";
  const date = new Date(utcDate);
  const isDST = checkIsDST(date, dstTimeZone);
  const offsetHours = isDST ? 3 : 2;
  return offsetHours;
}

export function checkIsDST(date: Date, timeZone: string) {
  return momentTimezone.tz(date, timeZone).isDST();
}
export function isDateInDST_US() {
  //store.state.AuthModule.config.utcEnabled
  // if (store.state.AuthModule.config?.HoursGapForMT5 == 3) {
  //   return true;
  // } else {
  //   return false;
  // }
  const now = momentTimezone.tz("America/New_York");
  const withoutDST = momentTimezone
    .tz(now, "America/New_York")
    .clone()
    .tz("Etc/GMT+5");
  return now.utcOffset() !== withoutDST.utcOffset();
}
