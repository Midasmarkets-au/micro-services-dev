<template>
  <div class="d-flex flex-column flex-column-fluid">
    <div class="card mb-5 mb-xl-8">
      <div class="card-header">
        <div class="card-title d-flex justify-content-between" style="width: 100%">
          <div class="d-flex align-items-center flex-wrap gap-3">
            <el-date-picker
              class="w-250px"
              v-model="period"
              type="daterange"
              start-placeholder="Period From"
              end-placeholder="Period To"
              :disabled="isLoading"
              format="YYYY-MM-DD"
              value-format="YYYY-MM-DD HH:mm:ss"
            />
            <el-input
              v-model="criteria.salesAccountId"
              placeholder="Sales Account ID"
              clearable
              :disabled="isLoading"
              class="w-200px"
            />
            <el-button @click="fetchData(1)" :disabled="isLoading">Search</el-button>
            <el-button @click="reset" :disabled="isLoading">Reset</el-button>
          </div>
        </div>
      </div>

      <div class="card-body">
        <table class="table align-middle table-row-dashed fs-6 table-hover" id="sales_rebate_k8s_table">
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th>Sales Acct UID</th>
              <th>Period</th>
              <th>Type</th>
              <th>Total Amount</th>
              <th>Trades</th>
              <th>Status</th>
              <th>Created On</th>
              <th></th>
            </tr>
          </thead>

          <tbody v-if="isLoading" style="height: 300px">
            <tr>
              <td colspan="8"><scale-loader></scale-loader></td>
            </tr>
          </tbody>
          <tbody v-else-if="data.length === 0">
            <tr>
              <td colspan="8"><NoDataBox /></td>
            </tr>
          </tbody>
          <tbody v-else class="text-gray-600 fw-semibold">
            <tr v-for="item in data" :key="item.id">
              <td>{{ item.salesAccountUid }}</td>
              <td>
                {{ serverDate(item.periodStart) }}
                &nbsp;~&nbsp;
                {{ serverDate(item.periodEnd) }}
              </td>
              <td>
                <el-tag v-if="item.scheduleType === 0" type="success" effect="light">Daily</el-tag>
                <el-tag v-else-if="item.scheduleType === 3" type="primary" effect="light">Monthly</el-tag>
                <el-tag v-else type="info" effect="light">Type {{ item.scheduleType }}</el-tag>
              </td>
              <td>{{ parseFloat(item.totalAmount).toFixed(4) }}</td>
              <td>{{ item.tradeCount }}</td>
              <td>
                <el-tag v-if="item.status === 1" type="success" effect="light">Released</el-tag>
                <el-tag v-else type="warning" effect="light">Pending</el-tag>
              </td>
              <td><TimeShow :date-iso-string="item.createdOn" /></td>
              <td>
                <el-button size="small" @click="showItems(item)">Details ({{ item.tradeCount }})</el-button>
                <el-button
                  v-if="item.status === 0 && item.totalAmount > 0"
                  size="small"
                  type="primary"
                  :loading="item._releasing"
                  @click="release(item)"
                >Release</el-button>
              </td>
            </tr>
          </tbody>
        </table>

        <TableFooter @page-change="pageChange" :criteria="criteria" />
      </div>
    </div>

    <SalesRebateItemK8sModal ref="itemModalRef" />
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted } from "vue";
import TimeShow from "@/components/TimeShow.vue";
import TableFooter from "@/components/TableFooter.vue";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import RebateService from "../services/RebateService";
import SalesRebateItemK8sModal from "../components/modal/SalesRebateItemK8sModal.vue";
import { handleCriteriaTradeTime } from "@/core/helpers/helpers";
import { convertToLocalTime } from "@/core/plugins/TimerService";

// period_start/end are stored at MT5-server midnight (UTC+2/+3); show the server day.
const serverDate = (iso?: string | null) =>
  iso ? convertToLocalTime(iso, "America/Los_Angeles").slice(0, 10) : "";

const isLoading = ref(true);
const data = ref<any[]>([]);
const period = ref<any[]>([]);
const itemModalRef = ref<any>(null);

const criteria = ref<any>({
  page: 1,
  size: 20,
  salesAccountId: null,
});

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  // align date-range filter to MT5-server days (matches stored period_start)
  handleCriteriaTradeTime(period.value, criteria);
  try {
    const res = await RebateService.querySalesRebateK8s(criteria.value);
    criteria.value = { ...criteria.value, ...res.criteria };
    data.value = res.data ?? [];
  } catch (e) {
    console.error(e);
  } finally {
    isLoading.value = false;
  }
};

const reset = () => {
  period.value = [];
  criteria.value = { page: 1, size: 20, salesAccountId: null };
  fetchData(1);
};

const pageChange = (_page: number) => {
  fetchData(_page);
};

const showItems = (item: any) => {
  itemModalRef.value?.show(item);
};

const release = async (item: any) => {
  item._releasing = true;
  try {
    await RebateService.releaseSalesRebateK8s(item.id);
    await fetchData(criteria.value.page);
  } catch (e) {
    console.error(e);
  } finally {
    item._releasing = false;
  }
};

onMounted(() => {
  fetchData(1);
});
</script>
