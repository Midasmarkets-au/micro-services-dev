<template>
  <LoadingRing />
</template>

<script lang="ts" setup>
import { onMounted } from "vue";
import { axiosInstance } from "@/core/services/api.client";
import { useRouter } from "vue-router";

const props = defineProps({
  tokenKey: {
    type: String,
    required: true,
  },
});

const router = useRouter();

onMounted(async () => {
  const key = props.tokenKey;
  if (key) {
    window.localStorage.clear();
    try {
      await axiosInstance.post("/api/v2/auth/god-mode/exchange", { key });
    } catch (e: any) {
      window.location.href = "/";
      return;
    }
    localStorage.setItem("id_token", "cookie");
    await router.push({ name: "dashboard" });
  }
});
</script>
