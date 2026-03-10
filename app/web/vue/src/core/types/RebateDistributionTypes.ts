export enum RebateDistributionTypes {
  Direct = 1,
  Allocation = 2,
  LevelSet = 3,
}

export const isDirect = (type: RebateDistributionTypes) =>
  type === RebateDistributionTypes.Direct;
