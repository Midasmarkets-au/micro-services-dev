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
            :type="isMonthlyType ? 'month' : 'datetimerange'"
            :disabled-date="disableFutureMonth"
            :placeholder="isMonthlyType ? $t('fields.date') : undefined"
            :start-placeholder="
              isMonthlyType ? undefined : $t('fields.startDate')
            "
            :end-placeholder="isMonthlyType ? undefined : $t('fields.endDate')"
            :format="isMonthlyType ? 'YYYY-MM' : 'YYYY-MM-DD HH:mm:ss'"
            :value-format="isMonthlyType ? 'YYYY-MM' : 'YYYY-MM-DD HH:mm:ss'"
            :disabled="isLoading"
          />
          <div
            v-if="isMonthlyType && monthlyDateRange.from && monthlyDateRange.to"
            class="mt-2 text-muted fs-7"
          >
            {{ $t("fields.startDate") }}: {{ monthlyDateRange.from }}<br />
            {{ $t("fields.endDate") }}: {{ monthlyDateRange.to }}
          </div>
        </el-form-item>
        <el-form-item :label="$t('fields.status')" required>
          <el-select v-model="stateId" :disabled="isLoading">
            <el-option :label="$t('fields.all')" :value="''"></el-option>
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
import { computed, ref } from "vue";
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
const isMonthlyType = computed(
  () => props.reportType === ReportRequestTypes.MonthlyEquityReport
);
const disableFutureMonth = (date: Date) => {
  if (!isMonthlyType.value) return false;

  const now = new Date();
  const dateYear = date.getFullYear();
  const dateMonth = date.getMonth();
  const currentYear = now.getFullYear();
  const currentMonth = now.getMonth();
  return (
    dateYear > currentYear ||
    (dateYear === currentYear && dateMonth > currentMonth)
  );
};
const period = ref<any>(isMonthlyType.value ? "" : []);
const stateId = ref<
  string | number | boolean | unknown[] | Record<string, any>
>("");
const useClosingTime = ref(true);
const criteria = ref<any>({});
const emits = defineEmits(["fetchData"]);
const formData = ref<{
  name: string;
  type: number;
  query: any;
}>({
  name: "",
  type: props.reportType,
  query: criteria.value,
});

const toUtcISOString = (value: string) => `${value.replace(" ", "T")}Z`;
const formatDate = (date: Date) => {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
};
const getMonthlyRange = (value: string) => {
  const [year, month] = value.split("-").map(Number);
  if (!year || !month) {
    return { from: null, to: null };
  }

  const monthString = String(month).padStart(2, "0");
  const from = `${year}-${monthString}-01 00:00:00`;
  const now = new Date();
  const isCurrentMonth =
    year === now.getFullYear() && month === now.getMonth() + 1;
  const to = isCurrentMonth
    ? `${formatDate(
        new Date(now.getFullYear(), now.getMonth(), now.getDate() - 1)
      )} 23:59:59`
    : `${year}-${monthString}-${String(
        new Date(year, month, 0).getDate()
      ).padStart(2, "0")} 23:59:59`;
  return { from, to };
};
const monthlyDateRange = computed(() => {
  if (
    !isMonthlyType.value ||
    typeof period.value !== "string" ||
    !period.value
  ) {
    return { from: null, to: null };
  }
  return getMonthlyRange(period.value);
});

const convertToISO = (prd: string[] | string) => {
  if (isMonthlyType.value) {
    const { from, to } = getMonthlyRange(prd as string);
    if (!from || !to) {
      return { from: null, to: null };
    }
    return { from: toUtcISOString(from), to: toUtcISOString(to) };
  }

  const val = (prd as string[]).map((x) => toUtcISOString(x));
  return { from: val ? val[0] : null, to: val ? val[1] : null };
};

const submitRebateReportRequest = async () => {
  isLoading.value = true;
  criteria.value = convertToISO(period.value as string[] | string);
  criteria.value.stateId = stateId.value === "" ? null : stateId.value;
  criteria.value.useClosingTime = useClosingTime.value;
  formData.value.query = criteria.value;
  // 根据 useClosingTime 自动添加后缀
  const timeBaseLabel = useClosingTime.value
    ? "MT5 ClosingTime Based"
    : "ReleasedTime Based";
  const monthLabel =
    isMonthlyType.value && typeof period.value === "string" && period.value
      ? `${period.value} `
      : "";
  formData.value.name = `${formData.value.name} (${monthLabel}${timeBaseLabel})`;
  console.log(formData.value.name);
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
    period.value = isMonthlyType.value ? "" : [];
    stateId.value = "";
    useClosingTime.value = true;
    modalRef.value?.show();
  },
  hide: () => {
    modalRef.value?.hide();
  },
});
</script>
