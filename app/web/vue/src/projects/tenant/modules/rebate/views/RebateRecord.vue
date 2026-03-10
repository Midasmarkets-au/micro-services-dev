<template>
  <div>
    <ul
      class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
    >
      <li class="nav-item">
        <a
          class="nav-link text-active-primary pb-4"
          :class="[
            { active: tab === 'record' },
            { 'disabled opacity-50 pe-none': isLoading },
          ]"
          data-bs-toggle="tab"
          href="#"
          @click="tab = 'record'"
          >{{ $t("title.record") }}</a
        >
      </li>

      <li class="nav-item">
        <a
          class="nav-link text-active-primary pb-4"
          :class="[
            { active: tab === 'report' },
            { 'disabled opacity-50 pe-none': isLoading },
          ]"
          data-bs-toggle="tab"
          href="#"
          @click="tab = 'report'"
          >{{ $t("title.report") }}</a
        >
      </li>
    </ul>
    <IbRecord v-if="tab === 'record'" />
    <IbRebateReport v-if="tab === 'report'" />
  </div>
</template>

<script setup lang="ts">
import IbRecord from "@/projects/tenant/modules/rebate/components/IbRecord.vue";
import IbRebateReport from "@/projects/tenant/modules/rebate/components/IbRebateReport.vue";
import { ref, provide } from "vue";
const tab = ref("record");
const isLoading = ref(false);
provide("isLoading", isLoading);
</script>

<style scoped></style>
