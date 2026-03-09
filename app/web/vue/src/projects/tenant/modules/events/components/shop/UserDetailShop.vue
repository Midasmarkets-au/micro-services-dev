<template>
  <el-drawer
    v-model="drawer"
    direction="rtl"
    size="60%"
    :before-close="close"
    :destroy-on-close="true"
  >
    <template #header="{ titleId }">
      <div class="d-flex align-items-center gap-5">
        <div :id="titleId">
          <span>{{ $t("fields.userShopDetail") }}</span>
        </div>
        <el-tag size="large" type="info">
          <span>{{ $t("fields.name") }}: </span>
          <span>{{ userName }}</span>
        </el-tag>
        <el-tag size="large">
          <span>{{ $t("fields.totalPoints") }}: </span>
          <span><ShopPoints :points="userData.totalPoint" /></span>
        </el-tag>
        <el-tag size="large" type="success">
          <span>{{ $t("fields.joinDate") }}: </span>
          <span>
            <TimeShow type="reportDate" :date-iso-string="userData.createdOn"
          /></span>
        </el-tag>
      </div>
    </template>
    <div>
      <headerMenu :tabs="tabs" @change-Tab="handleTabs" />
      <template v-for="tab in tabs" :key="tab.id">
        <template v-if="currentTab === tab.id">
          <component
            :is="getComponent(tab.component)"
            :partyId="partyId"
          ></component>
        </template>
      </template>
    </div>
  </el-drawer>
</template>

<script lang="ts" setup>
import { ref } from "vue";
import EventsServices from "../../services/EventsServices";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import i18n from "@/core/plugins/i18n";
import UserOrders from "./UserDetailShopComp/UserOrders.vue";
import UserPointsTransaction from "./UserDetailShopComp/UserPointsTransaction.vue";
import UserRewards from "./UserDetailShopComp/UserRewards.vue";
import UserRebate from "./UserDetailShopComp/UserRewardRebate.vue";
import headerMenu from "./HeaderMenu.vue";
const t = i18n.global.t;
const drawer = ref(false);
const initLoading = ref(false);
const userName = ref("");
const userData = ref<any>({});
const partyId = ref<number>(0);
const show = async (_partyId: number) => {
  drawer.value = true;
  partyId.value = _partyId;
  await fetchUserData(_partyId);
};
const currentTab = ref(1);

const handleTabs = (id: number) => {
  currentTab.value = id;
};

const close = () => {
  drawer.value = false;
  currentTab.value = 1;
  userData.value = {};
  userName.value = "";
  partyId.value = 0;
};

const componentMap = {
  Orders: UserOrders,
  Rebate: UserRebate,
  Rewards: UserRewards,
  Points: UserPointsTransaction,
};

const getComponent = (componentName: string) => {
  return componentMap[componentName];
};
const tabs = ref([
  {
    name: t("title.pointTransaction"),
    component: "Points",
    id: 1,
  },
  {
    name: t("title.shopOrders"),
    component: "Orders",
    id: 2,
  },
  {
    name: t("title.shopRewards"),
    component: "Rewards",
    id: 3,
  },
  {
    name: t("title.rewardRebate"),
    component: "Rebate",
    id: 4,
  },
]);

const fetchUserData = async (_partyId: number) => {
  initLoading.value = true;
  try {
    const res = await EventsServices.queryCustomerList({ partyId: _partyId });
    if (!res.data[0]) {
      MsgPrompt.error(t("error.userNotFound"));
      drawer.value = false;
      return;
    }
    userData.value = res.data[0];
    userName.value = userData.value.nativeName;
  } catch (e) {
    console.log(e);
    MsgPrompt.error(t("error.failedToFetchData"));
    drawer.value = false;
  }
  initLoading.value = false;
};

defineExpose({
  show,
});
</script>
<style scoped lang="scss">
:deep .el-drawer__body {
  border-top: 1px solid var(--el-border-color);
}
</style>
