<template>
  <div class="badge" :class="badgeClass[status]" v-if="!showDot">
    {{
      {
        0: $t("status.normal"),
        1: $t("status.paused"),
        2: $t("status.closed"),
      }[props.status]
    }}
  </div>
  <i v-else class="fa-solid fa-circle-dot" :class="dotClass[status]"></i>
</template>
<script lang="ts" setup>
import { ref } from "vue";
const props = defineProps({
  status: { type: Number, required: true },
  showDot: { type: Boolean, default: false },
});
const badgeClass = ref({
  0: "badge-light-success",
  1: "badge-light-warning",
  2: "badge-light-danger",
});

const dotClass = ref({
  0: "text-success",
  1: "text-warning",
  2: "text-danger",
});
</script>
