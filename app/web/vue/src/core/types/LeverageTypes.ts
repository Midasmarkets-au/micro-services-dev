import store from "@/store";
import { computed } from "vue";
import { PublicSetting } from "@/core/types/ConfigTypes";
import Can from "@/core/plugins/ICan";

export const ConfigLeverageSelections = computed(() => {
  const projectConfig: PublicSetting = store.state.AuthModule.config;

  // return (
  //   Can.can("Wholesale")
  //     ? projectConfig?.leverageForWholesaleAvailable
  //     : projectConfig?.leverageAvailable
  // )?.map((item) => ({
  //   label: item + ":1",
  //   value: item,
  // }));
  return (
    Can.can("Wholesale")
      ? projectConfig?.leverageForWholesaleAvailable
      : projectConfig?.leverageAvailable
  )?.map((item) => ({
    label: item + ":1",
    value: item,
  }));
});

export const backendConfigLeverageSelections = [
  { label: "20:1", value: 20 },
  { label: "25:1", value: 25 },
  { label: "30:1", value: 30 },
  { label: "50:1", value: 50 },
  { label: "100:1", value: 100 },
  { label: "200:1", value: 200 },
  { label: "400:1", value: 400 },
  { label: "500:1", value: 500 },
  { label: "1000:1", value: 1000 },
];
