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
        <el-input
          v-model="criteria.accountNumber"
          :placeholder="$t('fields.accountNo')"
          :disabled="isLoading"
          clearable
          class="ms-4"
        />
        <el-select
          v-model="criteria.status"
          :placeholder="$t('fields.status')"
          class="ms-4"
          :disabled="isLoading"
        >
          <el-option
            v-for="item in EventShopPointTransactionTypesOptions"
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
            <td>{{ item.id }}</td>
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
</template>
<script lang="ts" setup>
import { ref, onMounted, watch } from "vue";
import {
  EventShopPointTransactionTypes,
  EventShopPointTransactionTypesOptions,
  EventShopPointTransactionSourceOptions,
} from "@/core/types/shop/ShopPointsTypes";
import EventsServices from "../../../services/EventsServices";

const isLoading = ref(false);
const accountSelected = ref(0);
const props = defineProps<{
  partyId: number;
}>();
const data = ref(<any>[]);
const criteria = ref<any>({
  page: 1,
  size: 20,
  partyId: props.partyId,
  status: null,
});

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

const reset = () => {
  criteria.value = {
    page: 1,
    size: 20,
    partyId: props.partyId,
    status: null,
  };
  fetchData(1);
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

const selectedAccount = (id: number) => {
  accountSelected.value = id;
};

watch(
  () => criteria.value.status,
  (value) => {
    fetchData(1);
  }
);

onMounted(() => {
  fetchData(1);
});
</script>
