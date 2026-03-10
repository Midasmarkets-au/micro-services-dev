<template>
  <div>
    <div
      class="step-title"
      style="
        text-decoration: underline;
        text-decoration-color: #ffce00;
        text-decoration-thickness: 3px;
        text-underline-offset: 5px;
      "
    >
      {{ $t("title.walletNoteOn") }}
      {{ selectedService.name }}
      {{ $t("title.withdraw") }}
    </div>
    <div
      class="fs-6 text-gray-600 h-100"
      v-html="servicePolicy"
      style="overflow: scroll"
    ></div>
  </div>
</template>
<script lang="ts" setup>
import { inject } from "vue";

const servicePolicy = inject("servicePolicy");
const selectedService = inject<any>("selectedService");
</script>
<style lang="scss" scoped></style>
