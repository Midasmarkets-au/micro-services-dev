<template>
  <SimpleForm
    ref="formRef"
    :is-loading="isLoading"
    :title="$t('title.briefDetail')"
    :discard-title="$t('action.close')"
    :disableFooter="true"
    :min-height="775"
    :min-width="1000"
  >
    <LoadingCentralBox class="h-100" v-if="isLoading" />
    <div v-else class="card p-5 h-100">
      <div class="card-header d-flex justify-content-end pb-4">
        <div class="card-title">
          <el-date-picker
            v-model="period"
            type="datetimerange"
            start-placeholder="Start date"
            end-placeholder="End date"
            range-separator="-"
            value-format="YYYY-MM-DD HH:mm:ss"
            :disabled="isLoading"
          />

          <el-button
            type="success"
            plain
            @click="search()"
            :loading="isLoading"
            class="ms-4"
          >
            {{ $t("action.search") }}
          </el-button>
        </div>
      </div>
      <div v-if="recordInfo.mt4 != '' && recordInfo.mt5 == ''" class="mt-7">
        <div class="fs-1 mb-3">{{ $t("tip.mt4") }} - {{ recordInfo.name }}</div>
        <BriefDetailTradeStat
          :accountId="null"
          :fromBrirfDetail="true"
          :accountNumbers="recordInfo.mt4"
          :period="modifiedPeriod"
        ></BriefDetailTradeStat>
      </div>
      <div
        v-else-if="recordInfo.mt5 != '' && recordInfo.mt4 == ''"
        class="mt-7"
      >
        <div class="fs-1 mb-3">{{ $t("tip.mt5") }} - {{ recordInfo.name }}</div>
        <BriefDetailTradeStat
          :accountId="null"
          :fromBrirfDetail="true"
          :accountNumbers="recordInfo.mt5"
          :period="modifiedPeriod"
        ></BriefDetailTradeStat>
      </div>
      <div v-else class="mt-7">
        <div class="fs-1 mb-3">Summary - {{ recordInfo.name }}</div>
        <BriefDetailTradeStatSum
          :accountId="null"
          :fromBrirfDetail="true"
          :accountNumbers="recordInfo"
          :period="modifiedPeriod"
        ></BriefDetailTradeStatSum>
      </div>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { ref, inject, watch } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import LoadingCentralBox from "@/components/LoadingCentralBox.vue";
import BriefDetailTradeStat from "./BriefDetailTradeStat.vue";
import BriefDetailTradeStatSum from "./BriefDetailTradeStatSum.vue";

const today = new Date();
const isLoading = ref(true);
const recordInfo = ref<any>(null);
const formRef = ref<InstanceType<typeof SimpleForm> | null>(null);

const formatDate = (date: Date): string => {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  const hours = String(date.getHours()).padStart(2, "0");
  const minutes = String(date.getMinutes()).padStart(2, "0");
  const seconds = String(date.getSeconds()).padStart(2, "0");

  return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
};

const period = ref([
  formatDate(
    new Date(today.getFullYear(), today.getMonth(), today.getDate(), 0, 0, 0)
  ),
  formatDate(
    new Date(today.getFullYear(), today.getMonth(), today.getDate(), 23, 59, 59)
  ),
]);

const modifiedPeriod = ref([]);

const handleDateChange = () => {
  const endDate = new Date(period.value[1]);
  endDate.setHours(23, 59, 59, 999);
  period.value[1] = formatDate(endDate);

  modifiedPeriod.value = period.value;
};

const search = async () => {
  isLoading.value = true;
  handleDateChange();
  isLoading.value = false;
};

const show = async (_item: any, _period: any) => {
  isLoading.value = true;
  formRef.value?.show();
  recordInfo.value = _item;
  period.value = _period;

  handleDateChange();
  isLoading.value = false;
};

const hide = () => {
  formRef.value?.hide();
};

defineExpose({
  hide,
  show,
});
</script>
