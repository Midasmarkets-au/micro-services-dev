<template>
  <div>
    <div class="card">
      <div class="card-header">
        <div class="card-title">
          <el-switch
            v-model="criteria.completed"
            size="large"
            active-text="Completed"
            inactive-text="Not Completed"
            @change="fetchData(1)"
          />
          <el-checkbox
            v-model="selectAll"
            :indeterminate="isIndeterminate"
            @change="handleSelectAll"
            v-if="criteria.completed && !isLoading && items.length > 0"
            class="ms-3"
          >
            {{ $t("action.checkAll") }}
          </el-checkbox>
          <el-button
            type="primary"
            plain
            @click="handleBatchResend"
            v-if="criteria.completed && !isLoading && items.length > 0"
            class="ms-3"
          >
            {{ $t("action.batchResendRebate") }}
          </el-button>
        </div>
        <div class="card-toolbar">
          <div class="d-flex gap-2 me-4">
            <el-input
              class="w-125px"
              v-model="criteria.accountNumber"
              placeholder="Account No."
              clearable
            />
            <el-input
              class="w-100px"
              v-model="criteria.ticket"
              placeholder="Ticket"
              clearable
            />
            <el-input
              class="w-100px"
              v-model="criteria.symbol"
              placeholder="Symbol"
              clearable
            />

            <el-date-picker
              class="w-300px me-3"
              v-model="period"
              type="daterange"
              :start-placeholder="$t('fields.startDate')"
              :end-placeholder="$t('fields.endDate')"
              format="YYYY-MM-DD"
              value-format="YYYY-MM-DD HH:mm:ss"
            />
          </div>

          <div>
            <el-button type="primary" @click="fetchData(1)">{{
              $t("action.search")
            }}</el-button>
            <el-button @click="clearSearchCriteria">{{
              $t("action.clear")
            }}</el-button>
          </div>
        </div>
      </div>

      <div class="card-body">
        <table
          class="table align-middle table-row-dashed fs-6 gy-2 table-hover"
          id="table_accounts_requests"
        >
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th style="width: 50px">
                <el-checkbox
                  v-model="selectAll"
                  :indeterminate="isIndeterminate"
                  @change="handleSelectAll"
                  v-if="criteria.completed && !isLoading && items.length > 0"
                />
              </th>
              <th class="">id</th>
              <th class="">{{ $t("fields.accountNumber") }}</th>
              <th class="">Ticket</th>
              <th class="">Symbol</th>
              <th class="">Volume</th>
              <th class="">{{ $t("fields.currency") }}</th>
              <th class="">{{ $t("fields.status") }}</th>
              <th class="">{{ $t("fields.closeTime") }}</th>
              <th class="">{{ $t("fields.action") }}</th>
            </tr>
          </thead>

          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && items.length === 0">
            <NoDataBox />
          </tbody>

          <tbody v-else class="fw-semibold">
            <tr
              v-for="(item, index) in items"
              :key="index"
              :class="{
                'account-select': item.id === accountSelected,
              }"
              @click="selectedAccount(item.id)"
            >
              <td @click.stop>
                <el-checkbox
                  v-if="criteria.completed"
                  :model-value="selectedItemIds.includes(item.id)"
                  @change="handleItemSelect(item.id, $event)"
                />
              </td>
              <td>{{ item.id }}</td>
              <td>
                <el-button
                  @click.stop="showAccountDetail(item.accountId)"
                  :icon="User"
                  circle
                  class="me-1"
                />{{ item.accountNumber }}
              </td>
              <td>{{ item.ticket }}</td>
              <td>{{ item.symbol }}</td>
              <td>{{ $filters.digits(item.volume / 100) }}</td>
              <td>{{ $t(`type.currency.${item.currencyId}`) }}</td>
              <td>{{ $t(`type.tradeRebateStatus.${item.status}`) }}</td>
              <td>
                <TimeShow type="exactTime" :date-iso-string="item.closedOn" />
              </td>
              <td>
                <el-button
                  @click.stop="tradeRebateDetailModalRef?.show(item)"
                  >{{ $t("action.viewDetails") }}</el-button
                >
              </td>
            </tr>
          </tbody>
        </table>

        <TableFooter @page-change="fetchData" :criteria="criteria" />
      </div>
    </div>
    <TradeRebateDetailModal ref="tradeRebateDetailModalRef" />
    <AccountDetail ref="accountDetailRef" />

    <!-- 批量重新计算返佣对话框 -->
    <el-dialog
      v-model="batchDialogVisible"
      :title="$t('action.batchResendRebate')"
      :close-on-click-modal="!isExecuting"
      :close-on-press-escape="!isExecuting"
      :before-close="handleDialogBeforeClose"
      width="500px"
    >
      <div>
        <el-progress
          :percentage="progressPercentage"
          :status="progressStatus"
          :text-inside="true"
          :stroke-width="20"
        />
        <div class="mt-3 text-center" v-if="!confirmDisabled">
          <el-button
            type="primary"
            @click="handleConfirmExecute"
            :disabled="isExecuting || confirmDisabled"
            :loading="isExecuting"
          >
            {{ $t("action.confirmExecute") }}
          </el-button>
        </div>
      </div>
      <template #footer>
        <div class="dialog-footer text-right w-100">
          <el-button
            @click="handleCloseBatchDialog"
            :disabled="isExecuting"
            v-if="progressPercentage === 100"
          >
            {{ $t("status.completed") }}
          </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from "vue";
import { useI18n } from "vue-i18n";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import RebateService from "@/projects/tenant/modules/rebate/services/RebateService";
import TimeShow from "@/components/TimeShow.vue";
import TradeRebateDetailModal from "@/projects/tenant/modules/rebate/components/modal/TradeRebateDetailModal.vue";
import {
  getTradeRebateStatusSelections,
  TradeRebateStatusTypes,
} from "@/core/types/TradeRebateStatusTypes";
import AccountDetail from "../../accounts/components/AccountDetail.vue";
import { User } from "@element-plus/icons-vue";
import { convertToUTC } from "@/core/utils/DateUtils";

const { t } = useI18n();
const isLoading = ref(false);
const items = ref(Array<any>());
const accountSelected = ref(0);
const accountDetailRef = ref<InstanceType<typeof AccountDetail>>();
const tradeRebateDetailModalRef =
  ref<InstanceType<typeof TradeRebateDetailModal>>();
const criteria = ref<any>({
  page: 1,
  size: 20,
  completed: true,
  // statuses: [TradeRebateStatusTypes.Completed],
} as any);

const period = ref([] as any);
const tradeRebateSelections = getTradeRebateStatusSelections();

// 批量操作相关状态
const selectedItemIds = ref<number[]>([]);
const selectAll = ref(false);
const batchDialogVisible = ref(false);
const isExecuting = ref(false);
const progressPercentage = ref(0);
const progressStatus = ref<"success" | "exception" | "warning" | "">("");
const pendingItemIds = ref<number[]>([]);
const confirmDisabled = ref(false);

// 全选状态计算
const isIndeterminate = computed(() => {
  if (items.value.length === 0) {
    return false;
  }
  const selectedCount = selectedItemIds.value.length;
  const totalCount = items.value.length;
  return selectedCount > 0 && selectedCount < totalCount;
});

// 全选处理
const handleSelectAll = (val: any) => {
  const checked = Boolean(val);
  if (checked) {
    selectedItemIds.value = items.value.map((item: any) => item.id);
  } else {
    selectedItemIds.value = [];
  }
};

// 单行选择处理
const handleItemSelect = (itemId: number, val: any) => {
  const checked = Boolean(val);
  if (checked) {
    if (!selectedItemIds.value.includes(itemId)) {
      selectedItemIds.value.push(itemId);
    }
  } else {
    selectedItemIds.value = selectedItemIds.value.filter((id) => id !== itemId);
  }
  // 更新全选状态
  const totalCount = items.value.length;
  selectAll.value = selectedItemIds.value.length === totalCount;
};

// 批量重新计算返佣
const handleBatchResend = () => {
  if (selectedItemIds.value.length === 0) {
    MsgPrompt.error(t("error.pleaseSelectAtLeastOne"));
    return;
  }
  batchDialogVisible.value = true;
  pendingItemIds.value = [...selectedItemIds.value];
  progressPercentage.value = 0;
  progressStatus.value = "";
  isExecuting.value = false;
  confirmDisabled.value = false;
};

// 确认执行批量操作
const handleConfirmExecute = async () => {
  if (pendingItemIds.value.length === 0) {
    return;
  }

  isExecuting.value = true;
  progressPercentage.value = 0;
  progressStatus.value = "";

  const totalCount = pendingItemIds.value.length;
  const batchSize = 5;
  const progressIncrement = (batchSize / totalCount) * 100;

  try {
    // 分批处理，每次并发5个
    for (let i = 0; i < pendingItemIds.value.length; i += batchSize) {
      const batch = pendingItemIds.value.slice(i, i + batchSize);

      // 并发执行当前批次
      await Promise.all(
        batch.map((id: number) => RebateService.resendRebate(id))
      );

      // 更新进度条（保留两位小数）
      progressPercentage.value = parseFloat(
        Math.min(progressPercentage.value + progressIncrement, 100).toFixed(2)
      );
    }

    // 完成
    progressPercentage.value = 100;
    progressStatus.value = "success";

    // 刷新数据
    await fetchData(criteria.value.page);

    // 清空选择
    selectedItemIds.value = [];
    selectAll.value = false;

    // 允许用户手动关闭对话框并避免重复触发
    isExecuting.value = false;
    confirmDisabled.value = true;
  } catch (error: any) {
    progressStatus.value = "exception";
    MsgPrompt.error(error?.message || "批量重新计算返佣失败");
    isExecuting.value = false;
    confirmDisabled.value = false;
  }
};

// 对话框关闭前处理
const handleDialogBeforeClose = (done: () => void) => {
  if (isExecuting.value) {
    return; // 执行中不允许关闭
  }
  done();
};

// 关闭批量对话框
const handleCloseBatchDialog = () => {
  if (isExecuting.value) {
    return; // 执行中不允许关闭
  }
  batchDialogVisible.value = false;
  pendingItemIds.value = [];
  progressPercentage.value = 0;
  progressStatus.value = "";
  confirmDisabled.value = false;
};
const fetchData = async (_page: number) => {
  criteria.value.page = _page;
  const datesRange = convertToUTC(period.value);
  criteria.value.closedFrom = datesRange.from;
  criteria.value.closedTo = datesRange.to;
  try {
    isLoading.value = true;
    const res = await RebateService.queryTradeRebate(criteria.value);
    items.value = res.data;
    criteria.value = res.criteria;
    // 重置选择状态
    selectedItemIds.value = [];
    selectAll.value = false;
  } catch (e) {
    MsgPrompt.error(e);
  } finally {
    isLoading.value = false;
  }
};
const selectedAccount = (id: number) => {
  accountSelected.value = id;
};
const clearSearchCriteria = async () => {
  criteria.value = {
    page: 1,
    size: 20,
    statuses: [TradeRebateStatusTypes.Completed],
  };
  await fetchData(1);
};

const showAccountDetail = (id: number) => {
  accountDetailRef.value?.show(id, "infos", [] as any);
};

onMounted(async () => {
  await fetchData(1);
});
</script>

<style scoped></style>
