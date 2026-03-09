<template>
  <div class="d-flex px-sm-15 px-3">
    <div class="w-100">
      <div class="d-flex">
        <div
          class="nav nav-pills nav-pills-custom overflow-auto flex-nowrap sub-menu pl-2 pr-2"
        >
          <div v-for="(item, index) in tab" :key="index">
            <div
              class="fw-semibold text-nowrap sub-menu-item"
              :disabled="
                !Can.cans(['EventShop']) ||
                status != EventPartyStatusTypes.Approved
              "
              :class="{ active: currentTab === item.key }"
              data-bs-toggle="pill"
              @click="currentTab = item.key"
              :href="item.url"
            >
              {{ item.value }}
            </div>
          </div>
        </div>
      </div>
      <div class="grid grid-cols-1 md:grid-cols-12 gap-4 items-stretch">
        <div class="md:col-span-4 flex">
          <div class="card card-flush w-100 h-100">
            <CurrentAwardCard v-if="props.step != 0" />
          </div>
        </div>
        <div class="md:col-span-8 flex">
          <InnerEventShopBanner v-if="props.step != 0" />
        </div>
      </div>
      <div class="tab-content">
        <div
          v-for="(item, index) in tab"
          :key="index"
          class="tab-pane fade"
          :class="{
            'show active': currentTab === item.key,
          }"
          :id="item.key"
        >
          <template v-if="currentTab === item.key">
            <component :is="item.element" />
          </template>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, inject } from "vue";
import Can from "@/core/plugins/ICan";
import PointsMall from "./view/PointsMall.vue";
import RedemptionHistory from "./view/RedemptionHistory.vue";
import PointsHistory from "./view/PointsHistory.vue";
import RewardsDetails from "./view/RewardsDetails.vue";
import RedemptionRules from "./view/RedemptionRules.vue";
import PointsRule from "./view/PointsRule.vue";
import i18n from "@/core/plugins/i18n";
import ClientGlobalInjectionKeys from "@/core/types/ClientGlobalInjectionKeys";
import { EventPartyStatusTypes } from "@/core/types/shop/ShopCustomerTypes";

import InnerEventShopBanner from "@/projects/client/modules/eventshop/components/InnerEventShopBanner.vue";
import CurrentAwardCard from "@/projects/client/modules/eventshop/components/CurrentAwardCard.vue";
import { useStore } from "@/store";
const store = useStore();
const tenancy = store.state.AuthModule.user.tenancy;

const userDetail = inject(ClientGlobalInjectionKeys.EVENT_SHOP_USER_DETAIL);
const status = ref(userDetail?.value?.status ? userDetail.value.status : 0);
const currentTab = ref("store");
const t = i18n.global.t;
const props = defineProps<{ step: number }>();
const tab = [
  {
    key: "store",
    value: t("title.eventShop"),
    url: "#store",
    element: PointsMall,
  },
  {
    key: "redemptionHistory",
    value: t("title.ordersHistory"),
    url: "#redemptionHistory",
    element: RedemptionHistory,
  },
  {
    key: "pointsHistory",
    value: t("title.pointsHistory"),
    url: "#pointsHistory",
    element: PointsHistory,
  },
  {
    key: "rewardsDetails",
    value: t("title.rewardDetails"),
    url: "#rewardsDetails",
    element: RewardsDetails,
  },
  {
    key: "redemptionRules",
    value: t("title.redemptionRules"),
    url: "#redemptionRules",
    element: RedemptionRules,
  },
  {
    key: "pointsRule",
    value: t("title.pointsRules"),
    url: "#pointsRule",
    element: PointsRule,
  },
];
if (tenancy === "sea") {
  //remove rewardsDetails tab
  tab.splice(3, 1);
}
</script>
