import i18n from "@/core/plugins/i18n";

const { t } = i18n.global;

export enum StockCmdType {
  LONG = 0,
  SHORT = 1,
  BUY_LIMIT = 2,
  SELL_LIMIT = 3,
  BUY_STOP = 4,
  SELL_STOP = 5,
  BALANCE = 6,
  CREDIT = 7,
}

export const StockTransactionTypes = [StockCmdType.LONG, StockCmdType.SHORT];
export const StockPendingTypes = [
  StockCmdType.BUY_LIMIT,
  StockCmdType.SELL_LIMIT,
  StockCmdType.BUY_STOP,
  StockCmdType.SELL_STOP,
];

export const getStockCmdSelections = (keys?: string[]) =>
  Object.keys(StockCmdType)
    .filter((key) => isNaN(Number(key)) && (keys ? keys.includes(key) : true))
    .map((key) => ({
      label: t(`type.cmd.${StockCmdType[key]}`),
      value: StockCmdType[key],
    }));
