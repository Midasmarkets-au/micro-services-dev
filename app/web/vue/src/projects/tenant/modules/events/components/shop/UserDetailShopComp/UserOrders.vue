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
            v-for="item in EventShopOrderStatusOptions"
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
            <th>{{ $t("fields.itemName") }}</th>
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
              <TimeShow :date-iso-string="item.createdOn" type="GMToneLiner" />
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
  <OrderDetail ref="orderDetailRef" />
  <AddTrackingModal ref="addTrackingModalRef" @fetch-data="fetchData" />
</template>
<script setup lang="ts">
import { ref, onMounted, watch } from "vue";
import {
  EventShopOrderStatusTypes,
  EventShopOrderStatusOptions,
} from "@/core/types/shop/ShopPointsTypes";
import EventsServices from "../../../services/EventsServices";
import OrderDetail from "../OrderDetail.vue";
import AddTrackingModal from "../AddTrackingModal.vue";
import i18n from "@/core/plugins/i18n";

const t = i18n.global.t;
const isLoading = ref(false);
const orderDetailRef = ref<any>(null);
const addTrackingModalRef = ref<any>(null);
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
    const res = await EventsServices.queryShopOrderList(criteria.value);
    data.value = res.data;
    criteria.value = res.criteria;
  } catch (error) {
    console.error(error);
  }
  isLoading.value = false;
};

const showDetail = (item: any) => {
  orderDetailRef.value?.show(item);
};

function getStatusTag(status: EventShopOrderStatusTypes) {
  switch (status) {
    case EventShopOrderStatusTypes.Pending:
      return "warning";
    case EventShopOrderStatusTypes.Processing:
      return "info";
    case EventShopOrderStatusTypes.Shipped:
      return "primary";
    case EventShopOrderStatusTypes.Succeed:
      return "success";
    case EventShopOrderStatusTypes.Cancelled:
      return "danger";
  }
}

const getStatusLabel = (value: number) => {
  const option = EventShopOrderStatusOptions.value.find(
    (option) => option.value === value
  );
  return option ? option.label : "";
};

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
