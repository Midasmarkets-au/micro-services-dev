<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <el-select
          v-model="criteria.status"
          :placeholder="$t('fields.status')"
          :disabled="isLoading"
        >
          <el-option
            v-for="item in EventShopRewardStatusOptions"
            :key="item.value"
            :label="item.label"
            :value="item.value"
          >
          </el-option>
        </el-select>
      </div>
    </div>
    <div class="card-body">
      <table class="table table-tenant-sm table-hover">
        <thead>
          <tr>
            <th>Id</th>
            <th>{{ $t("fields.status") }}</th>
            <th>{{ $t("fields.rewardName") }}</th>
            <th>{{ $t("fields.totalPoints") }}</th>
            <th>{{ $t("fields.operator") }}</th>
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
            <td>{{ item.id }}</td>
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
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted, watch } from "vue";
import {
  EventShopRewardStatusOptions,
  EventShopRewardRebateStatusTypes,
} from "@/core/types/shop/ShopPointsTypes";
import EventsServices from "../../../services/EventsServices";

const props = defineProps<{
  partyId: number;
}>();
const isLoading = ref(false);
const data = ref(<any>[]);
const criteria = ref<any>({
  page: 1,
  size: 20,
  partyId: props.partyId,
  status: null,
});
const accountSelected = ref(0);

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

watch(
  () => criteria.value.status,
  () => {
    fetchData(1);
  }
);

onMounted(() => {
  fetchData(1);
});
</script>
