<template>
  <LoadingRing />
</template>

<script lang="ts" setup>
import { onMounted } from "vue";
import { axiosInstance } from "@/core/services/api.client";

const props = defineProps({
  tokenKey: {
    type: String,
    required: true,
  },
});

onMounted(async () => {
  const key = props.tokenKey;
  if (key) {
    window.localStorage.clear();
    try {
      await axiosInstance.post("/api/v2/auth/god-mode/exchange", { key });
    } catch {
      window.location.href = "/";
      return;
    }
    localStorage.setItem("id_token", "cookie");
    window.location.href = "/portal";
  }
});
</script>
