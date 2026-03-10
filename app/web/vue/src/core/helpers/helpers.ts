import moment from "moment";
import { isDateInDST_US } from "@/core/plugins/TimerService";

export const convertGMT = (time, isStartTime, defaultTime = false) => {
  const isDST = isDateInDST_US();
  const timeOffset = isStartTime == "start" ? ":00:00.000[Z]" : ":59:59.000[Z]";
  return time
    ? moment(time).format(`YYYY-MM-DD[T]${isDST ? 21 : 22}${timeOffset}`)
    : defaultTime
    ? moment.utc().format(`YYYY-MM-DD[T]${isDST ? 21 : 22}${timeOffset}`)
    : null;
};

export const convertTradeTime = (from, to) => {
  const isDST = isDateInDST_US();
  const createdFrom = from
    ? moment(from)
        .subtract(1, "day")
        .format(`YYYY-MM-DD[T]${isDST ? 21 : 22}:00:00.000[Z]`)
    : moment
        .utc()
        .subtract(1, "day")
        .format(`YYYY-MM-DD[T]${isDST ? 21 : 22}:00:00.000[Z]`);

  const createdTo = to
    ? moment(to).format(`YYYY-MM-DD[T]${isDST ? 20 : 21}:59:59.000[Z]`)
    : moment.utc().format(`YYYY-MM-DD[T]${isDST ? 20 : 21}:59:59.000[Z]`);

  return [createdFrom, createdTo];
};

export const handleCriteriaTradeTime = (
  periodVal,
  criteria,
  defualtTime = true
) => {
  if (defualtTime) {
    if (periodVal && periodVal.length > 0) {
      const [from, to] = convertTradeTime(periodVal[0], periodVal[1]);
      criteria.value.from = from;
      criteria.value.to = to;
    } else {
      criteria.value.from = null;
      criteria.value.to = null;
    }
  } else {
    if (periodVal[0] !== null) {
      const [from, to] = convertTradeTime(periodVal[0], null);
      criteria.value.from = from;
    } else {
      criteria.value.from = null;
    }
    if (periodVal[1] !== null) {
      const [from, to] = convertTradeTime(periodVal[0], periodVal[1]);
      criteria.value.to = to;
    } else {
      criteria.value.to = null;
    }
  }
};

import { ServiceTypes } from "../types/ServiceTypes";

export const handleTradeBuySellDisplay = (trade) => {
  if (trade.serviceId == ServiceTypes.MetaTrader5 && trade.closeAt != null) {
    return trade.cmd == 0 ? 1 : 0;
  } else {
    return trade.cmd;
  }
};

export const getItemByValue = (value, items) => {
  if (Array.isArray(items)) {
    return (
      items.find((item) => item.value === value) || { label: "", value: "" }
    );
  } else if (typeof items === "object") {
    return (
      Object.values(items).find((item: any) => item.value === value) || {
        label: "",
        value: "",
      }
    );
  }
  return {
    label: "",
    value: "",
  };
};

export const handleTradeFormatted = (price, digits) => {
  // Format to specified digits

  if (isNaN(price) || price === null || price === undefined) {
    return price;
  }
  const tradeFormatted = parseFloat(price.toFixed(digits));
  return tradeFormatted;
};
