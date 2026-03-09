<template>
  <div class="badge" :class="badgeClass[status]">
    {{ $t("status." + props.status) }}
  </div>
</template>
<script lang="ts" setup>
import { ref } from "vue";
const props = defineProps({
  status: { type: String, required: true },
});
const badgeClass = ref({
  pedding: "badge-light-primary",
  approved: "badge-light-success",
  rejected: "badge-light-danger",
  cancelled: "badge-light-danger",
  completed: "badge-light-success",
  processing: "badge-light-info",
  pending: "badge-light-primary",
  active: "badge-light-success",
  inactive: "badge-light-danger",
  suspended: "badge-light-danger",
  expired: "badge-light-danger",
  blocked: "badge-light-danger",
  unverified: "badge-light-warning",
  verified: "badge-light-success",
  unpaid: "badge-light-warning",
  paid: "badge-light-success",
  failed: "badge-light-danger",
});
</script>
