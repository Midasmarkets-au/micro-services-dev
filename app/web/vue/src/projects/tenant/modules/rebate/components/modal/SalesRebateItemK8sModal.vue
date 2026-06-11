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
      <!-- Summary bar (stats are server-side over the full item set) -->
      <div class="d-flex align-items-center gap-4 mb-4 fs-7 fw-semibold">
        <span class="text-muted">Total records: <span class="text-dark">{{ totalCount }}</span></span>
        <el-tag type="success" effect="light">Included: {{ includedCount }}</el-tag>
        <el-tag type="danger" effect="light">Excluded: {{ excludedCount }}</el-tag>
        <span class="text-muted ms-auto">Rebate Amount:
          <span class="text-dark fw-bold">{{ includedAmount.toFixed(4) }}</span>
        </span>
      </div>

      <!-- Filter tabs -->
      <el-radio-group v-model="filter" size="small" class="mb-3" @change="onFilterChange">
        <el-radio-button value="all">All</el-radio-button>
        <el-radio-button value="included">Included</el-radio-button>
        <el-radio-button value="excluded">Excluded</el-radio-button>
      </el-radio-group>

      <NoDataBox v-if="items.length === 0" />
      <template v-else>
        <table class="table align-middle table-row-dashed fs-6 table-hover">
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th>Status</th>
              <th>Ticket</th>
              <th>Account No.</th>
              <th>Currency</th>
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
              v-for="item in items"
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
              <td><CurrencyBadge :currency="item.tradeAccountCurrencyId" /></td>
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
      </template>

      <!-- Server-side pagination -->
      <div v-if="total > 0" class="d-flex justify-content-end mt-3">
        <el-pagination
          :current-page="page"
          :page-size="pageSize"
          :page-sizes="[10, 20, 50, 100]"
          :total="total"
          layout="total, sizes, prev, pager, next"
          small
          background
          @current-change="onPageChange"
          @size-change="onSizeChange"
        />
      </div>
    </template>
  </el-dialog>
</template>

<script lang="ts" setup>
import { ref, computed } from "vue";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import TimeShow from "@/components/TimeShow.vue";
import CurrencyBadge from "@/components/CurrencyBadge.vue";
import RebateService from "../../services/RebateService";
import { convertToLocalTime } from "@/core/plugins/TimerService";

const visible = ref(false);
const isLoading = ref(false);
const summary = ref<any>(null);
const items = ref<any[]>([]);
const filter = ref<"all" | "included" | "excluded">("all");

// Pagination + stats driven by the server.
const page = ref(1);
const pageSize = ref(20);
const total = ref(0); // count of the current filter (drives the paginator)
const includedCount = ref(0);
const excludedCount = ref(0);
const includedAmount = ref(0);
const totalCount = computed(() => includedCount.value + excludedCount.value);

const formatDate = (iso?: string) => {
  if (!iso) return "";
  // period_start/end are stored at MT5-server midnight (UTC+2/+3); convert to
  // server time so the displayed day matches the settlement day.
  return convertToLocalTime(iso, "America/Los_Angeles").slice(0, 10);
};

const fetchItems = async () => {
  if (!summary.value) return;
  try {
    const res = await RebateService.getSalesRebateK8sItems(summary.value.id, {
      page: page.value,
      size: pageSize.value,
      filter: filter.value,
    });
    items.value = Array.isArray(res) ? res : res.data ?? [];
    const c = (Array.isArray(res) ? null : res.criteria) ?? {};
    total.value = Number(c.total ?? items.value.length);
    includedCount.value = Number(c.includedCount ?? 0);
    excludedCount.value = Number(c.excludedCount ?? 0);
    includedAmount.value = parseFloat(c.includedAmount ?? 0);
  } catch (e) {
    console.error(e);
  }
};

const onFilterChange = () => {
  page.value = 1;
  fetchItems();
};

const onPageChange = (p: number) => {
  if (p === page.value) return; // ignore redundant emits (e.g. during size-change)
  page.value = p;
  fetchItems();
};

const onSizeChange = (s: number) => {
  pageSize.value = s;
  page.value = 1;
  fetchItems();
};

const show = async (s: any) => {
  summary.value = s;
  items.value = [];
  filter.value = "all";
  page.value = 1;
  pageSize.value = 20;
  visible.value = true;
  isLoading.value = true;
  await fetchItems();
  isLoading.value = false;
};

defineExpose({ show });
</script>
