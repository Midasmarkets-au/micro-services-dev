<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('action.export')"
    :is-loading="isLoading"
    :width="500"
    disable-footer
  >
    <div
      class="d-flex flex-column justify-content-center align-items-center gap-5"
    >
      <div class="d-flex flex-column">
        <label class="required mb-1">{{ $t("title.recordName") }}</label>
        <el-input class="w-400px" v-model="formData.name" />
      </div>

      <div class="d-flex flex-column">
        <label for="" class="required mb-1">{{ $t("fields.type") }}</label>
        <el-select v-model="formData.type" class="w-400px">
          <el-option
            :label="$t('fields.salesRebate')"
            :value="ReportRequestTypes.SalesRebateForTenant"
          />
          <el-option
            :label="$t('fields.salesRebateSumByAccount')"
            :value="ReportRequestTypes.SalesRebateSumByAccountForTenant"
          />
        </el-select>
      </div>

      <div class="w-100 px-10">
        <el-divider></el-divider>
        <div class="d-flex align-items-center justify-content-between mb-3">
          <h4>Conditions</h4>
          <el-button @click="resetCriteria">Reset Conditions</el-button>
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
          </el-form-item>

          <el-form-item label="Status">
            <el-select
              v-model="criteria.status"
              :placeholder="$t('fields.status')"
              :disabled="exporting"
            >
              <el-option value="0" label="Pending" />
              <el-option value="1" label="Complete" />
            </el-select>
          </el-form-item>
          <el-form-item label="Sales UID">
            <el-input
              v-model="criteria.salesAccountUid"
              placeholder="Sales UID"
              clearable
              :disabled="exporting"
            >
            </el-input>
          </el-form-item>
          <el-form-item label="Account #">
            <el-input
              v-model="criteria.tradeAccountNumber"
              placeholder="Account Number"
              clearable
              :disabled="exporting"
            >
            </el-input>
          </el-form-item>
          <el-form-item label="Ticket">
            <el-input
              v-model="criteria.ticket"
              placeholder="Ticket Number"
              clearable
              :disabled="exporting"
            >
            </el-input>
          </el-form-item>
        </el-form>
      </div>
      <div class="d-flex mt-5">
        <el-button
          type="primary"
          :loading="exporting"
          @click="submit"
          :disabled="!formData.type || !formData.name"
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
import TenantGlobalService from "@/projects/tenant/services/TenantGlobalService";

const modalRef = ref();
const isLoading = ref(false);
const exporting = ref(false);
const period = ref([] as any);

const defaultTime = ref<[Date, Date]>([
  new Date(2000, 1, 1, 0, 0, 0),
  new Date(2000, 2, 1, 23, 59, 59),
]);

const criteria = ref({
  status: "",
  salesAccountUid: "",
  tradeAccountNumber: "",
  ticket: "",
  from: null,
  to: null,
} as any);

const formData = ref({
  type: ReportRequestTypes.SalesRebateSumByAccountForTenant,
} as any);

const submit = async () => {
  exporting.value = true;

  formData.value.query = {
    from: criteria.value.from,
    to: criteria.value.to,
    salesAccountUid: criteria.value.salesAccountUid,
    tradeAccountNumber: criteria.value.tradeAccountNumber,
    ticket: criteria.value.ticket,
    status: criteria.value.status,
  };

  try {
    await TenantGlobalService.createReportRequestDownload(formData.value).then(
      MsgPrompt.success(
        "Request has been submitted, please check the Report Record"
      ).then(() => {
        exporting.value = false;
        resetCriteria();
        formData.value.name = null;
        hide();
      })
    );
  } catch (error) {
    MsgPrompt.error(error);
    exporting.value = false;
  }
};

const resetCriteria = () => {
  period.value = [];
  criteria.value.status = "";
  criteria.value.salesAccountUid = "";
  criteria.value.tradeAccountNumber = "";
  criteria.value.ticket = "";
};

const show = async (_criteria?: any, _period?: any) => {
  criteria.value = _criteria;
  period.value = _period;

  await nextTick();
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
    criteria.value.from = from
      ? moment(from)
          .subtract(1, "days")
          .format(`YYYY-MM-DD[T]${isDST ? 21 : 22}:00:00.000[Z]`)
      : null;
    criteria.value.to = to
      ? moment(to)
          .subtract(1, "days")
          .format(`YYYY-MM-DD[T]${isDST ? 20 : 21}:59:59.000[Z]`)
      : null;
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
