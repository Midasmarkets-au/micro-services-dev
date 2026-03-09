import store from "@/store";
import { computed } from "vue";
import { TenantConfigType } from "@/core/types/ConfigTypes";
import i18n from "@/core/plugins/i18n";

// const tenantConfig = computed<TenantConfigType>(
//   () => store.state.AuthModule.config
// );
const { t } = i18n.global;

export const ConfigFundTypeSelections = computed(() => {
  const tenantConfig: TenantConfigType = store.state.AuthModule.config;
  const ll = tenantConfig?.fundTypeAvailable.map((fundTypeId) => ({
    // label: FundTypes[fundTypeId],
    label: t(`type.fundType.${fundTypeId}`),
    value: fundTypeId,
  }));
  return ll;
});

export const ConfigDefaultFundType = computed(() => {
  const tenantConfig: TenantConfigType = store.state.AuthModule.config;
  // console.log(tenantConfig);
  return tenantConfig?.defaultFundType;
});

export const getAllFundTypeSelections = (types?: Array<FundTypes>) => {
  types ??= Object.values(FundTypes).filter(
    (v) => typeof v === "number"
  ) as Array<FundTypes>;
  return types.map((fundTypeId) => ({
    label: t(`type.fundType.${fundTypeId}`),
    value: fundTypeId,
  }));
};

export enum FundTypes {
  Wire = 1,
  Ips = 2,
  FundType3 = 3,
  FundType4 = 4,
  FundType5 = 5,
  //Cash = 2,
  //Bonus = 3,
  //Reward = 4,
  //Commission = 11,
  //Operation = 12,
  //Customs = 13,
}
