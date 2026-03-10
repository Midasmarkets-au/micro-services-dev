<template>
  <span
    class="badge fw-bold fs-8 px-2 py-1 ms-2"
    :class="badgeClass[role]"
    v-for="(role, index) in props.roles"
    :key="index"
  >
    {{ $t("type.role." + role) }}
  </span>
</template>
<script lang="ts" setup>
import { ref } from "vue";
const props = defineProps({
  roles: { type: Array, required: true },
});

const badgeClass = ref({
  SuperAdmin: "badge-danger",
  TenantAdmin: "badge-light-danger",
  Manage: "badge-light-success",
  User: "badge-light-primary",
  IB: "badge-success",
  Guest: "badge-light-info",
  Sales: "badge-warning",
  Client: "badge-primary",
});
</script>
