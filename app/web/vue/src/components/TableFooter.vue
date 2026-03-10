<template>
  <div
    class="my-5 d-flex justify-content-center align-items-center"
    v-if="criteria"
  >
    <el-pagination
      :hide-on-single-page="true"
      :page-size="criteria?.size"
      layout="prev, pager, next"
      :current-page="page"
      @update:current-page="pageChange($event)"
      :page-count="
        criteria?.total ? Math.ceil(criteria?.total / criteria?.size) : 1
      "
    />
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, watch } from "vue";

const props = withDefaults(
  defineProps<{
    criteria?: any;
    total?: number;
    itemsPerPage?: number;
    itemsPerPageDropdownEnabled?: boolean;
  }>(),
  {
    total: 5,
    itemsPerPage: 10,
    itemsPerPageDropdownEnabled: true,
  }
);

const emits = defineEmits([
  "update:itemsPerPage",
  "page-change",
  "update-page",
]);
const page = ref(1);
const inputItemsPerPage = ref(10);

watch(
  () => props.criteria?.page,
  (newPage) => {
    if (typeof newPage !== "number") return;
    page.value = newPage;
  }
);

onMounted(() => {
  inputItemsPerPage.value = props.criteria
    ? (props.criteria.size as number)
    : props.itemsPerPage;
});

const pageChange = (newPage: number) => {
  if (page.value === newPage) return;
  page.value = newPage;
  emits("page-change", page.value);
};
</script>
