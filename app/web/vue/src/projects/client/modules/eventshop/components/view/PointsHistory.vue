<template>
  <div class="d-flex flex-column w-100" :class="!isMobile ? 'mt-5' : ''">
    <div
      class="d-flex mb-2 flex-sm-row flex-column w-100 align-items-center justify-content-between card round-bl-br"
    >
      <div class="card-header align-items-center">
        <div class="d-flex flex-sm-row flex-column gap-5">
          <div
            class="d-flex flex-row align-items-center gap-sm-5 justify-content-sm-start justify-content-between gap-2 flex-wrap"
          >
            <!-- <el-input
              :placeholder="$t('fields.ticket')"
              v-model="criteria.ticket"
              class="w-150px w-sm-125px"
              clearable
              @keyup.enter="fetchData(1)"
              :disabled="isLoading"
            /> -->
            <el-select
              :placeholder="$t('fields.status')"
              v-model="criteria.status"
              class="w-150px w-sm-125px"
              @change="fetchData(1)"
              :disabled="isLoading"
              clearable
            >
              <el-option
                v-for="(item, index) in EventShopPointTransactionTypesOptions"
                :key="index"
                :value="item.value"
                :label="item.label"
              />
            </el-select>
            <el-select
              :placeholder="$t('fields.type')"
              v-model="criteria.sourceType"
              class="w-150px w-sm-125px"
              @change="fetchData(1)"
              :disabled="isLoading"
              clearable
            >
              <el-option
                v-for="(item, index) in EventShopPointTransactionSourceOptions"
                :key="index"
                :value="item.value"
                :label="item.label"
              />
            </el-select>
            <el-select
              :placeholder="$t('fields.date')"
              v-model="criteria.sortFlag"
              class="w-150px w-sm-125px"
              @change="fetchData(1)"
              :disabled="isLoading"
            >
              <el-option
                v-for="(item, index) in getDateSelection"
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
              {{ $t("fields.type") }}
            </th>
            <th scope="col">
              {{ $t("fields.status") }}
            </th>
            <th scope="col">
              {{ $t("fields.date") }}
            </th>
            <th scope="col" class="pe-5">
              {{ $t("fields.points") }}
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
            <td class="ps-5">
              {{ getSourceTypeLabel(item.sourceType) }}
              <span
                v-if="
                  item.sourceType == EventShopPointTransactionSourceTypes.Trade
                "
                >({{ item?.source?.ticket }})</span
              >
            </td>
            <td>
              {{ getStatusLabel(item.status) }}
            </td>
            <td>
              <TimeShow :date-iso-string="item.createdOn" type="customCSS" />
            </td>
            <td>
              <ShopPoints
                :points="item.point"
                :format="Math.abs(item.point) > 10000 ? 2 : 4"
              />
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
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import { isMobile } from "@/core/config/WindowConfig";
import {
  EventShopPointTransactionSourceOptions,
  EventShopPointTransactionTypesOptions,
  EventShopPointTransactionSourceTypes,
} from "@/core/types/shop/ShopPointsTypes";
import MsgPrompt from "@/core/plugins/MsgPrompt";
const { t } = i18n.global;
const criteria = ref<any>({
  page: 1,
  size: 10,
  sortFeild: "createdOn",
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
    const res = await ShopService.getPointsHistory(criteria.value);
    data.value = res.data;
    criteria.value = res.criteria;
    isLoading.value = false;
  } catch (error) {
    console.log(error);
    MsgPrompt.error(error);
  }
};
const getSourceTypeLabel = (value: number) => {
  const option = EventShopPointTransactionSourceOptions.value.find(
    (option) => option.value === value
  );
  return option ? option.label : "";
};
const getStatusLabel = (value: number) => {
  const option = EventShopPointTransactionTypesOptions.value.find(
    (option) => option.value === value
  );
  return option ? option.label : "";
};
const reset = () => {
  criteria.value = {
    page: 1,
    size: 10,
    sortFeild: "createdOn",
    sortFlag: true,
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
