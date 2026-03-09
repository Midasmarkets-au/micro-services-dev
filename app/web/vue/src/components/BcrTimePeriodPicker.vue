<template>
  <div class="d-flex gap-2 align-items-center">
    <el-date-picker
      :class="isMobile ? 'w-100' : 'w-125px'"
      v-model="fromDate"
      @change="onFromChange(fromDate)"
      type="date"
      :placeholder="$t('fields.startDate')"
      :disabled="props?.isLoading"
      format="YYYY/MM/DD"
      value-format="YYYY-MM-DD"
    />
    <span> - </span>
    <el-date-picker
      :class="isMobile ? 'w-100' : 'w-125px'"
      v-model="toDate"
      @change="onToChange(toDate)"
      type="date"
      :placeholder="$t('fields.endDate')"
      :disabled="props?.isLoading"
      format="YYYY/MM/DD"
      value-format="YYYY-MM-DD"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from "vue";
import { isMobile } from "@/core/config/WindowConfig";
import { TimeZoneService } from "@/core/plugins/TimerService";
import moment from "moment";
const props = withDefaults(
  defineProps<{
    from?: string | null;
    to?: string | null;
    startPlaceholder?: string;
    endPlaceholder?: string;
    isLoading?: boolean;
  }>(),
  {
    startPlaceholder: "Start Date",
    endPlaceholder: "End Date",
    isLoading: false,
  }
);

const emits = defineEmits<{
  (e: "update:from", value: string | null | undefined): void;
  (e: "update:to", value: string | null | undefined): void;
}>();

const fromDate = ref(props.from);
const toDate = ref(props.to);
const onFromChange = (val: string | null | undefined) => {
  if (!val) {
    emits("update:from", null);
    return;
  }

  emits("update:from", val);
};

const onToChange = (val: string | null | undefined) => {
  if (!val) {
    emits("update:to", null);
    return;
  }

  emits("update:to", val);
};

watch([() => props.from, () => props.to], ([from, to]) => {
  fromDate.value = from;
  toDate.value = to;
});
</script>

<style scoped></style>
