<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('action.export')"
    :is-loading="isLoading"
    :width="600"
    disable-footer
  >
    <div
      class="d-flex flex-column justify-content-center align-items-center gap-5"
    >
      <div class="d-flex flex-column">
        <label class="required mb-1">{{ $t("title.recordName") }}</label>
        <el-input class="w-400px" v-model="formData.name" />
      </div>

      <div class="row">
        <div class="col-6">
          <div class="d-flex flex-column">
            <label for="" class="required mb-1">{{ $t("fields.type") }}</label>
            <el-select v-model="formData.type">
              <el-option label="Include All" value="all" />
              <el-option label="Direct Client" value="direct" />
              <el-option label="Direct Detail" value="detail" />
            </el-select>
          </div>
        </div>

        <div class="col-6">
          <div class="d-flex flex-column">
            <label for="" class="required mb-1">{{ $t("fields.site") }}</label>
            <el-select v-model="formData.site" :disabled="true">
              <el-option label="VN" value="vn" />
              <!-- <el-option label="BVI" value="bvi" /> -->
              <!-- <el-option label="BA" value="ba" /> -->
            </el-select>
          </div>
        </div>
      </div>

      <div class="w-100 px-10">
        <el-divider></el-divider>
        <div class="d-flex align-items-center justify-content-between mb-3">
          <h4>Conditions</h4>
          <el-button @click="resetFormData">Reset Conditions</el-button>
        </div>

        <el-form label-width="auto" class="border">
          <el-form-item class="mt-3" label="Date">
            <el-date-picker
              v-model="period"
              type="daterange"
              :start-placeholder="$t('fields.startDate')"
              :end-placeholder="$t('fields.endDate')"
              :default-time="defaultTime"
              :disabled="isLoading"
            />
            <div
              style="
                background-color: #ffcccb;
                border-radius: 10px;
                padding: 1px 10px;
                margin-top: 10px;
                width: 100%;
                text-align: center;
              "
            >
              Start Date 21:00 - End Date 21:00
            </div>
          </el-form-item>

          <el-form-item label="Sales Uids">
            <el-input
              v-model="formData.salesAccountNumbers"
              placeholder="Sales Uids"
              type="textarea"
              clearable
              :disabled="exporting"
            >
            </el-input>
          </el-form-item>

          <hr v-if="formData.type != 'detail'" />

          <el-checkbox
            v-if="formData.type != 'detail'"
            v-model="checkAll"
            :indeterminate="isIndeterminate"
            @change="handleCheckAllChange"
          >
            Check all
          </el-checkbox>
          <el-checkbox-group
            v-if="formData.type != 'detail'"
            v-model="formData.checkedColumn"
            @change="handlecheckedColumnChange"
          >
            <div class="row">
              <el-checkbox
                class="col-3"
                v-for="city in columns"
                :key="city"
                :label="city"
                :value="city"
              >
                {{ city }}
              </el-checkbox>
            </div>
          </el-checkbox-group>
        </el-form>
      </div>
      <div class="d-flex mt-5">
        <el-button
          type="primary"
          :loading="exporting"
          @click="submit"
          :disabled="!formData.type || !formData.name || !formData.site"
        >
          {{ $t("action.export") }}
        </el-button>
        <el-button @click="hide">{{ $t("action.cancel") }}</el-button>
      </div>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import moment from "moment";
import { nextTick, ref, watch } from "vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import SimpleForm from "@/components/SimpleForm.vue";
import { isDateInDST_US } from "@/core/plugins/TimerService";
import { ReportRequestTypes } from "@/core/types/ReportRequestTypes";
import ReportService from "@/projects/tenant/modules/report/services/ReportService";

const emits = defineEmits(["fetchData"]);
const modalRef = ref();
const isLoading = ref(false);
const exporting = ref(false);
const period = ref([] as any);
const checkAll = ref(false);
const isIndeterminate = ref(true);
const columns = [
  "Date",
  "Email",
  "Sales Account",
  "Code",
  "Start Equity",
  "Last Equity",
  "Client Deposit",
  "Deposit Transfer",
  "Client Withdrawal",
  "Withdrawal Transfer",
  "IB Withdrawal",
  "IB Rebate",
  "Sales Rebate",
  "Net Deposit",
  "Adjust",
  "Credit",
  "P/L",
  "Net P/L",
  "Lots",
];

const defaultTime = ref<[Date, Date]>([
  new Date(2000, 1, 1, 0, 0, 0),
  new Date(2000, 2, 1, 23, 59, 59),
]);

const formData = ref({
  name: "",
  type: "",
  site: "vn",
  from: null,
  to: null,
  salesAccountNumbers: "",
  checkedColumn: ["Date", "Email", "Sales Account", "Code"],
} as any);

const handleCheckAllChange = (val: any) => {
  formData.value.checkedColumn = val ? columns : [];
  isIndeterminate.value = false;
};
const handlecheckedColumnChange = (value: any[]) => {
  const checkedCount = value.length;
  checkAll.value = checkedCount === columns.length;
  isIndeterminate.value = checkedCount > 0 && checkedCount < columns.length;
};

const submit = async () => {
  exporting.value = true;

  await ReportService.createSalesReport(formData.value).then(
    MsgPrompt.success(
      "Request has been submitted, please check the Report Record"
    ).then(() => {
      emits("fetchData");
      exporting.value = false;
      resetFormData();
      formData.value.name = null;
      formData.value.type = null;
      hide();
    })
  );
};

const resetFormData = () => {
  period.value = [];
  formData.value.from = null;
  formData.value.to = null;
  formData.value.salesAccountNumbers = "";
  formData.value.checkedColumn = ["Date", "Email", "Sales Account", "Code"];
};

const show = async () => {
  formData.value.name = "";
  formData.value.type = "";
  formData.value.site = "vn";
  resetFormData();
  modalRef.value?.show();
};

const hide = () => {
  modalRef.value?.hide();
};

watch(
  () => period.value,
  (periodVal) => {
    const [from, to] = periodVal;
    const isDST = isDateInDST_US();
    formData.value.from = from ? moment(from).format(`YYYY-MM-DD`) : null;
    formData.value.to = to ? moment(to).format(`YYYY-MM-DD`) : null;
  }
);

defineExpose({
  show,
  hide,
});
</script>

<style scoped>
.border {
  border: 1px solid #dedeee !important;
  border-radius: 5px;
  padding: 10px;
}
</style>
