<template>
  <div class="d-flex flex-column w-100" :class="!isMobile ? 'mt-5' : ''">
    <div
      class="d-flex mb-2 flex-sm-row flex-column w-100 align-items-center justify-content-between card round-bl-br"
    >
      <div class="card-header align-items-center">
        <div class="d-flex flex-sm-row flex-column gap-5 w-sm-auto w-100">
          <el-input
            type="text"
            :placeholder="$t('fields.pleaseEnterItemName')"
            class="w-sm-225px w-100"
            v-model="criteria.eventShopItemName"
            clearable
            @keyup.enter="fetchData(1)"
            :disabled="isLoading"
          />
          <div
            class="d-flex flex-row align-items-center gap-sm-5 justify-content-sm-start justify-content-between gap-2"
          >
            <el-select
              :placeholder="$t('fields.date')"
              v-model="criteria.sortFlag"
              class="w-sm-125px"
              @change="fetchData(1)"
              :disabled="isLoading"
              clearable
            >
              <el-option
                v-for="(item, index) in redemDateSelection"
                :key="index"
                :value="item.value"
                :label="item.label"
              />
            </el-select>
            <el-select
              :placeholder="$t('fields.status')"
              v-model="criteria.status"
              class="w-sm-125px"
              @change="fetchData(1)"
              clearable
              :disabled="isLoading"
            >
              <el-option
                v-for="(item, index) in EventShopOrderStatusOptions"
                :key="index"
                :value="item.value"
                :label="item.label"
              />
            </el-select>
          </div>
          <div>
            <el-button @click="fetchData(1)" size="large" :disabled="isLoading">
              {{ $t("action.search") }}
            </el-button>

            <el-button @click="reset" size="large" :disabled="isLoading">
              {{ $t("action.reset") }}
            </el-button>
          </div>
        </div>
      </div>
    </div>
    <!-- table section -->
    <div class="table-responsive card round-tl-tr">
      <table class="table">
        <thead class="card-table">
          <tr class="rounded">
            <th scope="col" class="ps-5">
              {{ $t("fields.itemName") }}
            </th>
            <th scope="col">
              {{ $t("fields.orderNumber") }}
            </th>
            <th scope="col">
              {{ $t("fields.points") }}
            </th>
            <th scope="col">
              {{ $t("fields.quantity") }}
            </th>
            <th scope="col">
              {{ $t("fields.date") }}
            </th>
            <th scope="col">
              {{ $t("fields.status") }}
            </th>
            <th scope="col">
              {{ $t("action.action") }}
            </th>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && data.length === 0">
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr v-for="(item, index) in data" :key="index" class="border-bottom">
            <td class="ps-5">{{ item.eventShopItemName }}</td>
            <td>{{ item.hashId }}</td>
            <td><ShopPoints :points="item.totalPoint" /></td>
            <td>{{ item.quantity }}</td>
            <td>
              <TimeShow :date-iso-string="item.createdOn" type="customCSS" />
            </td>
            <td>{{ getStatusLabel(item.status) }}</td>
            <td>
              <button
                class="btn btn-sm btn-link m-0 p-0 lh-0"
                @click="showDetail(item)"
              >
                {{ $t("action.view") }}
              </button>
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
  </div>
  <OrderDetail ref="orderDetailRef" @fetch-data="fetchData" />
</template>
<script setup lang="ts">
import { isMobile } from "@/core/config/WindowConfig";
import { computed, onMounted, ref } from "vue";
import ShopService from "../../services/ShopService";
import { EventShopOrderStatusOptions } from "@/core/types/shop/ShopPointsTypes";
import OrderDetail from "../modal/OrderDetail.vue";
import i18n from "@/core/plugins/i18n";
import MsgPrompt from "@/core/plugins/MsgPrompt";
const { t } = i18n.global;
const criteria = ref<any>({
  page: 1,
  size: 10,
  sortField: "createdOn",
  eventKey: "EventShop",
});
const isLoading = ref(false);
const orderDetailRef = ref<InstanceType<typeof OrderDetail>>();
const redemDateSelection = computed(() => [
  {
    label: t("fields.oldestFirst"),
    value: false,
  },
  {
    label: t("fields.newestFirst"),
    value: true,
  },
]);

const data = ref<any>([]);
const fetchData = async (_page: number, type?: string, value?: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  if (type && value) criteria.value[type] = value;
  try {
    const res = await ShopService.getOrderList(criteria.value);
    data.value = res.data;
    criteria.value = res.criteria;
    isLoading.value = false;
  } catch (error) {
    console.log(error);
    MsgPrompt.error(error);
  }
};

const getStatusLabel = (value: number) => {
  const option = EventShopOrderStatusOptions.value.find(
    (option) => option.value === value
  );
  return option ? option.label : "";
};

const showDetail = (item: any) => {
  orderDetailRef.value.show(item);
};
const reset = () => {
  criteria.value = {
    page: 1,
    size: 10,
    sortField: "createdOn",
    eventKey: "EventShop",
  };
  fetchData(1);
};
onMounted(() => {
  fetchData(1);
});
</script>
<style lang="scss">
th,
td {
  word-break: keep-all;
  text-wrap: nowrap;
}
</style>
