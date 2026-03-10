import { computed } from "vue";

export enum CommentType {
  Unknown = 0,
  User = 10,
  Account = 20,
  TradeAccount = 21,
  Verification = 30,
  Application = 31,
  Lead = 40,
  Deposit = 50,
  Withdrawal = 51,
  Transfer = 52,
}

export const commentTypeOptions = computed(() => {
  return Object.keys(CommentType)
    .filter((key) => !isNaN(Number(key)))
    .map((key) => {
      return {
        value: Number(key),
        label: CommentType[key],
      };
    });
});
