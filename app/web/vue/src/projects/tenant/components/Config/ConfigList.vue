<template>
  <p>hi</p>
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";
import TenantGlobalService from "@/projects/tenant/services/TenantGlobalService";
const props = defineProps({
  rowId: {
    type: Number,
    required: true,
  },
  category: {
    type: String,
    required: true,
  },
});
const isLoading = ref(false);
const data = ref([]);
const fecthData = async () => {
  isLoading.value = true;
  try {
    const res = await TenantGlobalService.queryConfig(
      props.rowId,
      props.category
    );
    data.value = res;
  } catch (e) {
    console.log(e);
  }

  isLoading.value = false;
};

onMounted(() => {
  fecthData();
});
</script>
