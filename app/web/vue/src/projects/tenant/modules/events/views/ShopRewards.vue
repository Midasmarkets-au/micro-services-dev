<template>
  <ul
    class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
  >
    <li
      class="nav-item"
      v-for="tab in EventShopRewardStatusOptions"
      :key="tab.value"
    >
      <a
        class="nav-link text-active-primary pb-2"
        :class="[
          { active: isTabActive(tab.value) },
          { 'disabled opacity-50 pe-none': isLoading },
        ]"
        data-bs-toggle="tab"
        href="#"
        @click="changeTab(tab.value)"
        >{{ tab.label }}</a
      >
    </li>
  </ul>
  <div class="card">
    <div class="card-header d-flex align-items-center"></div>
    <div class="card-body">
      <table class="table table-tenant-sm table-hover">
        <thead>
          <tr>
            <th>{{ $t("fields.user") }}</th>
            <th>{{ $t("fields.status") }}</th>
            <th>{{ $t("fields.rewardName") }}</th>
            <th>{{ $t("fields.totalPoints") }}</th>
            <th>{{ $t("fields.operator") }}</th>
            <th>{{ $t("fields.createdOn") }}</th>
            <th>{{ $t("fields.action") }}</th>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && data.length == 0">
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr
            v-for="item in data"
            :key="item.id"
            :class="{
              'tr-select': item.id === accountSelected,
            }"
            @click="selectedAccount(item.id)"
          >
            <td>
              <UserInfo
                :email="item.eventPartyEmail"
                :name="item.eventPartyNativeName"
                :partyId="item.partyId"
                class="me-2"
              />
            </td>
            <td>
              <el-tag :type="getStatusTag(item.status)">{{
                getStatusLabel(item.status)
              }}</el-tag>
            </td>
            <td>{{ item.eventShopItemName }}</td>
            <td><ShopPoints :points="item.totalPoint" /></td>
            <td>{{ item.operatorName }}</td>
            <td>
              <TimeShow type="GMToneLiner" :date-iso-string="item.createdOn" />
            </td>
            <td>
              <el-button size="small" @click="showDetail(item)">{{
                $t("action.detail")
              }}</el-button>
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
  </div>
  <RewardDetail ref="rewardDetailRef" />
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";
import {
  EventShopRewardStatusOptions,
  EventShopRewardRebateStatusTypes,
} from "@/core/types/shop/ShopPointsTypes";
import EventsServices from "../services/EventsServices";
import RewardDetail from "../components/shop/RewardDetail.vue";
const isLoading = ref(false);
const data = ref(<any>[]);
const rewardDetailRef = ref<any>(null);
const criteria = ref<any>({
  page: 1,
  size: 25,
  status: EventShopRewardRebateStatusTypes.Pending,
});
const accountSelected = ref(0);
const tab = ref<any>();
const isTabActive = (_tab: any) => {
  return tab.value === _tab;
};

const changeTab = (_value: any) => {
  tab.value = _value;
  criteria.value.status = tab.value;
  if (tab.value === 6) {
    criteria.value.status = null;
  }
  fetchData(1);
};
const showDetail = (item: any) => {
  rewardDetailRef.value?.show(item);
};
const fetchData = async (page: number) => {
  isLoading.value = true;
  criteria.value.page = page;
  try {
    const res = await EventsServices.queryShoppRewards(criteria.value);
    data.value = res.data;
    criteria.value = res.criteria;
  } catch (error) {
    console.error(error);
  }
  isLoading.value = false;
};

const getStatusLabel = (value: number) => {
  const option = EventShopRewardStatusOptions.value.find(
    (option) => option.value === value
  );
  return option ? option.label : "";
};

function getStatusTag(status: EventShopRewardRebateStatusTypes) {
  switch (status) {
    case EventShopRewardRebateStatusTypes.Pending:
      return "warning";
    case EventShopRewardRebateStatusTypes.Succeed:
      return "success";
    case EventShopRewardRebateStatusTypes.Failed:
      return "danger";
    case EventShopRewardRebateStatusTypes.Processing:
      return "info";
  }
}
const selectedAccount = (id: number) => {
  accountSelected.value = id;
};
onMounted(() => {
  tab.value = EventShopRewardRebateStatusTypes.Pending;
  fetchData(1);
});
</script>
