<template>
  <!--begin::Menu 1-->
  <div class="">
    <div class="card">
      <div class="card-header py-4 border-0">
        <div class="card-title gap-8">
          <div class="d-flex flex-column gap-1 align-items-center">
            <label class="fs-6 text fw-semibold me-2">{{
              $t("action.completePayment")
            }}</label>
            <el-switch
              v-model="filterOptions.complete"
              inline-prompt
              :active-icon="Check"
              :inactive-icon="Close"
              @change="emits('stateOptionChanged', filterOptions.complete)"
            />
          </div>

          <div
            class="vertical-line"
            style="
              border-left: 1px dashed gray;
              height: 50px;
              margin-left: 10px;
              margin-right: 20px;
            "
          ></div>

          <div class="d-flex flex-column gap-1">
            <label class="fs-6 text fw-semibold me-2">{{
              $t("fields.currency")
            }}</label>
            <el-select
              v-model="filterOptions.currencyId"
              class="w-150px"
              @change="currencyOptionChanged"
            >
              <el-option key="-1" :label="$t('fields.all')" value="-1" />
              <el-option
                v-for="item in ConfigCurrencySelections"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </el-select>
          </div>

          <div
            class="vertical-line"
            style="
              border-left: 1px dashed gray;
              height: 50px;
              margin-left: 10px;
              margin-right: 20px;
            "
          ></div>

          <div class="d-flex flex-column gap-1">
            <label class="fs-6 text fw-semibold me-2">
              {{ $t("fields.period") }}
            </label>
            <el-date-picker
              class="w-400px"
              v-model="filterOptions.period"
              type="datetimerange"
              :start-placeholder="$t('fields.startDate')"
              :end-placeholder="$t('fields.endDate')"
              :default-time="defaultTime"
            />
          </div>
        </div>
        <div class="card-toolbar">
          <button
            class="btn btn-primary me-4"
            :disabled="props.isLoading"
            @click="filterOptionsApplied"
          >
            <span v-if="props.isLoading">
              {{ $t("action.waiting") }}
              <span
                class="spinner-border h-15px w-15px align-middle text-gray-400"
              ></span>
            </span>
            <span v-else>
              {{ $t("action.apply") }}
            </span>
          </button>
          <button class="btn btn-secondary" @click="initFilterOptions">
            {{ $t("action.reset") }}
          </button>
        </div>
      </div>
    </div>
    <!-- <div class="btn btn-primary">{{ filterOptions }}</div> -->
    <!-- <div class="btn btn-primary">{{ props.isLoading }}</div> -->
  </div>
  <!--end::Menu 1-->
</template>

<script setup lang="ts">
import moment from "moment";
import { ref, onMounted } from "vue";
import { Check, Close } from "@element-plus/icons-vue";
import { ConfigCurrencySelections } from "@/core/types/CurrencyTypes";

const props = defineProps<{
  isLoading: boolean;
  criteria: any;
}>();

const emits = defineEmits<{
  (e: "filterApplied", value: any): void;
  (e: "stateOptionChanged", value: boolean): void;
}>();

const filterCriteria = ref({
  stateId: false,
  currencyId: null,
  from: null,
  to: null,
} as any);

const filterOptions = ref({} as any);
const defaultTime = new Date(2000, 1, 1, 12, 0, 0); // '12:00:00'

const initFilterOptions = () => {
  filterOptions.value = {
    complete: false,
    period: [moment().startOf("day"), moment().endOf("day")],
    currencyId: null,
  };

  filterCriteria.value = {
    stateId: null,
    currencyId: null,
    from: null,
    to: null,
  };

  emits("filterApplied", filterCriteria.value);
};

const currencyOptionChanged = () => {
  filterCriteria.value.currencyId =
    filterOptions.value.currencyId == -1
      ? null
      : filterOptions.value.currencyId;
  emits("filterApplied", filterCriteria.value);
};

const filterOptionsApplied = () => {
  filterOptions.value.complete = false;
  filterCriteria.value.stateId = null;

  filterCriteria.value.from = filterOptions.value.period
    ? filterOptions.value.period[0].toISOString()
    : null;
  filterCriteria.value.to = filterOptions.value.period
    ? filterOptions.value.period[1].toISOString()
    : null;
  emits("filterApplied", filterCriteria.value);
};

onMounted(async () => {
  try {
    filterCriteria.value = JSON.parse(JSON.stringify(props.criteria));
  } catch (error) {
    // console.log(error);
  }

  initFilterOptions();
});
</script>
