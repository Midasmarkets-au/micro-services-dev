<template>
  <div class="d-flex flex-column w-100" :class="!isMobile ? 'mt-5' : ''">
    <div
      class="d-flex mb-2 w-100 flex-sm-row flex-column align-items-center justify-content-between card round-bl-br"
    >
      <div class="card-header align-items-center">
        <div
          class="d-flex flex-row align-items-sm-center align-items-between gap-5"
        >
          <el-select
            :placeholder="$t('fields.status')"
            v-model="criteria.status"
            class="w-150px w-sm-125px"
            @change="fetchData(1)"
            :disabled="isLoading"
            clearable
          >
            <el-option
              v-for="(item, index) in EventShopRewardRebateStatusOptions"
              :key="index"
              :value="item.value"
              :label="item.label"
            />
          </el-select>
          <el-select
            :placeholder="$t('fields.closeTime')"
            v-model="criteria.sortFlag"
            class="w-150px w-sm-125px"
            :disabled="isLoading"
            clearable
            @change="fetchData(1)"
          >
            <el-option
              v-for="(item, index) in getDateSelection"
              :key="index"
              :value="item.value"
              :label="item.label"
            />
          </el-select>
        </div>
      </div>
    </div>
    <!-- table section -->
    <div class="table-responsive card round-tl-tr">
      <table class="table">
        <thead class="card-table">
          <tr class="rounded">
            <th scope="col" class="ps-5">
              {{ $t("fields.ticket") }}
            </th>
            <th scope="col">
              {{ $t("fields.symbol") }}
            </th>
            <th scope="col">
              {{ $t("fields.lot") }}
            </th>
            <th scope="col">
              {{ $t("fields.openTime") }}
            </th>
            <th scope="col">
              {{ $t("fields.closeTime") }}
            </th>
            <th scope="col">
              {{ $t("fields.status") }}
            </th>
            <th scope="col" class="pe-5">
              {{ $t("fields.amount") }}
            </th>
            <th scope="col">
              {{ $t("fields.createdOn") }}
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
            <td class="ps-5">{{ item.ticket }}</td>
            <td>{{ item.symbol }}</td>
            <td>{{ item.volume }}</td>

            <td>
              <TimeShow :date-iso-string="item.openAt" type="customCSS" />
            </td>
            <td>
              <TimeShow
                :date-iso-string="item?.closeAt"
                type="customCSS"
                v-if="item.closeAt"
              />
            </td>
            <td>{{ getRebateLabel(item.status) }}</td>
            <td><BalanceShow :balance="item.amount" /></td>
            <td>
              <TimeShow :date-iso-string="item.createdOn" type="customCSS" />
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
  </div>
</template>
<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import ShopService from "../../services/ShopService";
import i18n from "@/core/plugins/i18n";
import { EventShopRewardRebateStatusOptions } from "@/core/types/shop/ShopPointsTypes";
import { isMobile } from "@/core/config/WindowConfig";
const { t } = i18n.global;
import Decimal from "decimal.js";
import MsgPrompt from "@/core/plugins/MsgPrompt";
const criteria = ref<any>({
  page: 1,
  size: 10,
  sortField: "createdOn",
  sortFlag: true,
  eventKey: "EventShop",
});
const isLoading = ref(true);
const getDateSelection = computed(() => [
  {
    label: t("fields.oldestFirst"),
    value: false,
  },
  {
    label: t("fields.newestFirst"),
    value: true,
  },
]);

const data = ref<any[]>([]);
const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;

  try {
    const res = await ShopService.queryRewardRebateList(criteria.value);
    data.value = res.data;
    data.value = data.value.map((item) => {
      return {
        ...item,
        volume: new Decimal(item.amount).div(100).toNumber(),
      };
    });
    criteria.value = res.criteria;
  } catch (error) {
    console.log(error);
    MsgPrompt.error(error);
  }
  isLoading.value = false;
};
const getRebateLabel = (value: number) => {
  const option = EventShopRewardRebateStatusOptions.value.find(
    (option) => option.value === value
  );
  return option ? option.label : "";
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
