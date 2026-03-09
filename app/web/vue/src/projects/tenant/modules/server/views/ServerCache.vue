<template>
  <div class="card">
    <!-- 卡片头部 -->
    <div class="card-header border-0 pt-6">
      <div class="card-title">
        <h3>{{ $t("server.redisCache") }}</h3>
      </div>
      <div class="card-toolbar">
        <el-button
          type="danger"
          :disabled="selectedKeys.length === 0"
          @click="handleBatchDelete"
        >
          {{ $t("action.delete") }}
          <span v-if="selectedKeys.length > 0"
            >({{ selectedKeys.length }})</span
          >
        </el-button>
      </div>
    </div>

    <!-- 卡片内容 -->
    <div class="card-body pt-0">
      <div v-if="isLoading" class="text-center py-10">
        <el-icon class="is-loading" :size="30">
          <Loading />
        </el-icon>
        <div class="mt-3">{{ $t("tip.loading") }}</div>
      </div>

      <div v-else-if="keys.length === 0" class="text-center py-10">
        <el-empty :description="$t('server.noCacheKeys')" />
      </div>

      <div v-else>
        <!-- 全选 -->
        <div class="mb-4">
          <el-checkbox
            v-model="selectAll"
            :indeterminate="isIndeterminate"
            @change="handleSelectAll"
          >
            {{ $t("action.all") }}
          </el-checkbox>
        </div>

        <!-- Key 列表 -->
        <el-checkbox-group
          v-model="selectedKeys"
          @change="handleSelectionChange"
        >
          <div
            v-for="key in keys"
            :key="key"
            class="key-item d-flex align-items-center justify-content-between mb-3 p-1 border rounded"
          >
            <el-checkbox :label="key" class="flex-grow-1">
              <span class="key-text">{{ key }}</span>
            </el-checkbox>
            <el-button
              type="danger"
              size="small"
              link
              @click="handleDeleteSingle(key)"
            >
              {{ $t("action.delete") }}
            </el-button>
          </div>
        </el-checkbox-group>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from "vue";
import { Loading } from "@element-plus/icons-vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { useI18n } from "vue-i18n";
import ServerServices from "../services/ServerServices";

const { t } = useI18n();

const isLoading = ref(true);
const keys = ref<string[]>([]);
const selectedKeys = ref<string[]>([]);
const selectAll = ref(false);

// 是否是半选状态
const isIndeterminate = computed(() => {
  return (
    selectedKeys.value.length > 0 &&
    selectedKeys.value.length < keys.value.length
  );
});

// 获取 Redis Keys
const fetchKeys = async () => {
  isLoading.value = true;
  try {
    const response = await ServerServices.getRedisKeys();
    keys.value = response.data.keys || [];
  } catch (error) {
    console.error("获取 Redis keys 失败:", error);
    ElMessage.error(t("server.fetchKeysFailed"));
  } finally {
    isLoading.value = false;
  }
};

// 删除 Keys
const deleteKeys = async (keysToDelete: string[]) => {
  try {
    await ServerServices.deleteRedisKeys(keysToDelete);
    ElMessage.success(
      t("server.deleteKeysSuccess", { count: keysToDelete.length })
    );
    // 从列表中移除已删除的 keys
    keys.value = keys.value.filter((key) => !keysToDelete.includes(key));
    // 清空选中状态
    selectedKeys.value = selectedKeys.value.filter(
      (key) => !keysToDelete.includes(key)
    );
  } catch (error) {
    console.error("删除 Redis keys 失败:", error);
    ElMessage.error(t("server.deleteKeysFailed"));
  }
};

// 全选/取消全选
const handleSelectAll = (value: string | number | boolean) => {
  if (value) {
    selectedKeys.value = [...keys.value];
  } else {
    selectedKeys.value = [];
  }
};

// 选择变化时更新全选状态
const handleSelectionChange = () => {
  selectAll.value = selectedKeys.value.length === keys.value.length;
};

// 批量删除
const handleBatchDelete = async () => {
  if (selectedKeys.value.length === 0) {
    return;
  }

  try {
    await ElMessageBox.confirm(
      t("server.confirmDeleteKeys", { count: selectedKeys.value.length }),
      t("tip.confirm"),
      {
        confirmButtonText: t("action.confirm"),
        cancelButtonText: t("action.cancel"),
        type: "warning",
      }
    );

    await deleteKeys([...selectedKeys.value]);
  } catch (error) {
    // 用户取消删除
    if (error === "cancel") {
      return;
    }
    console.error(error);
  }
};

// 删除单个 key
const handleDeleteSingle = async (key: string) => {
  try {
    await ElMessageBox.confirm(
      t("server.confirmDeleteKey", { key }),
      t("tip.confirm"),
      {
        confirmButtonText: t("action.confirm"),
        cancelButtonText: t("action.cancel"),
        type: "warning",
      }
    );

    await deleteKeys([key]);
  } catch (error) {
    // 用户取消删除
    if (error === "cancel") {
      return;
    }
    console.error(error);
  }
};

onMounted(async () => {
  await fetchKeys();
});
</script>

<style scoped>
.key-item {
  transition: all 0.2s ease;
  background-color: var(--bs-gray-100);
}

.key-item:hover {
  background-color: var(--bs-gray-200);
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.key-text {
  font-family: "Courier New", monospace;
  font-size: 14px;
  color: var(--bs-gray-700);
  word-break: break-all;
}
</style>
