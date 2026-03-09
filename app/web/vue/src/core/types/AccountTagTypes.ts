export const AccountTagTypes = {
  // Unknown: "Unknown",
  DailyConfirmEmail: "DailyConfirmEmail",
  Test: "Test",
  AddPips: "AddPips",
  SwapFree: "SwapFree",
  AddCommission: "AddCommission",
  PauseReleaseRebate: "PauseReleaseRebate",
};

export const getAllAccountTagTypes = (keys: string[]) =>
  Object.keys(keys ?? AccountTagTypes).map((key) => ({
    label: key,
    value: AccountTagTypes[key],
  }));
