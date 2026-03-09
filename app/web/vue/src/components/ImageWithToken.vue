<template>
  <img ref="imageRef" alt="Loaded Image" />
</template>

<script setup lang="ts">
import { onMounted, ref, watch } from "vue";
import TenantGlobalService from "@/projects/tenant/services/TenantGlobalService";

const props = defineProps<{
  src: string;
}>();
const imageRef = ref<HTMLImageElement | null>(null);

const setImageUrlSrc = async () => {
  if (!imageRef.value) return;
  imageRef.value.src = await TenantGlobalService.getImageUrlWithToken(
    props.src
  );
};

onMounted(() => {
  if (props.src) {
    setImageUrlSrc();
  }
});

watch(() => props.src, setImageUrlSrc);
</script>

<style scoped></style>
