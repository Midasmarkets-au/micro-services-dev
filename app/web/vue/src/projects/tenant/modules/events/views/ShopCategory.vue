<template>
  <el-card>
    <template #header>
      <div class="d-flex justify-content-between align-items-center">
        <div>
          <span>{{ $t("fields.status") }}</span>
          <el-select
            v-model="criteria.status"
            placeholder="Select Status"
            class="w-150px ms-2"
            @change="fecthData"
            :disabled="isLoading"
          >
            <el-option
              v-for="status in [null, true, false]"
              :key="status"
              :label="status === null ? 'All' : status ? 'Active' : 'Inactive'"
              :value="status"
            ></el-option>
          </el-select>
        </div>
        <el-button
          type="success"
          plain
          @click="showAddCategory"
          :loading="isLoading"
          :disabled="isLoading"
          >{{ $t("action.add") }}</el-button
        >
      </div>
    </template>
    <table class="table table-tenant-sm table-hover">
      <thead>
        <tr>
          <th>{{ $t("fields.status") }}</th>
          <th>{{ $t("fields.key") }}</th>
          <th class="">{{ $t("fields.language") }}</th>
          <th>{{ $t("action.action") }}</th>
        </tr>
      </thead>
      <tbody v-if="isLoading">
        <LoadingRing />
      </tbody>
      <tbody v-else-if="!isLoading && !hasData">
        <NoDataBox />
      </tbody>
      <tbody v-else>
        <tr v-for="(item, index) in data" :key="index">
          <td>
            <el-switch
              v-model="item.status"
              active-color="#67c23a"
              inactive-color="#f56c6c"
              :loading="item.changeStatusLoading"
              @click="changeStatus(index, item)"
            ></el-switch>
          </td>
          <td>{{ index }}</td>
          <td>
            <span v-for="(i, index) in LanguageTypes.all" :key="index">
              <span
                class="badge text-bg-warning me-1"
                v-if="Object.keys(item.data).includes(i.code)"
                >{{ i.code }}</span
              >
              <span v-else class="badge text-bg-secondary me-1">{{
                i.code
              }}</span>
            </span>
          </td>
          <td>
            <el-button
              type="warning"
              plain
              @click="showEditCategory(index, item.data)"
              :loading="isLoading"
              :disabled="isLoading"
              >{{ $t("action.edit") }}</el-button
            >
          </td>
        </tr>
      </tbody>
    </table>
  </el-card>

  <AddCategory ref="addCategoryRef" @submit="fecthData" />
  <EditCategory ref="editCategoryRef" @submit="fecthData" />
</template>
<script setup lang="ts">
import { ref, onMounted, computed } from "vue";
import AddCategory from "../components/shop/ShopCategory/AddCategory.vue";
import EditCategory from "../components/shop/ShopCategory/EditCategory.vue";
import EventsServices from "../services/EventsServices";
import notification from "@/core/plugins/notification";
import { LanguageTypes } from "@/core/types/LanguageTypes";
const isLoading = ref(false);
const criteria = ref({
  status: null,
});

const data = ref<any>({});
const keys = computed(() => {
  return Object.keys(data.value);
});

const hasData = computed(() => {
  return data.value && Object.keys(data.value).length > 0;
});
const addCategoryRef = ref<InstanceType<typeof AddCategory> | null>(null);
const editCategoryRef = ref<InstanceType<typeof EditCategory> | null>(null);

const fecthData = async () => {
  isLoading.value = true;
  try {
    const response = await EventsServices.queryShopCategoryList(criteria.value);
    data.value = response;
    Object.values(data.value).forEach((item: any) => {
      item.changeStatusLoading = false;
    });
    console.log("fetched data:", data.value);
  } catch (error) {
    console.error("Error fetching data:", error);
  } finally {
    isLoading.value = false;
  }
};

const changeStatus = async (index: any, item: any) => {
  item.changeStatusLoading = true;
  try {
    await EventsServices.updateShopCategoryStatus(index);
    notification.success();
  } catch (error) {
    console.error("Error updating status:", error);
    notification.danger();
    item.status = !item.status; // Revert status change on error
  } finally {
    item.changeStatusLoading = false;
  }
};

const showAddCategory = () => {
  if (addCategoryRef.value) {
    addCategoryRef.value.show(keys.value);
  }
};

const showEditCategory = (key: any, languageData: any) => {
  if (editCategoryRef.value) {
    editCategoryRef.value.show(key, languageData);
  }
};

onMounted(() => {
  fecthData();
});
</script>
