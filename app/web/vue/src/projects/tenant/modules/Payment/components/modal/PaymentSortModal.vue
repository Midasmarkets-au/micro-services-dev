<template>
  <el-dialog
    v-model="visible"
    :title="$t('action.adjustClientDisplayOrder')"
    width="500px"
    :close-on-click-modal="false"
  >
    <div class="mb-3 text-muted small">
      <i class="fa-solid fa-info-circle me-1"></i>
      {{ $t("tip.dragToSort") }}
    </div>

    <div class="sort-list" @dragover.prevent @drop="handleDrop">
      <div
        v-for="(item, index) in sortList"
        :key="item.id"
        class="sort-item"
        :class="{ dragging: dragIndex === index }"
        draggable="true"
        @dragstart="handleDragStart(index)"
        @dragenter="handleDragEnter(index)"
        @dragend="handleDragEnd"
      >
        <div class="d-flex align-items-center">
          <i class="fa-solid fa-grip-vertical me-3 text-muted cursor-move"></i>
          <div>
            <div class="fw-semibold">{{ item.name }}</div>
            <div class="text-muted small">
              {{ item.group || "System Manual" }}
            </div>
          </div>
        </div>
        <span class="badge bg-light text-dark">{{ index + 1 }}</span>
      </div>
    </div>

    <template #footer>
      <el-button @click="visible = false" :disabled="isLoading">{{
        $t("action.cancel")
      }}</el-button>
      <el-button type="primary" @click="handleSave" :loading="isLoading">{{
        $t("action.save")
      }}</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref } from "vue";
import PaymentService from "../../services/PaymentService";
import MsgPrompt from "@/core/plugins/MsgPrompt";

const visible = ref(false);
const isLoading = ref(false);
const sortList = ref<any[]>([]);
const dragIndex = ref<number | null>(null);
const dragOverIndex = ref<number | null>(null);

const emits = defineEmits(["update"]);

const handleDragStart = (index: number) => {
  dragIndex.value = index;
};

const handleDragEnter = (index: number) => {
  if (dragIndex.value === null || dragIndex.value === index) return;

  const item = sortList.value[dragIndex.value];
  sortList.value.splice(dragIndex.value, 1);
  sortList.value.splice(index, 0, item);
  dragIndex.value = index;
};

const handleDragEnd = () => {
  dragIndex.value = null;
  dragOverIndex.value = null;
};

const handleDrop = () => {
  handleDragEnd();
};

const handleSave = async () => {
  isLoading.value = true;
  try {
    const payload = sortList.value.map((item, index) => ({
      id: item.id,
      sort: index + 1,
    }));
    const params = {
      items: payload,
    };
    await PaymentService.batchUpdateSort(params);
    MsgPrompt.success("Sort updated successfully");
    visible.value = false;
    emits("update");
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

const show = (services: Record<string, any[]>) => {
  // 将分组数据扁平化为列表
  const list: any[] = [];
  Object.keys(services).forEach((group) => {
    services[group].forEach((item) => {
      list.push({
        ...item,
        group: group,
      });
    });
  });
  sortList.value = list;
  visible.value = true;
};

defineExpose({
  show,
});
</script>

<style scoped lang="scss">
.sort-list {
  max-height: 400px;
  overflow-y: auto;
}

.sort-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  margin-bottom: 8px;
  background: #f8f9fa;
  border: 1px solid #e9ecef;
  border-radius: 8px;
  cursor: grab;
  transition: all 0.2s ease;

  &:hover {
    background: #e9ecef;
  }

  &.dragging {
    opacity: 0.5;
    background: #dee2e6;
  }
}

.cursor-move {
  cursor: move;
}
</style>
