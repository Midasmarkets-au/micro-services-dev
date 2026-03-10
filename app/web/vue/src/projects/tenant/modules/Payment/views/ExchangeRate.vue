<template>
  <div
    class="mb-2 ms-2 d-flex justify-content-between align-items-center"
    style="color: #ff8800; font-size: 18px"
  >
    <div>
      <i
        class="fa-solid fa-triangle-exclamation fa-lg me-1"
        style="color: #ff8800"
      ></i>
      {{ $t("tip.rateUpdate") }}
    </div>
    <el-button type="primary" @click="openBatchUploadDialog">
      {{ $t("action.batchUploadRate") }}
    </el-button>
  </div>
  <div class="input-group mb-3">
    <label class="input-group-text" for="inputGroupSelect01">{{
      $t("action.createNewRate")
    }}</label>
    <input
      type="number"
      class="form-control"
      name="depositRate"
      :placeholder="t('fields.depositRate')"
      v-model="newData.deposit"
    />
    <input
      type="number"
      class="form-control"
      name="depositRate"
      :placeholder="t('fields.withdrawalRate')"
      v-model="newData.withdraw"
    />
    <input
      type="number"
      class="form-control"
      name="adjujstRate"
      :placeholder="t('fields.markup')"
      v-model="newData.adjust"
    />
    <select class="form-select" v-model="newData.symbol">
      <option value="">{{ $t("action.selectSymbols") }}</option>
      <option
        v-for="(item, index) in rateSymbols"
        :key="index"
        :value="item.symbol"
      >
        {{ item.symbol }}
      </option>
    </select>
    <button class="btn btn-secondary" type="button" @click="create">
      <span v-if="isSubmitting">
        {{ $t("action.waiting") }}
        <span
          class="spinner-border h-15px w-15px align-middle text-gray-400"
        ></span>
      </span>
      <span v-else>{{ $t("action.create") }}</span>
    </button>
  </div>
  <div class="mb-5 fs-4 text-end" style="color: #d9534f">
    如要新增 <span class="fw-bold">貨幣</span> 請通知 IT Department
  </div>
  <div class="row">
    <div class="col-6 mb-4" v-for="(item, index) in rateSymbols" :key="index">
      <div class="card">
        <div class="card-header p-4">
          <div class="d-flex align-items-end">
            <span class="fs-1 fw-bold" style="color: #5cb85c">
              {{ item.symbol }}
            </span>
            <span class="fs-4 ms-3 pb-1 fw-bold" style="color: #6c757d">{{
              $t("title.exchangeRate")
            }}</span>
          </div>
        </div>
        <div class="card-body">
          <table
            class="table align-middle table-row-dashed fs-6 gy-5 dataTable no-footer"
          >
            <thead>
              <th class="fw-bold">Deposit 卖出价</th>
              <th class="fw-bold">Withdraw 买入价</th>
              <th class="fw-bold">Adjust 调整 %</th>
              <th class="fw-bold text-center">{{ $t("fields.updatedOn") }}</th>
              <th class="fw-bold text-center">{{ $t("fields.operator") }}</th>
            </thead>
            <tbody v-if="item.isLoading">
              <LoadingRing />
            </tbody>
            <tbody v-else-if="!item.isLoading && item.data.length === 0">
              <NoDataBox />
            </tbody>
            <tbody v-else-if="item.error">
              {{
                item.error
              }}
            </tbody>

            <template v-else>
              <tbody>
                <tr v-for="(rate, index) in item.data" :key="index">
                  <td>{{ rate.markupDeposit }} ({{ rate.deposit }})</td>
                  <td>{{ rate.markupWithdraw }} ({{ rate.withdraw }})</td>
                  <td>{{ rate.adjust }}</td>
                  <td class="text-center">
                    <div style="font-size: 12px; color: rgba(113, 113, 113, 1)">
                      {{ moment(rate.updatedOn).format("M-D H:m:s") }}
                    </div>
                  </td>
                  <td class="text-center">{{ rate.staff }}</td>
                </tr>
              </tbody>
            </template>
          </table>
        </div>
      </div>
    </div>
  </div>

  <!-- Batch Upload Dialog -->
  <el-dialog
    v-model="batchDialogVisible"
    :title="$t('action.batchUploadRate')"
    width="800px"
    :close-on-click-modal="!isUploading"
    :close-on-press-escape="!isUploading"
    :before-close="handleDialogBeforeClose"
  >
    <el-tabs v-model="activeTab" type="card" @tab-change="handleTabChange">
      <el-tab-pane :label="$t('action.uploadCsv')" name="csv">
        <div class="mb-3">
          <el-alert
            :title="$t('tip.csvFormatHint')"
            type="info"
            :closable="false"
            show-icon
          />
        </div>
        <el-upload
          ref="uploadRef"
          :auto-upload="false"
          :limit="1"
          accept=".csv"
          :on-change="handleFileChange"
          :file-list="fileList"
          drag
        >
          <el-icon class="el-icon--upload"
            ><i class="fa fa-upload"></i
          ></el-icon>
          <div class="el-upload__text">
            {{ $t("action.clickToUpload") }} <em>CSV</em>
          </div>
        </el-upload>
      </el-tab-pane>

      <el-tab-pane :label="$t('action.inputText')" name="text">
        <div class="mb-3">
          <el-alert
            :title="$t('tip.csvFormatHint')"
            type="info"
            :closable="false"
            show-icon
          />
        </div>
        <el-input
          v-model="inputText"
          type="textarea"
          :rows="10"
          :placeholder="'Example:\n1.23 1.20 2.5 USDVND\n1.50 1.45 3.0 USDTHB'"
        />
      </el-tab-pane>
    </el-tabs>

    <!-- Validation Error Display -->
    <div v-if="validationErrors.length > 0" class="mt-3">
      <el-alert type="error" :closable="false">
        <template #title>
          <div>{{ $t("error.uploadFailed") }}</div>
        </template>
        <div v-for="(error, index) in validationErrors" :key="index">
          {{ error }}
        </div>
      </el-alert>
    </div>

    <!-- Progress Bar -->
    <div
      v-if="showProgress && (isUploading || progressPercentage > 0)"
      class="mt-3"
    >
      <el-progress
        :percentage="progressPercentage"
        :status="progressStatus"
        :text-inside="true"
        :stroke-width="20"
      />
      <div v-if="uploadError" class="text-danger mt-2">{{ uploadError }}</div>
    </div>

    <template #footer>
      <div class="dialog-footer text-right w-100">
        <el-button
          v-if="progressPercentage < 100"
          type="primary"
          @click="handleConfirmUpload"
          :disabled="isUploading"
          :loading="isUploading"
        >
          {{
            isUploading ? $t("action.uploading") : $t("action.confirmExecute")
          }}
        </el-button>
        <el-button @click="handleCloseDialog" :disabled="isUploading">
          {{
            progressPercentage === 100
              ? $t("status.completed")
              : $t("action.close")
          }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import moment from "moment";

import { ref, onMounted } from "vue";
import { CurrencyTypes } from "@/core/types/CurrencyTypes";
import LoadingRing from "@/components/LoadingRing.vue";
import PaymentService from "../services/PaymentService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import NoDataBox from "@/components/NoDataBox.vue";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

// Batch upload dialog state
const batchDialogVisible = ref(false);
const activeTab = ref("csv");
const inputText = ref("");
const fileList = ref<any[]>([]);
const uploadRef = ref();
const isUploading = ref(false);
const progressPercentage = ref(0);
const progressStatus = ref<"success" | "exception" | "warning" | "">("") as any;
const validationErrors = ref<string[]>([]);
const uploadError = ref("");
const parsedData = ref<any[]>([]);
const showProgress = ref(false);
const isSubmitting = ref(false);
const newData = ref({} as any);
const rateSymbols = ref({} as any);
const criteria = ref({
  size: 5,
});

onMounted(async () => {
  const res = await PaymentService.getExchangeRate({
    page: 1,
    size: 100,
    numPages: 1,
  });
  res.data.forEach((item) => {
    if (rateSymbols.value[item.name] == undefined)
      rateSymbols.value[item.name] = {};
    rateSymbols.value[item.name].id = item.id;
    rateSymbols.value[item.name].symbol = item.name;
    rateSymbols.value[item.name].from = item.fromCurrencyId;
    rateSymbols.value[item.name].to = item.toCurrencyId;
    rateSymbols.value[item.name].isLoading = true;
    rateSymbols.value[item.name].error = "";
    rateSymbols.value[item.name].data = [];
  });
  for (const key of Object.keys(rateSymbols.value)) {
    getRate(rateSymbols.value[key]);
  }
});

const getRate = async (item: any) => {
  item.isLoading = true;
  item.data = [];

  try {
    var data = [] as any;

    data = await PaymentService.getExchangeRateHistory(item.id, criteria.value);
    const dataLength = ref(
      criteria.value.size < data.length ? criteria.value.size : data.length
    );
    console.log(data[0]);
    if (data.length == 0) {
      item.isLoading = false;
    } else {
      for (let i = 0; i < dataLength.value; i++) {
        item.data.push({
          deposit: data[i]?.changes.currentValues.SellingRate ?? "--",
          markupDeposit:
            Math.ceil(
              data[i]?.changes.currentValues.SellingRate *
                (1 + data[i]?.changes.currentValues.AdjustRate / 100) *
                1000
            ) / 1000,
          withdraw: data[i]?.changes.currentValues.BuyingRate ?? "--",
          markupWithdraw:
            Math.floor(
              data[i]?.changes.currentValues.BuyingRate *
                (1 - data[i]?.changes.currentValues.AdjustRate / 100) *
                1000
            ) / 1000,
          adjust: data[i]?.changes.currentValues.AdjustRate,

          updatedOn: data[i]?.createdOn,
          staff: data[i]?.operatorName,
        });
      }
    }
  } catch (error) {
    item.error = error;
  }
  item.isLoading = false;
};

const create = async () => {
  if (
    Object.keys(newData.value).length == 0 ||
    newData.value.symbol == "" ||
    newData.value.symbol == undefined
  ) {
    return;
  }

  const sellingRate = ref(newData.value.deposit);
  const buyingRate = ref(newData.value.withdraw);

  if (sellingRate.value == undefined || sellingRate.value == 0) {
    sellingRate.value =
      rateSymbols.value[newData.value.symbol].data[0]?.deposit ?? 0;
  }
  if (buyingRate.value == undefined || buyingRate.value == 0) {
    buyingRate.value =
      rateSymbols.value[newData.value.symbol].data[0]?.withdraw ?? 0;
  }

  isSubmitting.value = true;

  try {
    await PaymentService.putExchangeRate(
      rateSymbols.value[newData.value.symbol].id,
      {
        id: rateSymbols.value[newData.value.symbol].id,
        name: rateSymbols.value[newData.value.symbol].symbol,
        sellingRate: sellingRate.value,
        buyingRate: buyingRate.value,
        adjustRate: newData.value.adjust == "" ? 0 : newData.value.adjust,
      }
    );

    isSubmitting.value = false;

    MsgPrompt.success("Exchange Rate Update Success").then(() => {
      getRate(rateSymbols.value[newData.value.symbol]);
    });
  } catch (error) {
    console.log(error);
  }
};

// Batch upload functions
const openBatchUploadDialog = () => {
  batchDialogVisible.value = true;
  resetBatchDialog();
};

const resetBatchDialog = () => {
  activeTab.value = "csv";
  inputText.value = "";
  fileList.value = [];
  validationErrors.value = [];
  uploadError.value = "";
  progressPercentage.value = 0;
  progressStatus.value = "";
  parsedData.value = [];
  isUploading.value = false;
  showProgress.value = false;
};

const handleTabChange = () => {
  // Hide progress bar when switching tabs
  showProgress.value = false;
  progressPercentage.value = 0;
  progressStatus.value = "";
  uploadError.value = "";
  validationErrors.value = [];
};

const handleFileChange = (file: any) => {
  fileList.value = [file];
  const reader = new FileReader();
  reader.onload = (e) => {
    const text = e.target?.result as string;
    inputText.value = text;
  };
  reader.readAsText(file.raw);
};

const validateAndParseData = (): boolean => {
  validationErrors.value = [];
  parsedData.value = [];

  const text = inputText.value.trim();
  if (!text) {
    validationErrors.value.push(t("error.INPUT_REQUIRE"));
    return false;
  }

  const lines = text.split("\n").filter((line) => line.trim());

  // Determine delimiter: if CSV tab is active or first line contains comma, use comma; otherwise use space
  const isCSVMode = activeTab.value === "csv" || lines[0].includes(",");
  const delimiter = isCSVMode ? "," : /\s+/;

  // Check if first line is header (contains non-numeric values in first 3 fields)
  let startIndex = 0;
  if (lines.length > 0) {
    const firstLineParts = lines[0]
      .trim()
      .split(delimiter)
      .map((p) => p.trim())
      .filter((p) => p);

    if (firstLineParts.length === 4) {
      // Check if first three fields are non-numeric (likely header)
      const firstThreeNumeric = firstLineParts
        .slice(0, 3)
        .every((field) => !isNaN(parseFloat(field)));

      if (!firstThreeNumeric) {
        // First line is likely a header, skip it
        startIndex = 1;
      }
    }
  }

  for (let i = startIndex; i < lines.length; i++) {
    const lineNumber = i + 1;
    const parts = lines[i]
      .trim()
      .split(delimiter)
      .map((p) => p.trim())
      .filter((p) => p);

    if (parts.length !== 4) {
      validationErrors.value.push(
        t("error.lineError", {
          line: lineNumber,
          msg: `Expected 4 fields, got ${parts.length}`,
        })
      );
      continue;
    }

    const [sellingRate, buyingRate, adjustRate, name] = parts;

    // Validate numbers
    const sellingRateNum = parseFloat(sellingRate);
    const buyingRateNum = parseFloat(buyingRate);
    const adjustRateNum = parseFloat(adjustRate);

    if (isNaN(sellingRateNum)) {
      validationErrors.value.push(
        t("error.lineError", {
          line: lineNumber,
          msg: `SellingRate - ${t("error.invalidNumber")}`,
        })
      );
      continue;
    }

    if (isNaN(buyingRateNum)) {
      validationErrors.value.push(
        t("error.lineError", {
          line: lineNumber,
          msg: `BuyingRate - ${t("error.invalidNumber")}`,
        })
      );
      continue;
    }

    if (isNaN(adjustRateNum)) {
      validationErrors.value.push(
        t("error.lineError", {
          line: lineNumber,
          msg: `AdjustRate - ${t("error.invalidNumber")}`,
        })
      );
      continue;
    }

    // Validate name exists in rateSymbols
    if (!rateSymbols.value[name]) {
      validationErrors.value.push(
        t("error.lineError", {
          line: lineNumber,
          msg: `${name} - ${t("error.invalidSymbol")}`,
        })
      );
      continue;
    }

    parsedData.value.push({
      sellingRate: sellingRateNum,
      buyingRate: buyingRateNum,
      adjustRate: adjustRateNum,
      name: name,
      id: rateSymbols.value[name].id,
      symbol: rateSymbols.value[name].symbol,
    });
  }

  return validationErrors.value.length === 0 && parsedData.value.length > 0;
};

const handleConfirmUpload = async () => {
  // Validate data first
  if (!validateAndParseData()) {
    return;
  }

  isUploading.value = true;
  showProgress.value = true;
  progressPercentage.value = 0;
  progressStatus.value = "";
  uploadError.value = "";

  const totalCount = parsedData.value.length;
  const batchSize = 5;
  const progressIncrement = parseFloat(
    ((batchSize / totalCount) * 100).toFixed(2)
  );

  try {
    for (let i = 0; i < parsedData.value.length; i += batchSize) {
      const batch = parsedData.value.slice(i, i + batchSize);
      const promises = batch.map((item) =>
        PaymentService.putExchangeRate(item.id, {
          id: item.id,
          name: item.symbol,
          sellingRate: item.sellingRate,
          buyingRate: item.buyingRate,
          adjustRate: item.adjustRate,
        })
      );

      await Promise.all(promises);

      // Update progress
      progressPercentage.value = parseFloat(
        Math.min(
          progressPercentage.value + progressIncrement * batch.length,
          100
        ).toFixed(2)
      );
    }

    progressPercentage.value = 100;
    progressStatus.value = "success";

    // Refresh all rates
    for (const key of Object.keys(rateSymbols.value)) {
      await getRate(rateSymbols.value[key]);
    }
  } catch (error: any) {
    progressStatus.value = "exception";
    uploadError.value = error?.message || t("error.uploadFailed");
  } finally {
    isUploading.value = false;
  }
};

const handleDialogBeforeClose = (done: () => void) => {
  if (isUploading.value) {
    return;
  }
  done();
};

const handleCloseDialog = () => {
  if (isUploading.value) {
    return;
  }
  batchDialogVisible.value = false;
  resetBatchDialog();
};
</script>
