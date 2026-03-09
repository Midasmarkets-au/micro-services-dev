import store from "@/store";
import { computed } from "vue";
import i18n from "@/core/plugins/i18n";
import { PublicSetting } from "@/core/types/ConfigTypes";

const { t } = i18n.global;

export enum PlatformTypes {
  MetaTrader4 = 20,
  MetaTrader4Demo = 21,
  MetaTrader5 = 30,
  MetaTrader5Demo = 31,
}

export enum ServiceTypes {
  MetaTrader4Co = 10,
  MetaTrader4CoDemo = 11,
  MetaTrader4 = 20,
  MetaTrader4Demo = 21,
  MetaTrader5 = 30,
  MetaTrader5Demo = 31,
  CTrader = 40,
}

export type ServiceInfoType = {
  serverName: string;
  platform: PlatformTypes;
  platformName: string;
};

export type ServiceMapType = Record<number, ServiceInfoType>;

const ServiceTypeInfo = {
  [ServiceTypes.MetaTrader4Co]: {
    platform: PlatformTypes.MetaTrader4,
    img: "mt4",
  },
  [ServiceTypes.MetaTrader4CoDemo]: {
    platform: PlatformTypes.MetaTrader4Demo,
    img: "mt4",
  },
  [ServiceTypes.MetaTrader4]: {
    platform: PlatformTypes.MetaTrader4,
    img: "mt4",
  },
  [ServiceTypes.MetaTrader4Demo]: {
    platform: PlatformTypes.MetaTrader4Demo,
    img: "mt4",
  },
  [ServiceTypes.MetaTrader5]: {
    platform: PlatformTypes.MetaTrader5,
    img: "mt5",
  },
  [ServiceTypes.MetaTrader5Demo]: {
    platform: PlatformTypes.MetaTrader5Demo,
    img: "mt5",
  },
};

export const ConfigAllPlatformSelections = computed(() => {
  return [
    {
      label: "MT4",
      platform: PlatformTypes.MetaTrader4,
      iconPath: "/images/icons/brand/mt4.svg",
    },
    {
      label: "MT5",
      platform: PlatformTypes.MetaTrader5,
      iconPath: "/images/icons/brand/mt5.svg",
    },
  ];
});

export const demoPlatformSelections = computed(() => {
  return [
    {
      label: "MT4",
      platform: ServiceTypes.MetaTrader4CoDemo,
      iconPath: "/images/icons/brand/mt4.svg",
    },
    {
      label: "MT5",
      platform: ServiceTypes.MetaTrader5Demo,
      iconPath: "/images/icons/brand/mt5.svg",
    },
  ];
});

export const ConfigAllServiceSelections = computed(() => {
  return [
    {
      label: t(`type.service.${ServiceTypes.MetaTrader4Co}`),
      id: ServiceTypes.MetaTrader4Co,
      platform: PlatformTypes.MetaTrader4,
      iconPath: "/images/icons/brand/mt4.svg",
    },
    {
      label: t(`type.service.${ServiceTypes.MetaTrader4CoDemo}`),
      id: ServiceTypes.MetaTrader4CoDemo,
      platform: PlatformTypes.MetaTrader4,
      iconPath: "/images/icons/brand/mt4.svg",
    },
    {
      label: t(`type.service.${ServiceTypes.MetaTrader4}`),
      id: ServiceTypes.MetaTrader4,
      platform: PlatformTypes.MetaTrader4,
      iconPath: "/images/icons/brand/mt4.svg",
    },
    {
      label: t(`type.service.${ServiceTypes.MetaTrader4Demo}`),
      id: ServiceTypes.MetaTrader4Demo,
      platform: PlatformTypes.MetaTrader4,
      iconPath: "/images/icons/brand/mt4.svg",
    },
    {
      label: t(`type.service.${ServiceTypes.MetaTrader5}`),
      id: ServiceTypes.MetaTrader5,
      platform: PlatformTypes.MetaTrader5,
      iconPath: "/images/icons/brand/mt5.svg",
    },
    {
      label: t(`type.service.${ServiceTypes.MetaTrader5Demo}`),
      id: ServiceTypes.MetaTrader5Demo,
      platform: PlatformTypes.MetaTrader5,
      iconPath: "/images/icons/brand/mt5.svg",
    },
    // {
    //   label: t(`type.platform.${ServiceTypes.CTrader}`),
    //   value: ServiceTypes.CTrader,
    //   iconPath: "/images/icons/brand/mt4.svg",
    // },
  ];
});

export const ConfigRealServiceSelections = computed(() => {
  const projectConfig: PublicSetting = store.state.AuthModule.config;
  return projectConfig?.tradingPlatformAvailable?.map(
    (availableServiceId: number) => {
      return {
        label: t(
          `type.platform.${ServiceTypeInfo[availableServiceId].platform}`
        ),
        id: availableServiceId,
        platform: ServiceTypeInfo[availableServiceId].platform,
        iconPath: `/images/icons/brand/${ServiceTypeInfo[availableServiceId].img}.svg`,
      };
    }
  );
});

export const ConfigDemoPlatformSelections = computed(() => {
  const projectConfig: PublicSetting = store.state.AuthModule.config;
  return projectConfig?.demoTradingPlatformAvailable?.map(
    (availableServiceId: number) => {
      return {
        label: t(
          `type.platform.${ServiceTypeInfo[availableServiceId].platform}`
        ),
        id: availableServiceId,
        platform: ServiceTypeInfo[availableServiceId].platform,
        iconPath: `/images/icons/brand/${ServiceTypeInfo[availableServiceId].img}.svg`,
      };
    }
  );
});

export const getDemoPlatformSelections = (availableServices: any) => {
  return computed(() => {
    return availableServices.map((service: any) => {
      return {
        label: t(
          `type.platform.${ServiceTypeInfo[service.serviceId].platform}`
        ),
        id: service.serviceId,
        platform: ServiceTypeInfo[service.serviceId].platform,
        iconPath: `/images/icons/brand/${
          ServiceTypeInfo[service.serviceId].img
        }.svg`,
      };
    });
  });
};

export const ServiceToPlatform = {
  [ServiceTypes.MetaTrader4Co]: "MT4",
  [ServiceTypes.MetaTrader4CoDemo]: "MT4",
  [ServiceTypes.MetaTrader4]: "MT4",
  [ServiceTypes.MetaTrader4Demo]: "MT4",
  [ServiceTypes.MetaTrader5]: "MT5",
  [ServiceTypes.MetaTrader5Demo]: "MT5",
};
