<template>
  <ul
    class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
  >
    <li
      class="nav-item"
      v-for="tab in EventShopPointTransactionTypesOptions"
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
    <div class="card-header d-flex align-items-center justify-content-end">
      <div class="card-toolbar">
        <div class="d-flex gap-2 me-4">
          <el-input
            class="w-150px"
            v-model="criteria.ticket"
            placeholder="Ticket No."
            :disabled="isLoading"
            clearable
          />
          <el-input
            class="w-150px"
            v-model="criteria.accountNumber"
            placeholder="Account No."
            :disabled="isLoading"
            clearable
          />
        </div>

        <div>
          <el-button
            type="primary"
            @click="fetchData(1)"
            :loading="isLoading"
            >{{ $t("action.search") }}</el-button
          >
          <el-button @click="clearSearchCriteria" :disabled="isLoading">{{
            $t("action.clear")
          }}</el-button>
        </div>
      </div>
    </div>
    <div class="card-body">
      <table class="table table-tenant-sm table-hover">
        <thead>
          <tr>
            <th>{{ $t("fields.user") }}</th>
            <th>{{ $t("fields.status") }}</th>
            <th>{{ $t("fields.uid") }}</th>
            <th>{{ $t("fields.accountNo") }}</th>
            <th>{{ $t("fields.ticket") }}</th>
            <th>{{ $t("fields.points") }}</th>
            <th>{{ $t("fields.sourceType") }}</th>
            <th>{{ $t("fields.createdOn") }}</th>
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
              <UserInfo :user="item.user" class="me-2" />
            </td>
            <td>
              <el-tag :type="getStatusTag(item.status)">{{
                getStatusLabel(item.status)
              }}</el-tag>
            </td>
            <td>{{ item.targetAccountUid }}</td>
            <td>{{ item.source.accountNumber }}</td>
            <td>{{ item.ticket }}</td>
            <td><ShopPoints :points="item.point" /></td>
            <td>
              {{
                EventShopPointTransactionSourceOptions[item.sourceType].label
              }}
            </td>
            <td>
              <TimeShow type="GMToneLiner" :date-iso-string="item.createdOn" />
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
  EventShopPointTransactionTypes,
  EventShopPointTransactionTypesOptions,
  EventShopPointTransactionSourceOptions,
} from "@/core/types/shop/ShopPointsTypes";
import EventsServices from "../services/EventsServices";
import RewardDetail from "../components/shop/RewardDetail.vue";
const isLoading = ref(false);
const data = ref(<any>[]);
const rewardDetailRef = ref<any>(null);
const criteria = ref<any>({
  ticket: "",
  accountNumber: "",
  page: 1,
  size: 25,
  status: EventShopPointTransactionTypes.Success,
});

const accountSelected = ref(0);
const tab = ref<any>(EventShopPointTransactionTypes.Success);
const isTabActive = (_tab: any) => {
  return tab.value === _tab;
};

const changeTab = (_value: any) => {
  tab.value = _value;
  criteria.value.status = tab.value;
  if (tab.value === 5) {
    criteria.value.status = null;
  }
  fetchData(1);
};

const fetchData = async (page: number) => {
  isLoading.value = true;
  criteria.value.page = page;
  try {
    const res = await EventsServices.queryShopPointTransaction(criteria.value);
    data.value = res.data;
    criteria.value = res.criteria;
  } catch (error) {
    console.error(error);
  }
  isLoading.value = false;
};

function getStatusTag(status: EventShopPointTransactionTypes) {
  switch (status) {
    case EventShopPointTransactionTypes.Pending:
      return "warning";
    case EventShopPointTransactionTypes.Success:
      return "success";
    case EventShopPointTransactionTypes.Fail:
      return "danger";
  }
}

const getStatusLabel = (value: number) => {
  const option = EventShopPointTransactionTypesOptions.value.find(
    (option) => option.value === value
  );
  return option ? option.label : "";
};

const clearSearchCriteria = async () => {
  criteria.value = {
    ticket: "",
    accountNumber: "",
    page: 1,
    pageSize: 10,
    status: EventShopPointTransactionTypes.Success,
  };
  await fetchData(1);
};

const selectedAccount = (id: number) => {
  accountSelected.value = id;
};

onMounted(() => {
  fetchData(1);
});
</script>
