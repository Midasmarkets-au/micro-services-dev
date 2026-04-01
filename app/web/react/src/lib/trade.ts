import { ServiceTypes } from '@/types/accounts';

export interface TradeBuySellDisplayInput {
  cmd?: number | null;
  type?: number | null;
  serviceId?: number | null;
  closeAt?: string | null;
  closeTime?: string | null;
}

export function handleTradeBuySellDisplay(
  trade: TradeBuySellDisplayInput,
): number | undefined {
  const cmd = trade.cmd ?? trade.type;
  if (cmd == null) return undefined;

  const isClosed = trade.closeAt != null || trade.closeTime != null;
  if (trade.serviceId === ServiceTypes.MetaTrader5 && isClosed) {
    return cmd === 0 ? 1 : 0;
  }
  return cmd;
}
