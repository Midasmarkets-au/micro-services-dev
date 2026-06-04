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
              v-for="item in filteredItems"
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
      </template>
    </template>
  </el-dialog>
</template>

<script lang="ts" setup>
import { ref, computed } from "vue";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import TimeShow from "@/components/TimeShow.vue";
import RebateService from "../../services/RebateService";

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

const formatDate = (iso?: string) => {
  if (!iso) return "";
  return iso.slice(0, 10);
};

const show = async (s: any) => {
  summary.value = s;
  items.value = [];
  filter.value = "all";
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
