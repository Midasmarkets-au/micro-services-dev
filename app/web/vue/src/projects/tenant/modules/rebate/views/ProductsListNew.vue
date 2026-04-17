<template>
  <div class="products-list-container">
    <!-- Tab 切换 -->
    <div class="tab-container">
      <el-tabs v-model="activeTab" @tab-change="handleTabChange">
        <el-tab-pane
          :label="$t('title.accountLinkRebateProductCategory')"
          name="rebate"
        />
        <el-tab-pane
          :label="$t('title.allTradingProductCategory')"
          name="trading"
        />
      </el-tabs>
    </div>
    <!-- 内容区域 -->
    <div class="content-area">
      <!-- 交易产品 Tab -->
      <div v-if="activeTab === 'trading'" class="tab-content">
        <div class="action-bar">
          <el-button type="primary" @click="showAddCategoryModal('trading')">
            <el-icon><Plus /></el-icon>
            {{ $t("action.addNewCategory") }}
          </el-button>
        </div>

        <div v-if="loading" class="loading-container">
          <div class="loading-spinner">加载中...</div>
        </div>
        <div v-else class="categories-grid">
          <ProductCategoryCard
            v-for="category in tradingCategories"
            :key="category.id"
            :category="category"
            :type="'trading'"
            @update-category="updateCategory"
            @delete-category="deleteCategory"
            @add-product="addProduct"
            @edit-product="editProduct"
            @delete-product="deleteProduct"
            @batch-import="batchImport"
            @batch-delete="batchDelete"
          />
        </div>
      </div>

      <!-- 返佣表 Tab -->
      <div v-if="activeTab === 'rebate'" class="tab-content">
        <div class="action-bar">
          <!-- <el-button type="primary" @click="showAddCategoryModal('rebate')">
            <el-icon><Plus /></el-icon>
            {{ $t("action.addNewCategory") }}
          </el-button> -->
          <el-button type="default" @click="showRebateSettings = true">
            <el-icon><Setting /></el-icon>
            {{ $t("title.customerBaseRebateTable") }}
          </el-button>
        </div>

        <div v-if="loading" class="loading-container">
          <div class="loading-spinner">加载中...</div>
        </div>
        <div v-else class="categories-grid">
          <ProductCategoryCard
            v-for="category in rebateCategories"
            :key="category.id"
            :category="category"
            :type="'rebate'"
            @update-category="updateCategory"
            @delete-category="deleteCategory"
            @add-product="addProduct"
            @edit-product="editProduct"
            @delete-product="deleteProduct"
            @batch-import="batchImport"
            @batch-delete="batchDelete"
          />
        </div>
      </div>
    </div>

    <!-- 客户基础返佣表弹窗 -->
    <RebateSettingsModal
      v-if="showRebateSettings"
      @close="showRebateSettings = false"
      @save="saveRebateSettings"
    />

    <!-- 添加分类模态框 -->
    <el-dialog
      v-model="showAddCategoryDialog"
      :title="$t('action.addNewCategory')"
      width="500px"
      @close="resetCategoryForm"
    >
      <el-form
        ref="categoryFormRef"
        :model="categoryForm"
        :rules="categoryFormRules"
        label-width="120px"
      >
        <el-form-item :label="$t('fields.categoryName')" prop="category">
          <el-input
            v-model="categoryForm.category"
            :placeholder="$t('placeholder.enterCategoryName')"
          />
        </el-form-item>
        <el-form-item :label="$t('fields.categoryCode')" prop="code">
          <el-input
            v-model="categoryForm.code"
            :placeholder="$t('placeholder.enterCategoryCode')"
          />
        </el-form-item>
      </el-form>

      <template #footer>
        <div class="dialog-footer">
          <el-button @click="showAddCategoryDialog = false">
            {{ $t("action.cancel") }}
          </el-button>
          <el-button
            type="primary"
            @click="handleCreateCategory"
            :loading="categoryLoading"
          >
            {{ $t("action.confirm") }}
          </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, watch } from "vue";
import { ElMessage } from "element-plus";
import { Plus, Setting } from "@element-plus/icons-vue";
import { useI18n } from "vue-i18n";
import ProductCategoryCard from "../components/ProductCategoryCard.vue";
import RebateSettingsModal from "../components/RebateSettingsModal.vue";
import rebateService from "../services/RebateService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
const { t } = useI18n();

// 定义类型
interface Product {
  id: number;
  code: string;
}

interface Category {
  id: number;
  name: string;
  code: string;
  type: number;
  products: Product[];
}

// 响应式数据
const activeTab = ref("rebate");
const showRebateSettings = ref(false);
const loading = ref(false);

// 交易产品分类数据
const tradingCategories = reactive<Category[]>([]);

// 返佣表分类数据
const rebateCategories = reactive<Category[]>([]);

// 添加分类相关
const showAddCategoryDialog = ref(false);
const categoryLoading = ref(false);
const categoryFormRef = ref();
const categoryForm = reactive({
  category: "",
  code: "",
  type: 300,
});

// 表单验证规则
const categoryFormRules = {
  category: [{ required: true, message: "请输入分类名称", trigger: "blur" }],
  code: [{ required: true, message: "请输入分类代码", trigger: "blur" }],
};

// 数据转换函数：将API返回的数据转换为组件需要的格式
// 兼容 gRPC 格式 { id, name, type } 和旧 HTTP 格式 { categoryId, category, type, symbols }
const transformApiData = (apiData: any[]): Category[] => {
  return apiData.map((item) => ({
    id: item.id ?? item.categoryId,
    name: item.name ?? item.category,
    code: item.code || "",
    type: item.type || 0,
    products:
      item.symbols?.map((symbol: any) => ({
        id: symbol.id,
        code: symbol.code,
      })) || [],
  }));
};

// 获取数据
const fetchCategories = async (type: number) => {
  try {
    loading.value = true;
    const response = await rebateService.getProductsListByType(type);
    // gRPC 返回 { categories: [...] }，旧 HTTP 返回数组
    const rawData = response.categories ?? response.data ?? response;
    const transformedData = transformApiData(Array.isArray(rawData) ? rawData : []);

    if (type === 300) {
      rebateCategories.splice(0, rebateCategories.length, ...transformedData);
    } else if (type === 400) {
      tradingCategories.splice(0, tradingCategories.length, ...transformedData);
    }
  } catch (error) {
    console.error("获取分类数据失败:", error);
  } finally {
    loading.value = false;
  }
};

// 组件挂载时获取默认数据
onMounted(() => {
  fetchCategories(300); // 默认加载 rebate 数据
});

// 监听 activeTab 变化，重新获取数据
watch(activeTab, (newTab) => {
  const type = newTab === "rebate" ? 300 : 400;
  fetchCategories(type);
});

// Tab 切换
const handleTabChange = (tab: string | number) => {
  activeTab.value = String(tab);
  // 数据获取由 watch 自动处理
};

// 显示添加分类模态框
const showAddCategoryModal = (type: string) => {
  categoryForm.type = type === "trading" ? 400 : 300;
  showAddCategoryDialog.value = true;
};

// 重置分类表单
const resetCategoryForm = () => {
  categoryForm.category = "";
  categoryForm.code = "";
  categoryFormRef.value?.resetFields();
};

// 创建分类
const handleCreateCategory = async () => {
  if (!categoryFormRef.value) return;

  try {
    await categoryFormRef.value.validate();
    categoryLoading.value = true;

    await rebateService.createCategory({
      category: categoryForm.category,
      code: categoryForm.code,
      type: categoryForm.type,
    });

    ElMessage.success(t("title.categoryCreatedSuccess"));
    showAddCategoryDialog.value = false;
    resetCategoryForm();

    // 重新加载当前 tab 数据
    const type = activeTab.value === "trading" ? 400 : 300;
    await fetchCategories(type);
  } catch (error) {
    console.error("创建分类失败:", error);
    ElMessage.error(t("title.categoryNotFound"));
  } finally {
    categoryLoading.value = false;
  }
};

// 更新分类
const updateCategory = async (categoryId: number, updates: any) => {
  try {
    const categories =
      activeTab.value === "trading" ? tradingCategories : rebateCategories;
    const category = categories.find((c) => c.id === categoryId);
    if (category) {
      // 如果是更新分类名称，调用 API
      if (updates.name) {
        await rebateService.updateCategory(categoryId, {
          category: updates.name,
          code: category.code,
          type: category.type,
        });
        ElMessage.success(t("title.categoryUpdatedSuccess"));
      }
      // 更新本地状态
      Object.assign(category, updates);
    }
  } catch (error) {
    console.error("更新分类失败:", error);
    ElMessage.error(t("title.categoryUpdateFailed"));
  }
};

// 删除分类
const deleteCategory = async (categoryId: number) => {
  try {
    await rebateService.deleteCategory(categoryId);
    ElMessage.success(t("title.categoryDeletedSuccess"));

    // 重新加载数据
    const type = activeTab.value === "trading" ? 400 : 300;
    await fetchCategories(type);
  } catch (error) {
    console.error("删除分类失败:", error);
    ElMessage.error(t("title.categoryDeleteFailed"));
  }
};

// 添加产品
const addProduct = async (categoryId: number, productName: string) => {
  try {
    const type = activeTab.value === "trading" ? 400 : 300;
    const category = (
      activeTab.value === "trading" ? tradingCategories : rebateCategories
    ).find((c) => c.id === categoryId);

    if (!category) {
      ElMessage.error(t("title.categoryNotFound"));
      return;
    }

    await rebateService.createSymbol({
      Code: productName,
      Category: category.name,
      CategoryId: categoryId.toString(),
      Type: type.toString(),
    });

    ElMessage.success(t("title.productAddedSuccess"));
    // 重新加载数据
    await fetchCategories(type);
  } catch (error) {
    console.error("添加产品失败:", error);
    ElMessage.error(t("title.productAddFailed"));
  }
};

// 编辑产品
const editProduct = async (
  categoryId: number,
  oldName: string,
  newName: string,
  symbolId: number
) => {
  try {
    const type = activeTab.value === "trading" ? 400 : 300;
    const category = (
      activeTab.value === "trading" ? tradingCategories : rebateCategories
    ).find((c) => c.id === categoryId);

    if (!category) {
      ElMessage.error(t("title.categoryNotFound"));
      return;
    }

    await rebateService.updateSymbol(symbolId, {
      code: newName,
      category: category.name,
      categoryId: categoryId,
      type: type,
    });

    ElMessage.success(t("title.productUpdatedSuccess"));
    // 重新加载数据
    await fetchCategories(type);
  } catch (error) {
    console.error("更新产品失败:", error);
    ElMessage.error(t("title.productUpdateFailed"));
  }
};

// 删除产品
const deleteProduct = async (
  categoryId: number,
  productName: string,
  symbolId: number
) => {
  try {
    await rebateService.deleteSymbol(symbolId);
    ElMessage.success(t("title.productDeletedSuccess"));

    // 重新加载数据
    const type = activeTab.value === "trading" ? 400 : 300;
    await fetchCategories(type);
  } catch (error) {
    MsgPrompt.error(error);
  }
};

// 批量导入
const batchImport = async (categoryId: number, products: string[]) => {
  try {
    const type = activeTab.value === "trading" ? 400 : 300;
    const category = (
      activeTab.value === "trading" ? tradingCategories : rebateCategories
    ).find((c) => c.id === categoryId);

    if (!category) {
      ElMessage.error(t("title.categoryNotFound"));
      return;
    }

    // 批量创建产品
    const promises = products.map((product) =>
      rebateService.createSymbol({
        Code: product,
        Category: category.name,
        CategoryId: categoryId.toString(),
        Type: type.toString(),
      })
    );

    await Promise.all(promises);
    ElMessage.success(
      t("title.productsImportedSuccess", { count: products.length })
    );

    // 重新加载数据
    await fetchCategories(type);
  } catch (error) {
    console.error(t("title.productsImportFailed") + ":", error);
    ElMessage.error(t("title.productsImportFailed"));
  }
};

// 批量删除
const batchDelete = async (categoryId: number, symbolIds: number[]) => {
  try {
    // 使用批量删除 API
    await rebateService.batchDeleteSymbols(symbolIds);

    ElMessage.success(
      t("title.productsDeletedSuccess", { count: symbolIds.length })
    );

    // 重新加载数据
    const type = activeTab.value === "trading" ? 400 : 300;
    await fetchCategories(type);
  } catch (error) {
    console.error(t("title.productsImportFailed") + ":", error);
    ElMessage.error(t("title.productsDeleteFailed"));
  }
};

// 保存返佣设置
const saveRebateSettings = (settings: any) => {
  console.log("保存返佣设置:", settings);
  showRebateSettings.value = false;
};
</script>

<style scoped lang="scss">
.products-list-container {
  padding: 24px;
  background: #ffffff;
  min-height: 100vh;
}

.tab-container {
  margin-bottom: 24px;
}

.content-area {
  .action-bar {
    display: flex;
    gap: 12px;
    margin-bottom: 24px;
  }

  .categories-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
    gap: 24px;

    @media (max-width: 768px) {
      grid-template-columns: 1fr;
    }
  }

  .loading-container {
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 200px;

    .loading-spinner {
      font-size: 16px;
      color: #6b7280;
    }
  }
}
</style>
