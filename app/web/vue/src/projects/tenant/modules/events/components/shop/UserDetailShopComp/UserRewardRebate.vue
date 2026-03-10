<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <el-input
          v-model="criteria.ticket"
          :placeholder="$t('fields.ticket')"
          :disabled="isLoading"
          clearable
        />

        <el-select
          v-model="criteria.status"
          :placeholder="$t('fields.status')"
          class="ms-4"
          :disabled="isLoading"
        >
          <el-option
            v-for="item in EventShopRewardRebateStatusOptions"
            :key="item.value"
            :label="item.label"
            :value="item.value"
          >
          </el-option>
        </el-select>
        <el-button
          type="primary"
          @click="fetchData(1)"
          :loading="isLoading"
          class="ms-4"
          >{{ $t("action.search") }}</el-button
        >
        <el-button @click="reset" :disabled="isLoading">{{
          $t("action.clear")
        }}</el-button>
      </div>
    </div>
    <div class="card-body">
      <table class="table table-tenant-sm table-hover">
        <thead>
          <tr>
            <th>Id</th>
            <th>{{ $t("fields.status") }}</th>
            <th>{{ $t("fields.rewardId") }}</th>
            <th>{{ $t("fields.ticket") }}</th>
            <th>{{ $t("fields.amount") }}</th>
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
            <td>{{ item.id }}</td>
            <td>
              <el-tag :type="getStatusTag(item.status)">{{
                getStatusLabel(item.status)
              }}</el-tag>
            </td>
            <td>{{ item.eventShopRewardId }}</td>
            <td>{{ item.ticket }}</td>
            <td>
              <BalanceShow
                :currency-id="item.currencyId"
                :balance="item.amount"
              />
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
</template>
<script setup lang="ts">
import { ref, onMounted, watch } from "vue";
import {
  EventShopRewardRebateStatusOptions,
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
  ticket: null,
});
const accountSelected = ref(0);

const fetchData = async (page: number) => {
  isLoading.value = true;
  criteria.value.page = page;
  try {
    const res = await EventsServices.queryShopRewardRebate(criteria.value);
    data.value = res.data;
    criteria.value = res.criteria;
  } catch (error) {
    console.error(error);
  }
  isLoading.value = false;
};

const reset = () => {
  criteria.value = {
    page: 1,
    size: 20,
    partyId: props.partyId,
    status: null,
    ticket: null,
  };
  fetchData(1);
};

const getStatusLabel = (value: number) => {
  const option = EventShopRewardRebateStatusOptions.value.find(
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
