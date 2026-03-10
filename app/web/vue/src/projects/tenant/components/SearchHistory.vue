<template>
  <span
    class="badge bg-secondary cursor-pointer p-2 m-2 history-item"
    v-for="(item, index) in searchHistory"
    v-show="index < 5"
    :key="index"
    ><span class="overflow-hidden text-color" @click="reSearch(item)">{{
      item
    }}</span>
    <span
      class="badge bg-danger cursor-pointer delete-icon"
      @click.prevent="deleteSearchHistory(index)"
      >x</span
    >
  </span>
</template>
<script setup lang="ts">
import { ref, onMounted, defineProps, defineEmits } from "vue";
import { useStore } from "@/store";

const store = useStore();
const user = store.state.AuthModule.user;

const searchHistory = ref<string[]>([]);
const props = defineProps<{
  category: string;
}>();
const emit = defineEmits(["reSearch"]);

const localStorageKey = `searchHistory_${user.tenancy}_${props.category}`;

onMounted(() => {
  searchHistory.value = JSON.parse(
    localStorage.getItem(localStorageKey) || "[]"
  );
});

const updateSearchHistory = (searchText: string) => {
  const index = searchHistory.value.indexOf(searchText);
  if (index > -1) {
    searchHistory.value.splice(index, 1);
  }
  searchHistory.value.unshift(searchText);
  // if (searchHistory.value.length > 10) {
  //   searchHistory.value.length = 10;
  // }
  // 更新 localStorage 中的搜索历史
  localStorage.setItem(localStorageKey, JSON.stringify(searchHistory.value));
};

const deleteSearchHistory = (index: number) => {
  console.log("deleteSearchHistory", index);
  searchHistory.value.splice(index, 1);
  localStorage.setItem(localStorageKey, JSON.stringify(searchHistory.value));
};

const reSearch = (searchText: string) => {
  emit("reSearch", searchText);
};

defineExpose({
  updateSearchHistory,
});
</script>
<style scoped>
.history-item {
  max-width: 110px;
}

.delete-icon {
  padding: 0px 4px !important;
  margin: 0 0 0 2px;
  width: 12px;
  font-size: 9px;
  height: 12px;
  border-radius: 50%;
}
.text-color {
  color: #524f4f;
}
</style>
