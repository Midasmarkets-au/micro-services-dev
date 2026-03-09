<template>
  <SimpleForm
    ref="modalRef"
    :title="customTitle || $t('action.createNewRebateReport')"
    :is-loading="isLoading"
    :submit="submitRebateReportRequest"
    style="width: 600px"
  >
    <div class="mx-5 mb-5">
      <el-form label-width="70px" class="demo-ruleForm" status-icon>
        <el-form-item :label="$t('fields.name')" required>
          <el-input v-model="formData.name" :disabled="isLoading" />
        </el-form-item>

        <el-form-item :label="$t('fields.date')" required>
          <el-date-picker
            class="w-400px"
            v-model="period"
            type="datetimerange"
            :start-placeholder="$t('fields.startDate')"
            :end-placeholder="$t('fields.endDate')"
            format="YYYY-MM-DD HH:mm:ss"
            value-format="YYYY-MM-DD HH:mm:ss"
            :disabled="isLoading"
          />
        </el-form-item>
        <el-form-item :label="$t('fields.status')" required>
          <el-select v-model="stateId" :disabled="isLoading">
            <el-option :label="$t('fields.all')" :value="null"></el-option>
            <el-option
              v-for="item in getTradeRebateStatusSelections"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item :label="$t('fields.timeBase')" required>
          <el-select v-model="useClosingTime" :disabled="isLoading">
            <el-option
              :label="$t('fields.releasedTimeBased')"
              :value="false"
            ></el-option>
            <el-option
              :label="$t('fields.mt5ClosingTimeBased')"
              :value="true"
            ></el-option>
          </el-select>
        </el-form-item>
      </el-form>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import SimpleForm from "@/components/SimpleForm.vue";
import { ref } from "vue";
import { ReportRequestTypes } from "@/core/types/ReportRequestTypes";
import TenantGlobalService from "@/projects/tenant/services/TenantGlobalService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { getTradeRebateStatusSelections } from "@/core/types/RebateStatus";

const props = withDefaults(
  defineProps<{
    customTitle?: string;
    reportType?: number;
  }>(),
  {
    customTitle: "",
    reportType: ReportRequestTypes.Rebate,
  }
);

const modalRef = ref();
const isLoading = ref(false);
const period = ref([] as any);
const stateId = ref(null);
const useClosingTime = ref(true);
const criteria = ref<any>({});
const emits = defineEmits(["fetchData"]);
const formData = ref({
  name: "",
  type: props.reportType,
  query: criteria.value,
});

const convertToISO = (prd) => {
  const val = prd.map((x) => (x = x.replace(" ", "T") + "Z"));
  return { from: val ? val[0] : null, to: val ? val[1] : null };
};

const submitRebateReportRequest = async () => {
  isLoading.value = true;
  criteria.value = convertToISO(period.value);
  criteria.value.stateId = stateId.value;
  criteria.value.useClosingTime = useClosingTime.value;
  formData.value.query = criteria.value;
  // 根据 useClosingTime 自动添加后缀
  const suffix = useClosingTime.value
    ? "(MT5 ClosingTime Based)"
    : "(ReleasedTime Based)";
  formData.value.name = `${formData.value.name} ${suffix}`;
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
    // 清空表单数据
    formData.value.name = "";
    period.value = [];
    stateId.value = null;
    useClosingTime.value = true;
    modalRef.value?.show();
  },
  hide: () => {
    modalRef.value?.hide();
  },
});
</script>
