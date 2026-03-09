// import i18n from "@/core/plugins/i18n";
// import store from "@/store";
// import { computed } from "vue";
// import { PublicSetting } from "@/core/types/ConfigTypes";
// const { t } = i18n.global;

// export enum PlatformTypes {
//   MetaTrader4Co = 10,
//   MetaTrader4CoDemo = 11,
//   MetaTrader4 = 20,
//   MetaTrader4Demo = 21,
//   MetaTrader5 = 30,
//   MetaTrader5Demo = 31,
//   CTrader = 40,
// }

// export type ServiceInfoType = {
//   serverName: string;
//   platform: PlatformTypes;
//   platformName: string;
// };

// export type ServiceMapType = Record<number, ServiceInfoType>;

// export const ConfigAllPlatformSelections = computed(() => {
//   return [
//     {
//       label: t(`type.platform.${PlatformTypes.MetaTrader4Co}`),
//       value: PlatformTypes.MetaTrader4Co,
//       iconPath: "/images/icons/brand/mt4.svg",
//     },
//     {
//       label: t(`type.platform.${PlatformTypes.MetaTrader4}`),
//       value: PlatformTypes.MetaTrader4,
//       iconPath: "/images/icons/brand/mt4.svg",
//     },
//     {
//       label: t(`type.platform.${PlatformTypes.MetaTrader5}`),
//       value: PlatformTypes.MetaTrader5,
//       iconPath: "/images/icons/brand/mt5.svg",
//     },
//   ];
// });

// export const ConfigRealPlatformSelections = computed(() => {
//   const projectConfig: PublicSetting = store.state.AuthModule.config;
//   return projectConfig?.tradingPlatformAvailable?.map(
//     (availablePlatformId: number) => {
//       return {
//         label: t(`type.platform.${availablePlatformId}`),
//         value: availablePlatformId,
//         iconPath: `/images/icons/brand/${
//           {
//             [PlatformTypes.MetaTrader4Co]: "mt4",
//             [PlatformTypes.MetaTrader4]: "mt4",
//             [PlatformTypes.MetaTrader5]: "mt5",
//           }[availablePlatformId]
//         }.svg`,
//       };
//     }
//   );
// });

// export const ConfigDemoPlatformSelections = computed(() => {
//   const projectConfig: PublicSetting = store.state.AuthModule.config;
//   return projectConfig?.demoTradingPlatformAvailable?.map(
//     (availablePlatformId: number) => {
//       return {
//         label: t(`type.platform.${availablePlatformId}`),
//         value: availablePlatformId,
//         iconPath: `/images/icons/brand/${
//           {
//             [PlatformTypes.MetaTrader4CoDemo]: "mt4",
//             [PlatformTypes.MetaTrader4Demo]: "mt4",
//             [PlatformTypes.MetaTrader5Demo]: "mt5",
//           }[availablePlatformId]
//         }.svg`,
//       };
//     }
//   );
// });

// // Object.keys(PlatformTypes)
// //   .filter((key) => isNaN(Number(key)) && (keys ? keys.includes(key) : true))
// //   .map((key) => ({
// //     label: t(`type.platform.${PlatformTypes[key]}`),
// //     value: PlatformTypes[key],
// //   }));
