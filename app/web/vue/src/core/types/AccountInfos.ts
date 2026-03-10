import i18n from "@/core/plugins/i18n";
const { t } = i18n.global;
import store from "@/store";
import { computed } from "vue";
import { PublicSetting } from "@/core/types/ConfigTypes";
import { getUserTenancy, tenancies } from "./TenantTypes";

export enum AccountRoleTypes {
  // Unknown = 0,
  // Sales = 10,
  // Ib = 20,
  // Client = 100,

  Unknown = 0,
  Sales = 100,
  Rep = 110,
  Broker = 200,
  IB = 300,
  Client = 400,
  System = 1,
  SuperAdmin = 2,
  TenantAdmin = 10,
  Wholesale = 310,
  Guest = 1000,
}
export enum AccountTypes {
  Unknown = 0,
  // Individual = 1,
  // Joint = 2,
  // Corp = 3,
  Standard = 4,
  // Pro = 5,
  Alpha = 6,
  // SwapFreeStandard = 7,
  // SwapFreePro = 8,
  // SwapFreeAlpha = 9,
  // Wholesale = 10,
  // Advantage = 11,
  // Affiliate = 12,
  // Vn = 13,
  // AlphaPlus = 14,
  // SEAPRO = 21,
  // Elite = 22,
}

export enum AccountTypesJp {
  JpSTDCFD = 15,
  JpSTDIND = 16,
  JpSTDFX = 17,
  JpALPCFD = 18,
  JpALPIND = 19,
  JpALPFX = 20,
}

export enum AccountStatusTypes {
  Activate = 0,
  Pause = 1,
  Inactivated = 2,
}

export enum AccountCategory {
  Wire = -2,
  Ips = -3,
}

export enum AccountLanguage {
  English = -1,
  Simply = -2,
  Traditional = -3,
}

export enum AccountOpenAt {
  Bvi = -1,
  Cn = -2,
}

export enum EventsAcountTypes {
  Sales = 100,
  IB = 300,
  Client = 400,
}

// for the transactions
export enum TransactionAccountTypes {
  Unknown = 0,
  Wallet = 1,
  TradeAccount = 2,
}

export const AccountStatusOptions = [
  {
    label: t("status.normal"),
    value: AccountStatusTypes.Activate,
  },
  {
    label: t("status.paused"),
    value: AccountStatusTypes.Pause,
  },
  {
    label: t("status.closed"),
    value: AccountStatusTypes.Inactivated,
  },
];

export const ConfigEventAccountTypeSelections = computed(() => {
  return Object.keys(EventsAcountTypes)
    .filter((item: string | number) => !isNaN(Number(item)))
    .map((typeNum) => ({
      label: t(`type.accountRole.${typeNum}`),
      value: Number(typeNum),
    }));
});

export const ConfigAllAccountTypeSelections = computed(() => {
  if (getUserTenancy() === tenancies.jp) {
    return [
      {
        label: "JP STD CFD",
        value: AccountTypesJp.JpSTDCFD,
      },
      {
        label: "JP STD IND",
        value: AccountTypesJp.JpSTDIND,
      },
      {
        label: "JP STD FX",
        value: AccountTypesJp.JpSTDFX,
      },
      {
        label: "JP ALP CFD",
        value: AccountTypesJp.JpALPCFD,
      },
      {
        label: "JP ALP IND",
        value: AccountTypesJp.JpALPIND,
      },
      {
        label: "JP ALP FX",
        value: AccountTypesJp.JpALPFX,
      },
    ];
  } else {
    return Object.keys(AccountTypes)
      .filter(
        (item: string | number) =>
          !isNaN(Number(item)) && Number(item) !== AccountTypes.Unknown
      )
      .map((typeNum) => ({
        label: t(`type.account.${typeNum}`),
        value: Number(typeNum),
      }));
  }
});

export const ConfigAccountTypeSelections = computed(() => {
  const projectConfig: PublicSetting = store.state.AuthModule.config;
  return projectConfig?.accountTypeAvailable?.map((item) => ({
    label: t(`type.account.${item}`),
    value: item,
    iconPath:
      {
        1: "/images/icons/communication/com005.svg",
        2: "/images/icons/finance/fin006.svg",
      }[item] ?? "/images/icons/communication/com005.svg",
  }));
});

type SelectionValue = "FX" | "IND" | "CFD";

export const JpAccountTypeSelection: {
  [key in AccountTypesJp]: SelectionValue;
} = {
  [AccountTypesJp.JpSTDCFD]: "CFD",
  [AccountTypesJp.JpSTDIND]: "IND",
  [AccountTypesJp.JpSTDFX]: "FX",
  [AccountTypesJp.JpALPCFD]: "CFD",
  [AccountTypesJp.JpALPIND]: "IND",
  [AccountTypesJp.JpALPFX]: "FX",
};

export const JpAccountStandOrAlpSelection: {
  [key in AccountTypesJp]: "Standard" | "Alpha";
} = {
  [AccountTypesJp.JpSTDCFD]: "Standard",
  [AccountTypesJp.JpSTDIND]: "Standard",
  [AccountTypesJp.JpSTDFX]: "Standard",
  [AccountTypesJp.JpALPCFD]: "Alpha",
  [AccountTypesJp.JpALPIND]: "Alpha",
  [AccountTypesJp.JpALPFX]: "Alpha",
};

export const ConfigDemoAccountTypeSelections = computed(() => {
  return [4].map((item) => ({
    label: t(`type.account.${item}`),
    value: item,
    iconPath:
      {
        1: "/images/icons/communication/com005.svg",
        2: "/images/icons/finance/fin006.svg",
      }[item] ?? "/images/icons/communication/com005.svg",
  }));
});

export const getAccountRoleSelectionsByKeys = async () => {
  // console.log(systemTypes);
  return [
    AccountRoleTypes.Sales,
    AccountRoleTypes.IB,
    AccountRoleTypes.Client,
    AccountRoleTypes.Rep,
  ].map((key) => ({
    label: t(`type.accountRole.${key}`),
    value: key,
  }));
};

export const getAccountTypesOptions = [
  // {
  //   label: t("type.account." + AccountTypes.Individual),
  //   value: AccountTypes.Individual,
  // },
  // { label: t("type.account." + AccountTypes.Joint), value: AccountTypes.Joint },
  // { label: t("type.account." + AccountTypes.Corp), value: AccountTypes.Corp },
  {
    label: t("type.account." + AccountTypes.Standard),
    value: AccountTypes.Standard,
  },
  // { label: t("type.account." + AccountTypes.Pro), value: AccountTypes.Pro },
  { label: t("type.account." + AccountTypes.Alpha), value: AccountTypes.Alpha },
  // {
  //   label: t("type.account." + AccountTypes.SwapFreeStandard),
  //   value: AccountTypes.SwapFreeStandard,
  // },
  // {
  //   label: t("type.account." + AccountTypes.SwapFreePro),
  //   value: AccountTypes.SwapFreePro,
  // },
  // {
  //   label: t("type.account." + AccountTypes.SwapFreeAlpha),
  //   value: AccountTypes.SwapFreeAlpha,
  // },
  // {
  //   label: t("type.account." + AccountTypes.Wholesale),
  //   value: AccountTypes.Wholesale,
  // },
  // {
  //   label: t("type.account." + AccountTypes.Advantage),
  //   value: AccountTypes.Advantage,
  // },
  // {
  //   label: t("type.account." + AccountTypes.Affiliate),
  //   value: AccountTypes.Affiliate,
  // },
  // {
  //   label: t("type.account." + AccountTypes.Elite),
  //   value: AccountTypes.Elite,
  // },
  // { label: t("type.account." + AccountTypes.Vn), value: AccountTypes.Vn },
  // {
  //   label: t("type.account." + AccountTypes.AlphaPlus),
  //   value: AccountTypes.AlphaPlus,
  // },
];
