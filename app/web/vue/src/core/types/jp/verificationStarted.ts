import i18n from "@/core/plugins/i18n";
import { computed } from "vue";
const t = i18n.global.t;

export enum accountTypes {}

export const accountTypesSelection = computed(() => [
  {
    value: 1,
    label: t("fields.marginTrading"),
    children: [
      {
        value: 1,
        label: t("fields.standard"),
      },
      {
        value: 2,
        label: t("fields.alpha"),
      },
    ],
  },
  {
    value: 2,
    label: t("fields.cfdTrading"),
    children: [
      {
        value: 3,
        label: t("fields.standard"),
      },
      {
        value: 4,
        label: t("fields.alpha"),
      },
    ],
  },
  {
    value: 3,
    label: t("fields.productTrading"),
    children: [
      {
        value: 3,
        label: t("fields.standard"),
      },
      {
        value: 4,
        label: t("fields.alpha"),
      },
    ],
  },
]);
