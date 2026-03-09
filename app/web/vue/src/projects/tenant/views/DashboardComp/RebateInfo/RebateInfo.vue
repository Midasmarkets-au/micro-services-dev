<template>
  <div class="card">
    <div class="card-header">
      <div
        class="d-flex align-items-center justify-content-between"
        style="min-width: 360px"
      >
        <div class="fs-6 fw-bold">{{ $t("fields.rebateInfo") }}</div>
        <div class="d-flex gap-4">
          <el-date-picker
            v-model="date"
            type="date"
            placeholder="Pick a day"
            style="width: 150px"
            :disabled="isLoading"
            :value-format="'YYYY-MM-DD'"
          />
          <el-button type="warning" @click="fetchData" :loading="isLoading">{{
            $t("action.search")
          }}</el-button>
        </div>
      </div>
    </div>
    <div class="card-body">
      <el-table :data="data" v-loading="isLoading" size="small">
        <el-table-column
          prop="hour"
          :label="$t('fields.hour')"
          align="center"
        />
        <el-table-column
          prop="rebateCount"
          :label="$t('fields.rebateCount')"
          align="center"
        />
        <el-table-column
          prop="tradeCount"
          :label="$t('fields.tradeCount')"
          align="center"
        />
      </el-table>
    </div>
  </div>
</template>
<script lang="ts" setup>
import { ref, onMounted } from "vue";
import SystemService from "@/projects/tenant/modules/system/services/SystemService";
import moment from "moment";

const date = ref(moment().format("YYYY-MM-DD"));
const data = ref<any>([]);
const isLoading = ref(false);
const fetchData = async () => {
  isLoading.value = true;
  try {
    const res = await SystemService.checkTradeSync({ date: date.value });
    data.value = res;
  } catch (e) {
    console.log(e);
  } finally {
    isLoading.value = false;
  }
};

onMounted(() => {
  // fetchData();
});
</script>
