<template>
  <el-checkbox-group v-model="tagsOptions">
    <el-checkbox
      :disable="isLoading"
      v-for="(item, index) in tags"
      :key="index"
      :label="item.name"
      :checked="item.enabled"
      @change="updateTages(item)"
    >
    </el-checkbox>
  </el-checkbox-group>
</template>
<script setup lang="ts">
import { defineProps, onMounted, ref } from "vue";
import TenantGlobalService from "@/projects/tenant/services/TenantGlobalService";
import { en } from "element-plus/es/locale";

const props = defineProps({
  type: { type: String, required: true },
  id: { type: Number, required: true },
});

const isLoading = ref(false);

interface Tag {
  name: string;
  enabled: boolean;
}

const tagsOptions = ref([]);

const tags = ref<Tag[]>([]);

onMounted(async () => {
  fetchData();
});

const fetchData = async () => {
  isLoading.value = true;
  const res = await TenantGlobalService.getTagsById({
    type: props.type,
    rowId: props.id,
  });
  tags.value = res;
  console.log(tags.value);
  isLoading.value = false;
};

const updateTages = async (item) => {
  console.log("Update tags", item);
  item.enabled = !item.enabled;
  const res = await TenantGlobalService.updateTagsById({
    type: props.type,
    rowId: props.id,
    name: item.name,
    enabled: item.enabled,
  });
  console.log(res);
};
</script>
