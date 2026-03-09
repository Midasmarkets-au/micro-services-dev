<template>
  <div>
    <div class="card">
      <div class="card-header">
        <div class="card-title">
          {{ $t("title.ibRebateReport") }}
          <el-checkbox
            v-model="selectAll"
            :indeterminate="isIndeterminate"
            @change="handleSelectAll"
            v-if="!isLoading && reports.length > 0"
            class="ms-3"
          >
            {{ $t("action.checkAll") }}
          </el-checkbox>
          <el-button
            type="primary"
            plain
            @click="handleBatchRegenerate"
            v-if="!isLoading && reports.length > 0"
            class="ms-3"
          >
            {{ $t("action.batchRegenerateReport") }}
          </el-button>
          <el-button
            type="success"
            @click="showDailyEquityReportModal"
            class="ms-3"
          >
            {{ $t("action.generateDailyEquityReport") }}
          </el-button>
        </div>
        <div class="card-toolbar">
          <div class="d-flex gap-2">
            <el-input
              class="w-200px"
              v-model="criteria.keywords"
              :placeholder="$t('fields.name')"
              clearable
            />
            <el-button type="primary" @click="fetchData(1)">{{
              $t("action.search")
            }}</el-button>
          </div>
        </div>
      </div>

      <div class="card-body">
        <table
          class="table align-middle table-row-dashed fs-6 gy-5"
          id="table_accounts_requests"
        >
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th style="width: 50px">
                <el-checkbox
                  v-model="selectAll"
                  :indeterminate="isIndeterminate"
                  @change="handleSelectAll"
                  v-if="!isLoading && reports.length > 0"
                />
              </th>
              <th class="">id</th>
              <th class="">{{ $t("fields.name") }}</th>
              <th class="">{{ $t("fields.type") }}</th>
              <th class="">{{ $t("fields.createdOn") }}</th>
              <th class="">{{ $t("fields.generatedOn") }}</th>
              <th class="">{{ $t("fields.startTime") }}</th>
              <th class="">{{ $t("fields.endTime") }}</th>
              <th class="">{{ $t("fields.hasData") }}</th>
              <th class="">{{ $t("fields.valid") }}</th>
              <th class="">{{ $t("fields.status") }}</th>
              <th class="">{{ $t("fields.action") }}</th>
            </tr>
          </thead>

          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && reports.length === 0">
            <NoDataBox />
          </tbody>

          <tbody v-else class="fw-semibold">
            <tr v-for="(item, index) in reports" :key="index">
              <td @click.stop>
                <el-checkbox
                  :model-value="selectedReportIds.includes(item.id)"
                  @change="handleItemSelect(item.id, $event)"
                />
              </td>
              <td>{{ item.id }}</td>
              <td>{{ item.name }}</td>
              <td>{{ $t(`type.reportRequest.${item.type}`) }}</td>
              <td><TimeShow :date-iso-string="item.createdOn" /></td>
              <td><TimeShow :date-iso-string="item.generatedOn" /></td>
              <td><TimeShow :date-iso-string="item.query.from" /></td>
              <td><TimeShow :date-iso-string="item.query.to" /></td>
              <td>
                <template v-if="!item.isEmpty"
                  ><i
                    class="fa-solid fa-check fa-xl"
                    style="color: #4ed06e"
                  ></i>
                </template>
                <template v-else>
                  <i class="fa-solid fa-xmark fa-xl" style="color: #d92626"></i>
                </template>
              </td>
              <td>
                <template v-if="!item.isExpired"
                  ><i
                    class="fa-solid fa-check fa-xl"
                    style="color: #4ed06e"
                  ></i>
                </template>
                <template v-else>
                  <i class="fa-solid fa-xmark fa-xl" style="color: #d92626"></i>
                </template>
              </td>
              <td style="color: rgb(124, 143, 162)">
                <template v-if="item.isGenerated"
                  >{{ $t("status.readyForDownload") }}
                </template>
                <template v-else> {{ $t("status.processing") }} ... </template>
              </td>
              <td>
                <div class="d-flex gap-1">
                  <a
                    v-if="item.isGenerated"
                    href="#"
                    @click="downloadReportFile(item)"
                    class="me-2"
                    ><i
                      class="fa-solid fa-download fa-xl"
                      style="color: #5b6b86"
                    ></i
                  ></a>
                  <el-button
                    type="primary"
                    size="small"
                    @click="handleSingleRegenerate(item.id)"
                  >
                    {{ $t("action.regenerateReport") }}
                  </el-button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>

        <TableFooter @page-change="fetchData" :criteria="criteria" />
      </div>
    </div>

    <!-- 批量重新生成报告对话框 -->
    <el-dialog
      v-model="batchDialogVisible"
      :title="$t('action.batchRegenerateReport')"
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

    <!-- Daily Equity Report Modal -->
    <NewReportRequestModal
      ref="dailyEquityReportModalRef"
      :custom-title="$t('action.generateDailyEquityReport')"
      :report-type="ReportRequestTypes.DailyEquityReport"
      @fetch-data="fetchData(1)"
    />
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from "vue";
import { useI18n } from "vue-i18n";
import ReportService from "@/projects/tenant/modules/report/services/ReportService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import TimeShow from "@/components/TimeShow.vue";
import { getImageUrl } from "@/core/plugins/ProcessImageLink";
import TenantGlobalService from "@/projects/tenant/services/TenantGlobalService";
import moment from "moment";
import NewReportRequestModal from "@/projects/tenant/modules/rebate/components/modal/NewReportRequestModal.vue";
import { ReportRequestTypes } from "@/core/types/ReportRequestTypes";

const { t } = useI18n();
const reports = ref<Array<any>>([]);
const isLoading = ref(false);
const dailyEquityReportModalRef = ref();

// 打开 Daily Equity Report 模态框
const showDailyEquityReportModal = () => {
  dailyEquityReportModalRef.value?.show();
};
const criteria = ref({
  page: 1,
  size: 10,
  keywords: "",
});

// 批量操作相关状态
const selectedReportIds = ref<number[]>([]);
const selectAll = ref(false);
const batchDialogVisible = ref(false);
const isExecuting = ref(false);
const progressPercentage = ref(0);
const progressStatus = ref<"success" | "exception" | "warning" | "">("");
const pendingReportIds = ref<number[]>([]);
const confirmDisabled = ref(false);

// 全选状态计算
const isIndeterminate = computed(() => {
  if (reports.value.length === 0) {
    return false;
  }
  const selectedCount = selectedReportIds.value.length;
  const totalCount = reports.value.length;
  return selectedCount > 0 && selectedCount < totalCount;
});

// 全选处理
const handleSelectAll = (val: any) => {
  const checked = Boolean(val);
  if (checked) {
    selectedReportIds.value = reports.value.map((item: any) => item.id);
  } else {
    selectedReportIds.value = [];
  }
};

// 单行选择处理
const handleItemSelect = (itemId: number, val: any) => {
  const checked = Boolean(val);
  if (checked) {
    if (!selectedReportIds.value.includes(itemId)) {
      selectedReportIds.value.push(itemId);
    }
  } else {
    selectedReportIds.value = selectedReportIds.value.filter(
      (id) => id !== itemId
    );
  }
  // 更新全选状态
  const totalCount = reports.value.length;
  selectAll.value = selectedReportIds.value.length === totalCount;
};

// 单个重新生成报告
const handleSingleRegenerate = async (id: number) => {
  try {
    await ReportService.regenerateReport(id);
    MsgPrompt.success(t("action.regenerateReport") + " " + t("status.success"));
    await fetchData(criteria.value.page);
  } catch (error: any) {
    MsgPrompt.error(
      error?.message || t("action.regenerateReport") + " " + t("status.failed")
    );
  }
};

// 批量重新生成报告
const handleBatchRegenerate = () => {
  if (selectedReportIds.value.length === 0) {
    MsgPrompt.error(t("error.pleaseSelectAtLeastOne"));
    return;
  }
  batchDialogVisible.value = true;
  pendingReportIds.value = [...selectedReportIds.value];
  progressPercentage.value = 0;
  progressStatus.value = "";
  isExecuting.value = false;
  confirmDisabled.value = false;
};

// 确认执行批量操作
const handleConfirmExecute = async () => {
  if (pendingReportIds.value.length === 0) {
    return;
  }

  isExecuting.value = true;
  progressPercentage.value = 0;
  progressStatus.value = "";

  const totalCount = pendingReportIds.value.length;
  const batchSize = 5;
  const progressIncrement = (batchSize / totalCount) * 100;

  try {
    // 分批处理，每次并发5个
    for (let i = 0; i < pendingReportIds.value.length; i += batchSize) {
      const batch = pendingReportIds.value.slice(i, i + batchSize);

      // 并发执行当前批次
      await Promise.all(
        batch.map((id: number) => ReportService.regenerateReport(id))
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
    selectedReportIds.value = [];
    selectAll.value = false;

    // 允许用户手动关闭对话框并避免重复触发
    isExecuting.value = false;
    confirmDisabled.value = true;
  } catch (error: any) {
    progressStatus.value = "exception";
    MsgPrompt.error(
      error?.message ||
        t("action.batchRegenerateReport") + " " + t("status.failed")
    );
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
  pendingReportIds.value = [];
  progressPercentage.value = 0;
  progressStatus.value = "";
  confirmDisabled.value = false;
};

const fetchData = async (_page: number) => {
  criteria.value.page = _page;
  isLoading.value = true;
  try {
    const res = await ReportService.queryRequests(criteria.value);
    reports.value = res.data.map((item) => ({
      ...item,
      query: item.query ? JSON.parse(item.query) : null,
    }));
    criteria.value = res.criteria;
    // 重置选择状态
    selectedReportIds.value = [];
    selectAll.value = false;
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

const downloadReportFile = async (_item) => {
  const downloadUrl = getImageUrl(_item.fileName);
  try {
    const timestamp = moment().format("HH:mm:ss");
    let name =
      _item.id + "_" + _item.name.split(" ").join("_") + `${timestamp}`;
    await TenantGlobalService.downloadFileByLink(downloadUrl, name);
  } catch (error) {
    MsgPrompt.error(error);
  }
};

onMounted(async () => {
  await fetchData(1);
});
</script>

<style scoped></style>
