<template>
  <SimpleForm
    ref="modalRef"
    title="New Deposit Report"
    :is-loading="isLoading"
    :submit="submitRebateReportRequest"
    style="width: 400px"
  >
    <div class="mx-5 mb-5">
      <el-form label-width="70px" class="demo-ruleForm" status-icon>
        <el-form-item label="Name" required>
          <el-input v-model="formData.name" />
        </el-form-item>

        <el-form-item label="Period" required>
          <el-date-picker
            class="w-250px"
            v-model="period"
            type="daterange"
            :start-placeholder="$t('fields.startDate')"
            :end-placeholder="$t('fields.endDate')"
            :default-time="defaultTime"
          />
        </el-form-item>
      </el-form>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import SimpleForm from "@/components/SimpleForm.vue";
import { ref } from "vue";
import { ReportRequestTypes } from "@/core/types/ReportRequestTypes";
import moment from "moment/moment";
import TenantGlobalService, {
  CreateReportSpec,
} from "@/projects/tenant/services/TenantGlobalService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { isDateInDST_US } from "@/core/plugins/TimerService";
const modalRef = ref();
const isLoading = ref(false);
const period = ref([undefined, undefined] as any);

const criteria = ref<any>({});
const emits = defineEmits(["fetchData"]);
const formData = ref<CreateReportSpec>({
  name: "",
  type: ReportRequestTypes.Rebate,
  query: criteria.value,
});

const defaultTime = ref<[Date, Date]>([
  new Date(2000, 1, 1),
  new Date(2000, 2, 1),
]);

const submitRebateReportRequest = async () => {
  isLoading.value = true;
  const [from, to] = period.value;
  const isDST = isDateInDST_US();
  criteria.value.createdFrom = from
    ? moment(from).format(`YYYY-MM-DD[T]${isDST ? 21 : 22}:00:00.000[Z]`)
    : null;
  criteria.value.createdTo = to
    ? moment(to).format(`YYYY-MM-DD[T]${isDST ? 20 : 21}:59:59.000[Z]`)
    : null;

  try {
    await TenantGlobalService.createReportRequest(formData.value);
    MsgPrompt.success("New report request sent successfully").then(() => {
      modalRef.value?.hide();
      emits("fetchData");
    });
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

defineExpose({
  show: () => {
    modalRef.value?.show();
  },
  hide: () => {
    modalRef.value?.hide();
  },
});
</script>

<style scoped></style>
