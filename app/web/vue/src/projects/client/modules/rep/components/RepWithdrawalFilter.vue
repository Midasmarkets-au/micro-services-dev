<template>
  <div class="">
    <div v-if="!isMobile" v-show="showFilter" class="d-flex py-5">
      <div class="d-flex gap-5">
        <div
          class="d-flex align-items-center"
          v-if="filterOptions?.includes('size')"
        >
          <label class="filter-label me-2" style="">
            {{ $t("fields.pageSize") }}
          </label>
          <el-select
            v-model="criteria.size"
            class="w-75px"
            @change="fetchData(1)"
          >
            <el-option
              v-for="item in pageSizes"
              :key="item"
              :label="item"
              :value="item"
            />
          </el-select>
        </div>

        <div
          class="d-flex align-items-center"
          v-if="filterOptions?.includes('period')"
        >
          <label class="filter-label me-2" style="">
            {{ $t("fields.period") }}
          </label>
          <el-date-picker
            class="w-250px"
            v-model="period"
            type="daterange"
            :start-placeholder="$t('fields.startDate')"
            :end-placeholder="$t('fields.endDate')"
            :default-time="defaultTime"
          />
        </div>

        <div
          class="d-flex align-items-center"
          v-if="filterOptions?.includes('accountNumber')"
        >
          <label class="filter-label me-2" style="">
            {{ $t("fields.accountNo") }}
          </label>
          <el-input
            v-model="criteria.accountNumber"
            class="w-200px"
            clearable
            @keyup.enter="filterData(1)"
          >
            <template #append>
              <el-button :icon="Search as any" @click="filterData(1)" />
            </template>
          </el-input>
        </div>
      </div>
      <div
        v-if="trigger === 'button'"
        class="card-toolbar justify-content-center ms-11"
      >
        <!-- <button
          :class="`btn btn-${color} me-4`"
          :disabled="isLoading"
          @click="fetchData(1)"
        >
          <span v-if="isLoading">
            {{ $t("action.waiting") }}
            <span
              class="spinner-border h-15px w-15px align-middle text-gray-400"
            ></span>
          </span>
          <span v-else>
            {{ $t("action.apply") }}
          </span>
        </button>
        <button class="btn btn-secondary" @click="initCriteria()">
          {{ $t("action.reset") }}
        </button> -->
        <el-button :disabled="isLoading" size="large" @click="fetchData(1)">
          <span v-if="isLoading">
            {{ $t("action.waiting") }}
            <span
              class="spinner-border h-15px w-15px align-middle text-gray-400"
            ></span>
          </span>
          <span v-else>
            {{ $t("action.apply") }}
          </span>
        </el-button>
        <el-button @click="initCriteria" size="large">{{
          $t("action.reset")
        }}</el-button>
      </div>
    </div>

    <div v-if="isMobile" class="row">
      <div
        class="col-12 d-flex align-items-center mb-5"
        v-if="filterOptions?.includes('period')"
      >
        <label class="filter-label me-2" style="">
          {{ $t("fields.date") }}
        </label>
        <el-date-picker
          class="w-250px"
          v-model="period"
          type="daterange"
          :start-placeholder="$t('fields.startDate')"
          :end-placeholder="$t('fields.endDate')"
          :default-time="defaultTime"
        />
      </div>

      <div
        class="col-12 d-flex align-items-center mb-5"
        v-if="filterOptions?.includes('accountNumber')"
      >
        <label class="filter-label me-2" style="">
          {{ $t("fields.accountNo") }}
        </label>
        <el-input
          v-model="criteria.accountNumber"
          class="w-150px"
          @keyup.enter="filterData(1)"
        >
          <template #append>
            <el-button :icon="Search" @click="filterData(1)" />
          </template>
        </el-input>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, watch } from "vue";

import { useI18n } from "vue-i18n";
import MsgPrompt from "@/core/plugins/MsgPrompt";

import moment from "moment";
import { isMobile } from "@/core/config/WindowConfig";
import { Search } from "@element-plus/icons-vue";

const props = withDefaults(
  defineProps<{
    fromPage?: string;
    showFilter?: boolean;
    defaultCriteria?: any;
    serviceHandler: (criteria: any) => Promise<any>;
    color?: string;
    filterOptions?: Array<string>;
    trigger?: string;
  }>(),
  {
    showFilter: true,
    defaultCriteria: () => ({ page: 1, size: 10 }),
    color: "primary",
    filterOptions: () => [
      "closed",
      "size",
      "period",
      "symbol",
      "order",
      "transactionType",
      "transactionStatus",
    ],
    trigger: "button",
  }
);

const { t } = useI18n();

const showFilter = ref(props.showFilter);
const filtered = ref(false); // flag to show whether the filter is applied
const isLoading = ref(true);
const pageSizes = [10, 15, 20, 25, 30, 50, 100];
const defaultTime = ref<[Date, Date]>([
  new Date(2000, 1, 1, 0, 0, 0),
  new Date(2000, 2, 1, 23, 59, 59),
]);

const criteria = ref<any>({});

// since the status key in criteria is an array, using it in v-model will cause display error(display the number)
// set a new variable and watch it to update the criteria.status

const period = ref([] as any);
const initCriteria = (myCriteria?: any) => {
  filtered.value = false;
  if (myCriteria) {
    criteria.value = myCriteria;
  } else {
    criteria.value = {
      ...props.defaultCriteria,
    };
  }

  if (criteria.value.from && criteria.value.to) {
    period.value = [criteria.value.openedFrom, criteria.value.openedTo];
  } else {
    period.value[0] = null;
    period.value[1] = null;
  }
  fetchData(1);
};

const data = ref(Array<any>());

const fetchData = async (selectedPage: number) => {
  isLoading.value = true;
  criteria.value.page = selectedPage;
  // console.log(criteria.value);

  try {
    const res = await props.serviceHandler(criteria.value);
    data.value = res.data;
    criteria.value = res.criteria;
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

const filterData = (_page: number) => {
  filtered.value = true;
  fetchData(_page);
};

onMounted(async () => {
  isLoading.value = true;
  try {
    initCriteria();
    await fetchData(1);
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
});

const parsePeriodIntoCriteria = () => {
  let val = period.value;
  if (val && val.length > 0 && typeof val[0] !== "string") {
    val = [
      moment(val[0]).local().toISOString(),
      moment(val[1]).local().toISOString(),
    ];
  }
  criteria.value.from = val ? val[0] : null;
  criteria.value.to = val ? val[1] : null;
};

watch(
  () => criteria.value.symbol,
  (val) => (criteria.value.symbol = val ?? "")
);

watch(
  () => period.value,
  async () => {
    /**
     * Yixuan notes:
     *
     * backend time is based on UTC Times while the period value here is local time
     * the period we get from the date picker is local time,
     * we need to convert the local time to UTC time to filter the data
     */
    parsePeriodIntoCriteria();
    if (props.trigger === "change") await filterData(1);
  }
);

defineExpose({
  show: () => (showFilter.value = !showFilter.value),
  getData: () => data.value,
  data,
  fetchData,
  initCriteria,
  criteria,
  isLoading,
  filterData,
  filtered,
});
</script>

<style scoped lang="scss">
.filter-label {
  font-size: 14px;
  color: #b1b1b1;
}
</style>
