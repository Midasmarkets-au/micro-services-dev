import { AccountRoleTypes, AccountTypes } from "@/core/types/AccountInfos";
import { PlatformTypes } from "@/core/types/ServiceTypes";
import { FundTypes } from "@/core/types/FundTypes";

export interface ApplicationApproveSupplement {
  role: AccountRoleTypes;
  referCode: string | null;
  salesDividedGroup: string | null;
  leverage: number | null;
  platform: PlatformTypes | null;
  serviceId: number | null;
  currencyId: number | null;
  accountType: AccountTypes | null;
  fundType: FundTypes;
  accountNumber: string | number | null;
  accountUid: string | number | null;

  /**
   * [Optional]
   *
   * if (agentAccountId == null), assign the account to the corresponding agent account
   * else, the account should be TOP IB(Broker)
   */
  agentAccountId: number | null;

  /**
   * [Required] when role is not Sales
   *
   * Both Agent and Client should be assigned to a Sales
   */
  salesAccountId: number | null;

  repAccountId: number | null;

  /**
   * [Required] when role is Agent/Broker
   *
   * for creating a new Agent/Broker with its own group
   * if (ibSelfGroup != null) ibSelfGroup should be UNIQUE
   */
  agentSelfGroup: string | null;

  /**
   * [Required] when role is Sales
   *
   * for creating a new Sales with its own code
   * if (salesSelfCode != null) salesSelfCode should be UNIQUE
   */
  salesSelfGroup: string | null;
}
const cc = {
  Id: 46526,
  TradeTicket: 3506963,
  ReceivingAccount: 53738697,
  RebateRate: 2.4,
  Pips: 0.0,
  PipsRate: 0.0,
  Commission: 0.0,
  CommissionRate: 0.0,
  Rebate: 0.12,
  OriginalRebate: 0.12,
  IsSubmitted: true,
  IsDuplicate: false,
  ReceivingTicket: 3507759,
  CreatedAt: "2017-05-30T23:45:03",
  UpdatedAt: "2020-03-10T20:12:57",
};
