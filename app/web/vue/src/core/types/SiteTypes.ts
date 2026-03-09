import { computed } from "vue";
import i18n from "@/core/plugins/i18n";

const { t } = i18n.global;

export enum SiteTypes {
  Default = 0,
  BritishVirginIslands = 1,
  Australia = 2,
  China = 3,
  Taiwan = 4,
  Vietnam = 5,
  Japan = 6,
  Mongolia = 7,
  Malaysia = 8,
}

export const ConfigSiteTypesSelections = computed(() => {
  return Object.keys(SiteTypes)
    .filter((item: string | number) => !isNaN(Number(item)))
    .map((typeNum) => ({
      label: t(`type.siteType.${typeNum}`),
      value: Number(typeNum),
    }));
});

export const SiteTypesValues = computed(() => {
  return Object.values(SiteTypes).filter((value) => typeof value === "number");
});
