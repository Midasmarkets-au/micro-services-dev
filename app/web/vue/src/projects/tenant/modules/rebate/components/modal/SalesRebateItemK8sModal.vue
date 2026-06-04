<template>
  <el-dialog
    v-model="visible"
    :title="`Sales Rebate Details — ${formatDate(summary?.periodStart)} ~ ${formatDate(summary?.periodEnd)}`"
    width="90%"
    destroy-on-close
  >
    <div v-if="isLoading" class="text-center py-5">
      <scale-loader></scale-loader>
    </div>
    <template v-else>
      <NoDataBox v-if="items.length === 0" />
      <template v-else>
        <!-- Summary bar -->
        <div class="d-flex align-items-center gap-4 mb-4 fs-7 fw-semibold">
          <span class="text-muted">Total records: <span class="text-dark">{{ items.length }}</span></span>
          <el-tag type="success" effect="light">Included: {{ includedItems.length }}</el-tag>
          <el-tag type="danger" effect="light">Excluded: {{ excludedItems.length }}</el-tag>
          <span class="text-muted ms-auto">Rebate Amount:
            <span class="text-dark fw-bold">{{ includedTotal.toFixed(4) }}</span>
          </span>
        </div>

        <!-- Filter tabs -->
        <el-radio-group v-model="filter" size="small" class="mb-3">
          <el-radio-button value="all">All</el-radio-button>
          <el-radio-button value="included">Included</el-radio-button>
          <el-radio-button value="excluded">Excluded</el-radio-button>
        </el-radio-group>

        <table class="table align-middle table-row-dashed fs-6 table-hover">
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th>Status</th>
              <th>Ticket</th>
              <th>Account No.</th>
              <th>Symbol</th>
              <th>Volume (lots)</th>
              <th>Rebate Type</th>
              <th>Rebate Base</th>
              <th>Amount</th>
              <th>Closed On</th>
            </tr>
          </thead>
          <tbody class="text-gray-600 fw-semibold">
            <tr
              v-for="item in pagedItems"
              :key="item.id"
              :class="{ 'bg-light-danger': item.excluded }"
              :style="item.excluded ? 'opacity: 0.7' : ''"
            >
              <td>
                <el-tag v-if="item.excluded" type="danger" effect="light" size="small">Excluded</el-tag>
                <el-tag v-else type="success" effect="light" size="small">Included</el-tag>
              </td>
              <td :style="item.excluded ? 'text-decoration: line-through; color: #999' : ''">{{ item.ticket }}</td>
              <td>{{ item.tradeAccountNumber }}</td>
              <td>{{ item.symbol }}</td>
              <td>{{ (item.volume / 100).toFixed(2) }}</td>
              <td>{{ item.rebateType }}</td>
              <td>{{ parseFloat(item.rebateBase).toFixed(4) }}</td>
              <td>
                <span v-if="item.excluded" class="text-muted">—</span>
                <span v-else>{{ parseFloat(item.amount).toFixed(4) }}</span>
              </td>
              <td><TimeShow :date-iso-string="item.closedOn" /></td>
            </tr>
          </tbody>
        </table>

        <!-- Pagination -->
        <div class="d-flex justify-content-end mt-3">
          <el-pagination
            v-model:current-page="page"
            v-model:page-size="pageSize"
            :page-sizes="[10, 20, 50, 100]"
            :total="filteredItems.length"
            layout="total, sizes, prev, pager, next"
            small
            background
          />
        </div>
      </template>
    </template>
  </el-dialog>
</template>

<script lang="ts" setup>
import { ref, computed, watch } from "vue";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import TimeShow from "@/components/TimeShow.vue";
import RebateService from "../../services/RebateService";
import { convertToLocalTime } from "@/core/plugins/TimerService";

const visible = ref(false);
const isLoading = ref(false);
const summary = ref<any>(null);
const items = ref<any[]>([]);
const filter = ref<"all" | "included" | "excluded">("all");

const includedItems = computed(() => items.value.filter(i => !i.excluded));
const excludedItems = computed(() => items.value.filter(i => i.excluded));
const includedTotal = computed(() => includedItems.value.reduce((s, i) => s + parseFloat(i.amount), 0));

const filteredItems = computed(() => {
  if (filter.value === "included") return includedItems.value;
  if (filter.value === "excluded") return excludedItems.value;
  return items.value;
});

// Client-side pagination over the (filtered) items.
const page = ref(1);
const pageSize = ref(20);
const pagedItems = computed(() => {
  const start = (page.value - 1) * pageSize.value;
  return filteredItems.value.slice(start, start + pageSize.value);
});
// Reset to first page whenever the filter or page size changes.
watch([filter, pageSize], () => {
  page.value = 1;
});

const formatDate = (iso?: string) => {
  if (!iso) return "";
  // period_start/end are stored at MT5-server midnight (UTC+2/+3); convert to
  // server time so the displayed day matches the settlement day.
  return convertToLocalTime(iso, "America/Los_Angeles").slice(0, 10);
};

const show = async (s: any) => {
  summary.value = s;
  items.value = [];
  filter.value = "all";
  page.value = 1;
  visible.value = true;
  isLoading.value = true;
  try {
    const res = await RebateService.getSalesRebateK8sItems(s.id);
    items.value = Array.isArray(res) ? res : res.data ?? [];
  } catch (e) {
    console.error(e);
  } finally {
    isLoading.value = false;
  }
};

defineExpose({ show });
</script>
