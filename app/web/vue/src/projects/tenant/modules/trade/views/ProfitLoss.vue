<template>
  <el-card>
    <template #header>
      <div class="d-flex align-items-center gap-5">
        <el-input
          v-model="criteria.accountNumber"
          :placeholder="$t('fields.accountNo')"
          :disabled="isLoading"
          class="w-150px"
          clearable
        />

        <el-date-picker
          v-model="date"
          :type="dateType"
          :value-format="valueFormat"
          placeholder="Time"
          class="w-150px"
          :shortcuts="shortcuts"
          :disabled="isLoading"
          :clearable="false"
        />

        <el-input
          v-model="criteria.count"
          type="number"
          :placeholder="$t('fields.amount')"
          :disabled="isLoading"
          class="w-150px"
          clearable
        />

        <el-button
          type="success"
          @click="fetchData(1)"
          plain
          :loading="isLoading"
          >{{ $t("action.search") }}</el-button
        >
      </div>
    </template>
    <el-table :data="data" style="width: 100%" v-loading="isLoading">
      <el-table-column
        prop="accountNumber"
        :label="$t('fields.accountNo')"
        width="180"
      />
      <el-table-column label="Net In & Out" width="180">
        <template #default="scope">
          <div style="display: flex; align-items: center">
            <BalanceShow :balance="scope.row.netInOut" />
          </div>
        </template>
      </el-table-column>
      <el-table-column prop="pnl" label="Profit & Loss" width="180">
        <template #default="scope">
          <div style="display: flex; align-items: center">
            <BalanceShow :balance="scope.row.pnl" />
          </div>
        </template>
      </el-table-column>
    </el-table>
  </el-card>
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";
import TradeServices from "../services/TradeServices";
import { Search } from "@element-plus/icons-vue";
import moment from "moment";

const isLoading = ref(false);
const data = ref<any[]>([]);
const criteria = ref<any>({
  count: 100,
});
const date = ref<any>(moment().format("YYYY-MM-DD"));
const dateType = ref("date");
const valueFormat = ref("YYYY-MM-DD");

const fetchData = async (page: number) => {
  isLoading.value = true;
  try {
    await updateDate();
    console.log(criteria.value);
    const response = await TradeServices.queryProfitAndLoss(criteria.value);
    data.value = response;
  } catch (error) {
    console.error("Error fetching data:", error);
  } finally {
    isLoading.value = false;
  }
};

const updateDate = async () => {
  if (!date.value) {
    // Clear any existing date criteria if date is null
    delete criteria.value.from;
    delete criteria.value.to;
    return;
  }

  if (dateType.value === "date") {
    // For a single date, set from to the start of the day and to to the end of the day
    const selectedDate = date.value;
    criteria.value.from = moment(selectedDate)
      .startOf("day")
      .format("YYYY-MM-DD HH:mm:ss");
    criteria.value.to = moment(selectedDate)
      .endOf("day")
      .format("YYYY-MM-DD HH:mm:ss");
    valueFormat.value = "YYYY-MM-DD";
  } else if (dateType.value === "month") {
    // For a month, set from to the first day of the month and to to the last day of the month
    const selectedMonth = date.value;
    criteria.value.from = moment(selectedMonth)
      .startOf("month")
      .format("YYYY-MM-DD HH:mm:ss");
    criteria.value.to = moment(selectedMonth)
      .endOf("month")
      .format("YYYY-MM-DD HH:mm:ss");
    valueFormat.value = "YYYY-MM";
  }
};

const shortcuts = [
  {
    text: "Get by Day",
    onClick() {
      dateType.value = "date";
      valueFormat.value = "YYYY-MM-DD";
    },
  },

  {
    text: "Get by Month",
    onClick() {
      dateType.value = "month";
      valueFormat.value = "YYYY-MM";
    },
  },
];

onMounted(() => {
  fetchData(1);
});
</script>
<style scoped lang="scss">
::v-deep .el-input-group__append {
  background-color: var(--el-fill-color-blank);
}
</style>
