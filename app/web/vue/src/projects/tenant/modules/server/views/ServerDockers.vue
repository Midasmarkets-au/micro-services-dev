<template>
  <div>Dockers</div>
</template>

<script setup lang="ts">
import TableFooter from "@/components/TableFooter.vue";
import { onMounted, ref } from "vue";
import { Search } from "@element-plus/icons-vue";
import TimeShow from "@/components/TimeShow.vue";

const detailVisible = ref(false);

const isLoading = ref(true);
const dialogItem = ref({});
const items = ref(Array<any>());
const criteria = ref({
  page: 1,
  size: 20,
});

const fetchData = async (_page: number) => {
  criteria.value.page = _page;
  isLoading.value = true;

  //   try {
  //     const res = await TenantGlobalService.queryApiLogs(criteria.value);
  //     items.value = res.data;
  //     criteria.value = res.criteria;
  //     isLoading.value = false;
  //   } catch (error) {
  //     console.log(error);
  //   }
};

onMounted(async () => {
  await fetchData(1);
});
</script>
<style scoped></style>
