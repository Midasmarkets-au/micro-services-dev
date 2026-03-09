<template>
  <div class="d-flex align-items-center h-100">
    <label class="filter-label me-2" style="">
      {{ $t("fields.period") }}
    </label>
    <el-date-picker
      class="w-250px"
      v-model="period"
      type="daterange"
      :start-placeholder="$t('fields.startDate')"
      :end-placeholder="$t('fields.endDate')"
      @change="updatePeriod"
    />
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
const period = ref([] as any);
const isFiltered = ref(false);

const emit = defineEmits(["updatePeroid"]);

const updatePeriod = () => {
  let from = "";
  let to = "";
  if (period.value === null) {
    isFiltered.value = false;
  } else {
    from = period.value[0];
    to = period.value[1];
    isFiltered.value = true;
  }
  emit("updatePeroid", { from, to });
};

defineExpose({
  isFiltered,
});
</script>
