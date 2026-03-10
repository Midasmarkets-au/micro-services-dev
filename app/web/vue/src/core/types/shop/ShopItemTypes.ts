import i18n from "@/core/plugins/i18n";
import { computed } from "vue";

const { t } = i18n.global;
export enum EventShopItemStatusTypes {
  Draft = 0,
  Published = 1,
  Closed = 2,
}

export enum EventShopItemTypes {
  Product = 0,
  Reward = 1,
}

export const EventShopItemOptions = computed(() => [
  { label: t("fields.products"), value: EventShopItemTypes.Product },
  { label: t("fields.rewards"), value: EventShopItemTypes.Reward },
]);

export enum EventItemTypes {
  Product = 0,
  ClientReward = 1,
  AgentReward = 2,
  SalesReward = 3,
}

export const RewardRolesOptions = computed(() => [
  { label: t("fields.client"), value: EventItemTypes.ClientReward },
  { label: t("fields.agent"), value: EventItemTypes.AgentReward },
  { label: t("fields.sales"), value: EventItemTypes.SalesReward },
]);

export const allOPtions = computed(() => [
  { label: t("fields.products"), value: EventItemTypes.Product },
  { label: "Client Reward", value: EventItemTypes.ClientReward },
  { label: "Agent Rewawrd", value: EventItemTypes.AgentReward },
  { label: "Sales Reward", value: EventItemTypes.SalesReward },
]);

export enum EventShopRewardTypes {
  Point1000 = 1,
  Point3000 = 3,
  Point5000 = 5,
  Point250 = 25,
}
export const RewardTypesOptions = [
  {
    label: "1000 Point Card",
    value: EventShopRewardTypes.Point1000,
  },
  {
    label: "3000 Point Card",
    value: EventShopRewardTypes.Point3000,
  },
  {
    label: "5000 Point Card",
    value: EventShopRewardTypes.Point5000,
  },
  {
    label: "250 Point Card",
    value: EventShopRewardTypes.Point250,
  },
];

export enum EventShopItemCategory {
  travelTheWorld = 24,
  chineseBrandSkinCare = 23,
  H520 = 21,
  midAutumnFestival = 22,
  bcrMerch = 0,
  luxury = 1,
  electronics = 2,
  vehicle = 3,
  homeAppliances = 4,
  luxuryFurniture = 20,
  luxuryFood = 5,
  tobacco = 6,
  watch = 7,
  jewelry = 8,
  giftCard = 9,
  travelPackage = 10,
  skinCare = 11,
  culterySet = 12,
  phone = 13,
  furniture = 14,
  clock = 15,
  outdoor = 16,
  bag = 17,
  art = 18,
  makeup = 19,
}

export const EventShopItemCategoryOptions = computed(() => [
  {
    label: t("type.eventShopCategory.travelTheWorld"),
    value: EventShopItemCategory.travelTheWorld,
  },

  {
    label: t("type.eventShopCategory.chineseBrandSkinCare"),
    value: EventShopItemCategory.chineseBrandSkinCare,
  },
  // {
  //   label: t("type.eventShopCategory.chineseValentinesDay"),
  //   value: EventShopItemCategory.H520,
  // },
  // {
  //   label: t("type.eventShopCategory.midAutumnFestival"),
  //   value: EventShopItemCategory.midAutumnFestival,
  // },
  {
    label: t("type.eventShopCategory.bcrMerch"),
    value: EventShopItemCategory.bcrMerch,
  },
  {
    label: t("type.eventShopCategory.luxury"),
    value: EventShopItemCategory.luxury,
  },
  {
    label: t("type.eventShopCategory.electronics"),
    value: EventShopItemCategory.electronics,
  },
  {
    label: t("type.eventShopCategory.vehicle"),
    value: EventShopItemCategory.vehicle,
  },
  {
    label: t("type.eventShopCategory.homeAppliances"),
    value: EventShopItemCategory.homeAppliances,
  },
  {
    label: t("type.eventShopCategory.luxuryFood"),
    value: EventShopItemCategory.luxuryFood,
  },
  {
    label: t("type.eventShopCategory.luxuryFurniture"),
    value: EventShopItemCategory.luxuryFurniture,
  },
  // {
  //   label: t("type.eventShopCategory.tobacco"),
  //   value: EventShopItemCategory.tobacco,
  // },
  {
    label: t("type.eventShopCategory.watch"),
    value: EventShopItemCategory.watch,
  },
  {
    label: t("type.eventShopCategory.jewelry"),
    value: EventShopItemCategory.jewelry,
  },
  {
    label: t("type.eventShopCategory.giftCard"),
    value: EventShopItemCategory.giftCard,
  },
  {
    label: t("type.eventShopCategory.travelPackage"),
    value: EventShopItemCategory.travelPackage,
  },
  {
    label: t("type.eventShopCategory.skinCare"),
    value: EventShopItemCategory.skinCare,
  },
  {
    label: t("type.eventShopCategory.culterySet"),
    value: EventShopItemCategory.culterySet,
  },

  // {
  //   label: t("type.eventShopCategory.phone"),
  //   value: EventShopItemCategory.phone,
  // },
  // {
  //   label: t("type.eventShopCategory.furniture"),
  //   value: EventShopItemCategory.furniture,
  // },
  // {
  //   label: t("type.eventShopCategory.vehicle"),
  //   value: EventShopItemCategory.vehicle,
  // },
  // {
  //   label: t("type.eventShopCategory.clock"),
  //   value: EventShopItemCategory.clock,
  // },
  // {
  //   label: t("type.eventShopCategory.watch"),
  //   value: EventShopItemCategory.watch,
  // },
  // {
  //   label: t("type.eventShopCategory.outdoor"),
  //   value: EventShopItemCategory.outdoor,
  // },
  // { label: t("type.eventShopCategory.bag"), value: EventShopItemCategory.bag },
  // { label: t("type.eventShopCategory.art"), value: EventShopItemCategory.art },
  // {
  //   label: t("type.eventShopCategory.makeup"),
  //   value: EventShopItemCategory.makeup,
  // },
]);
