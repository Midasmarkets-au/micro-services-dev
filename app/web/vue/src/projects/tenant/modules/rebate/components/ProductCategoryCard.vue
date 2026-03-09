<template>
  <div class="category-card">
    <!-- 卡片头部 -->
    <div class="card-header">
      <div class="header-left">
        <h3 class="category-name" @click="startEditingCategoryName">
          {{ isEditingCategoryName ? "" : category.name }}
          <input
            v-if="isEditingCategoryName"
            v-model="editingCategoryName"
            @blur="saveEditingCategoryName"
            @keyup.enter="saveEditingCategoryName"
            class="category-name-input"
            ref="categoryNameInputRef"
          />
        </h3>
        <span class="category-id">ID: {{ category.id }}</span>
        <span class="product-count"> ({{ category.products.length }}) </span>
      </div>
      <div class="header-actions">
        <el-button type="default" size="small" @click="showBatchImport = true">
          <el-icon><Upload /></el-icon>
          {{ $t("title.batchImport") }}
        </el-button>
        <el-button
          class="ml-1"
          type="danger"
          size="small"
          style="margin-left: 2px"
          @click="handleBatchDeleteConfirm"
          :disabled="
            selectedProducts.length === 0 ||
            category.products.length == selectedProducts.length
          "
        >
          <el-icon><Delete /></el-icon>
          {{ $t("title.batchDelete") }}
          <span v-if="selectedProducts.length > 0">
            ({{ selectedProducts.length }})
          </span>
        </el-button>
        <el-checkbox
          style="margin-left: 2px"
          v-model="isAllSelected"
          @change="handleSelectAll"
          :indeterminate="isIndeterminate"
        >
          {{ $t("title.selectAll") }}
        </el-checkbox>
      </div>
    </div>

    <!-- 卡片内容 -->
    <div class="card-content">
      <!-- 产品列表 -->
      <div class="products-section">
        <div class="products-grid" v-if="category.products.length > 0">
          <div
            v-for="product in category.products"
            :key="product.id"
            class="product-tag"
            :class="{ selected: selectedProducts.includes(product.id) }"
            @click="toggleProductSelection(product.id)"
          >
            <el-checkbox
              :model-value="selectedProducts.includes(product.id)"
              @click.stop
              @change="toggleProductSelection(product.id)"
              class="product-checkbox"
            />
            <span class="product-name">{{ product.code }}</span>
            <div class="product-actions" @click.stop>
              <el-button
                type="text"
                size="small"
                class="btn-edit"
                @click="editProduct(product)"
              >
                <el-icon><Edit /></el-icon>
              </el-button>
              <el-button
                style="margin-left: 1px"
                type="text"
                size="small"
                class="btn-delete"
                @click="deleteProduct(product)"
              >
                <el-icon><Delete /></el-icon>
              </el-button>
            </div>
          </div>
        </div>

        <div v-else class="empty-state">
          <p>{{ $t("title.noProducts") }}</p>
        </div>
      </div>

      <!-- 操作区域 -->
      <div class="actions-section">
        <!-- 添加产品 -->
        <div class="add-product">
          <el-input
            v-model="newProductName"
            @keyup.enter="addProduct"
            :placeholder="$t('title.enterProductName')"
          />
          <el-button
            type="primary"
            @click="addProduct"
            :disabled="!newProductName.trim()"
          >
            {{ $t("action.add") }}
          </el-button>
        </div>
      </div>
    </div>

    <!-- 批量导入弹窗 -->
    <el-dialog
      v-model="showBatchImport"
      :title="$t('title.batchImportProducts')"
      width="500px"
    >
      <el-input
        v-model="importText"
        type="textarea"
        :rows="8"
        :placeholder="$t('title.batchImportPlaceholder')"
      />
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="showBatchImport = false">
            {{ $t("action.cancel") }}
          </el-button>
          <el-button type="primary" @click="handleBatchImport">
            {{ $t("title.confirmImport") }}
          </el-button>
        </div>
      </template>
    </el-dialog>

    <!-- 编辑产品弹窗 -->
    <el-dialog
      v-model="showEditProduct"
      :title="$t('title.editProduct')"
      width="400px"
    >
      <el-input
        v-model="editingProductName"
        :placeholder="$t('title.productName')"
      />
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="showEditProduct = false">
            {{ $t("action.cancel") }}
          </el-button>
          <el-button type="primary" @click="confirmEditProduct">
            {{ $t("action.confirm") }}
          </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, nextTick, computed } from "vue";
import { Delete, Edit, Upload } from "@element-plus/icons-vue";
import { ElMessageBox, ElMessage } from "element-plus";
import { useI18n } from "vue-i18n";

interface Product {
  id: number;
  code: string;
}

const props = defineProps<{
  category: {
    id: number;
    name: string;
    products: Product[];
  };
  type: "trading" | "rebate";
}>();

const { t } = useI18n();

const emit = defineEmits<{
  "update-category": [id: number, updates: any];
  "delete-category": [id: number];
  "add-product": [id: number, name: string];
  "edit-product": [
    id: number,
    oldName: string,
    newName: string,
    symbolId: number
  ];
  "delete-product": [id: number, name: string, symbolId: number];
  "batch-import": [id: number, products: string[]];
  "batch-delete": [id: number, symbolIds: number[]];
}>();

// 响应式数据
const newProductName = ref("");
const showBatchImport = ref(false);
const showEditProduct = ref(false);
const importText = ref("");
const selectedProducts = ref<number[]>([]);
const editingProductName = ref("");
const editingOriginalName = ref("");
const isEditingCategoryName = ref(false);
const editingCategoryName = ref("");
const categoryNameInputRef = ref<HTMLInputElement | null>(null);

// 全选状态计算
const isAllSelected = computed({
  get: () => {
    return (
      props.category.products.length > 0 &&
      selectedProducts.value.length === props.category.products.length
    );
  },
  set: (value: boolean) => {
    if (value) {
      selectedProducts.value = props.category.products.map((p) => p.id);
    } else {
      selectedProducts.value = [];
    }
  },
});

// 半选状态
const isIndeterminate = computed(() => {
  const selectedCount = selectedProducts.value.length;
  return selectedCount > 0 && selectedCount < props.category.products.length;
});

// 处理全选
const handleSelectAll = (value: boolean | string | number) => {
  if (value) {
    selectedProducts.value = props.category.products.map((p) => p.id);
  } else {
    selectedProducts.value = [];
  }
};

// 编辑分类名称
const startEditingCategoryName = () => {
  isEditingCategoryName.value = true;
  editingCategoryName.value = props.category.name;
  nextTick(() => {
    categoryNameInputRef.value?.focus();
  });
};

const saveEditingCategoryName = () => {
  if (
    editingCategoryName.value.trim() &&
    editingCategoryName.value !== props.category.name
  ) {
    emit("update-category", props.category.id, {
      name: editingCategoryName.value.trim(),
    });
  }
  isEditingCategoryName.value = false;
};

const deleteCategory = async () => {
  // 检查是否有产品
  if (props.category.products.length > 0) {
    ElMessage.warning(t("title.cannotDeleteCategoryWithProducts"));
    return;
  }

  try {
    await ElMessageBox.confirm(
      t("title.confirmDeleteCategory"),
      t("tip.confirm"),
      {
        confirmButtonText: t("action.confirm"),
        cancelButtonText: t("action.cancel"),
        type: "warning",
      }
    );
    emit("delete-category", props.category.id);
  } catch (error) {
    // 用户取消操作
  }
};

const addProduct = () => {
  const productName = newProductName.value.trim();
  if (
    productName &&
    !props.category.products.some((p) => p.code === productName)
  ) {
    emit("add-product", props.category.id, productName);
    newProductName.value = "";
  } else if (props.category.products.some((p) => p.code === productName)) {
    ElMessage.warning(t("title.productAlreadyExists"));
  }
};

const editProduct = (product: Product) => {
  editingOriginalName.value = product.code;
  editingProductName.value = product.code;
  showEditProduct.value = true;
};

const confirmEditProduct = () => {
  const newName = editingProductName.value.trim();
  if (newName && newName !== editingOriginalName.value) {
    if (!props.category.products.some((p) => p.code === newName)) {
      const product = props.category.products.find(
        (p) => p.code === editingOriginalName.value
      );
      if (product) {
        emit(
          "edit-product",
          props.category.id,
          editingOriginalName.value,
          newName,
          product.id
        );
        showEditProduct.value = false;
      }
    } else {
      ElMessage.warning(t("title.productNameAlreadyExists"));
    }
  }
};

const deleteProduct = async (product: Product) => {
  try {
    await ElMessageBox.confirm(
      t("title.confirmDeleteProduct", { product: product.code }),
      t("tip.confirm"),
      {
        confirmButtonText: t("action.confirm"),
        cancelButtonText: t("action.cancel"),
        type: "warning",
      }
    );
    emit("delete-product", props.category.id, product.code, product.id);
  } catch (error) {
    // 用户取消操作
  }
};

const handleBatchImport = () => {
  const products = importText.value
    .split("\n")
    .map((p) => p.trim())
    .filter((p) => p);

  if (products.length > 0) {
    emit("batch-import", props.category.id, products);
    showBatchImport.value = false;
    importText.value = "";
  }
};

// 切换产品选择
const toggleProductSelection = (productId: number) => {
  const index = selectedProducts.value.indexOf(productId);
  if (index > -1) {
    selectedProducts.value.splice(index, 1);
  } else {
    selectedProducts.value.push(productId);
  }
};

// 批量删除确认
const handleBatchDeleteConfirm = async () => {
  if (selectedProducts.value.length == props.category.products.length) {
    return;
  }
  if (selectedProducts.value.length === 0) return;

  try {
    await ElMessageBox.confirm(
      t("title.confirmBatchDelete", { count: selectedProducts.value.length }),
      t("tip.confirm"),
      {
        confirmButtonText: t("action.confirm"),
        cancelButtonText: t("action.cancel"),
        type: "warning",
      }
    );

    emit("batch-delete", props.category.id, selectedProducts.value);
    selectedProducts.value = [];
  } catch (error) {
    // 用户取消操作
  }
};
</script>

<style scoped lang="scss">
.category-card {
  background: #ffffff;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  overflow: hidden;
  transition: all 0.2s;

  &:hover {
    box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  }

  &.collapsed {
    .card-content {
      display: none;
    }
  }
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 20px;
  background: #f9fafb;
  border-bottom: 1px solid #e5e7eb;

  .header-left {
    display: flex;
    align-items: center;
    gap: 8px;

    .category-name {
      font-weight: 600;
      color: #1f2937;
      margin: 0;
      cursor: pointer;
      position: relative;
      display: inline-block;
      border-radius: 4px;
      transition: background-color 0.2s;

      &:hover {
        background-color: #f3f4f6;
      }

      .category-name-input {
        font-size: 16px;
        font-weight: 500;
        color: #1f2937;
        border: 1px solid #3b82f6;
        border-radius: 4px;
        padding: 4px 8px;
        outline: none;
        background: white;
        min-width: 150px;
      }
    }

    .category-id {
      color: #6b7280;
      font-weight: normal;
      background: #f3f4f6;
      border-radius: 4px;
    }

    .product-count {
      color: #6b7280;
    }
  }

  .header-actions {
    display: flex;
    align-items: center;

    .btn-collapse,
    .btn-delete-category {
      width: 24px;
      height: 24px;
      border: none;
      background: transparent;
      border-radius: 4px;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      color: #6b7280;
      transition: all 0.2s;
      padding: 0;

      &:hover {
        background: #f3f4f6;
        color: #374151;
      }
    }

    .btn-delete-category:hover {
      background: #fef2f2;
      color: #dc2626;
    }
  }
}

.card-content {
  padding: 20px;
}

.products-section {
  margin-bottom: 20px;

  .products-grid {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
    max-height: 200px;
    overflow-y: auto;

    .product-tag {
      display: flex;
      align-items: center;
      gap: 8px;
      background: #f3f4f6;
      border: 1px solid #d1d5db;
      border-radius: 6px;
      padding: 0px 5px;
      font-size: 14px;
      color: #374151;
      transition: all 0.2s;
      position: relative;
      cursor: pointer;

      &:hover {
        background: #e5e7eb;
        border-color: #9ca3af;
      }

      &.selected {
        background: #dbeafe;
        border-color: #3b82f6;
      }

      .product-checkbox {
        margin-right: 4px;
      }

      .product-name {
        font-weight: 500;
        flex: 1;
      }

      .product-actions {
        display: flex;
        gap: 1px;
        opacity: 0;
        margin-left: 20px;
        transition: opacity 0.2s;

        .btn-edit,
        .btn-delete {
          width: 20px;
          height: 20px;
          border: none;
          background: transparent;
          border-radius: 3px;
          cursor: pointer;
          display: flex;
          align-items: center;
          justify-content: center;
          color: #6b7280;
          font-size: 12px;
          transition: all 0.2s;
          padding: 0;

          &:hover {
            background: #e5e7eb;
            color: #374151;
          }
        }

        .btn-delete {
          color: #dc2626;

          &:hover {
            background: #fef2f2;
            color: #b91c1c;
          }
        }
      }

      &:hover .product-actions {
        opacity: 1;
      }
    }
  }

  .empty-state {
    text-align: center;
    padding: 40px 20px;
    color: #6b7280;
  }
}

.actions-section {
  .add-product {
    display: flex;
    gap: 8px;
    margin-bottom: 16px;
  }
}

.btn {
  padding: 8px 16px;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  border: none;

  &.btn-primary {
    background: #3b82f6;
    color: white;

    &:hover:not(:disabled) {
      background: #2563eb;
    }

    &:disabled {
      background: #9ca3af;
      cursor: not-allowed;
    }
  }

  &.btn-secondary {
    background: #6b7280;
    color: white;

    &:hover {
      background: #4b5563;
    }
  }

  &.btn-danger {
    background: #dc2626;
    color: white;

    &:hover:not(:disabled) {
      background: #b91c1c;
    }

    &:disabled {
      background: #9ca3af;
      cursor: not-allowed;
    }
  }
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  border-radius: 8px;
  width: 90%;
  max-width: 500px;
  max-height: 80vh;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  border-bottom: 1px solid #e5e7eb;

  h3 {
    margin: 0;
    font-size: 18px;
    font-weight: 600;
    color: #1f2937;
  }

  .btn-close {
    width: 32px;
    height: 32px;
    border: none;
    background: transparent;
    border-radius: 4px;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 20px;
    color: #6b7280;

    &:hover {
      background: #f3f4f6;
    }
  }
}

.modal-body {
  padding: 20px;
  flex: 1;
  overflow-y: auto;

  .import-textarea {
    width: 100%;
    height: 200px;
    padding: 12px;
    border: 1px solid #d1d5db;
    border-radius: 6px;
    font-size: 14px;
    font-family: monospace;
    color: #374151;
    resize: vertical;

    &:focus {
      outline: none;
      border-color: #3b82f6;
      box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
    }
  }

  .product-selection {
    max-height: 300px;
    overflow-y: auto;

    .product-checkbox {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px 0;
      cursor: pointer;

      input[type="checkbox"] {
        width: 16px;
        height: 16px;
      }

      span {
        font-size: 14px;
        color: #374151;
      }
    }
  }
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 20px;
  border-top: 1px solid #e5e7eb;
}

// 图标样式
.icon-expand::before {
  content: "▼";
}
.icon-collapse::before {
  content: "▲";
}
.icon-delete::before {
  content: "🗑";
}
.icon-edit::before {
  content: "✏";
}
</style>
