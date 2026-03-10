<template>
  <el-tabs v-model="tab" @tab-change="changeTab()" class="demo-tabs">
    <el-tab-pane
      label="MT4 Real"
      :name="10"
      :disabled="isSubmitting || isLoading"
    ></el-tab-pane>
    <el-tab-pane
      label="MT5 Real"
      :name="30"
      :disabled="isSubmitting || isLoading"
    ></el-tab-pane>
    <el-tab-pane
      label="MT4 Demo"
      :name="11"
      :disabled="isSubmitting || isLoading"
    ></el-tab-pane>
    <el-tab-pane
      label="MT5 Demo"
      :name="31"
      :disabled="isSubmitting || isLoading"
    ></el-tab-pane>
  </el-tabs>
  <div class="card">
    <div class="card-body">
      <div
        v-if="isLoading"
        style="height: 300px"
        class="d-flex align-items-center justify-content-center"
      >
        <el-row>
          <el-col colspan="24">
            <scale-loader :color="'#ffc730'"></scale-loader>
          </el-col>
        </el-row>
      </div>
      <div v-else>
        <el-row class="mb-6">
          <el-col :span="6">
            <el-col :span="12">
              <el-input
                v-model="searchTerm"
                :placeholder="$t('tip.searchKeyWords')"
                prefix-icon="el-icon-search"
                clearable
                @clear="searchTerm = ''"
                :suffix-icon="Search"
                :disabled="isSubmitting"
                @keyup.escape="searchTerm = ''"
              />
            </el-col>
          </el-col>

          <el-col :span="3">
            <el-input
              v-model="newGroup"
              :placeholder="$t('fields.addGroup')"
              prefix-icon="el-icon-search"
              clearable
              @clear="newGroup = ''"
              :disabled="isSubmitting"
              @keyup.enter="addGroup"
              @keyup.escape="newGroup = ''"
            ></el-input>
          </el-col>
          <el-col :span="3">
            <el-button
              type="success"
              class="ms-3"
              plain
              @click="addGroup"
              :disabled="isSubmitting"
              >{{ $t("action.add") }}</el-button
            >
          </el-col>
        </el-row>
        <el-row>
          <el-col
            :span="4"
            v-for="group in filteredData"
            :key="group"
            :class="['group', { 'bg-success': tempAddedGroup.includes(group) }]"
          >
            <div class="d-flex justify-content-between align-items-center">
              <div>{{ group }}</div>
              <div>
                <el-popconfirm
                  :confirm-button-text="$t('action.yes')"
                  :cancel-button-text="$t('action.no')"
                  :icon="InfoFilled"
                  icon-color="#ff4949"
                  :title="$t('tip.confirmPrompt')"
                  @confirm="deleteGroup(group)"
                  width="220"
                >
                  <template #reference>
                    <el-button
                      type="danger"
                      plain
                      size="small"
                      :icon="Delete"
                      circle
                      :disabled="isSubmitting"
                    />
                  </template>
                </el-popconfirm>
              </div>
            </div>
          </el-col>
        </el-row>
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
import { onMounted, ref, computed } from "vue";
import UserService from "../../users/services/UserService";
import { Delete, Search, InfoFilled } from "@element-plus/icons-vue";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import { ElNotification } from "element-plus";
import i18n from "@/core/plugins/i18n";

const t = i18n.global.t;
const isLoading = ref(true);
const tab = ref(10);
const data = ref<any>([]);
const searchTerm = ref("");
const newGroup = ref("");
const isSubmitting = ref(false);

const tempAddedGroup = ref<any>([]);

const deleteGroup = async (group: any) => {
  isSubmitting.value = true;
  try {
    await UserService.deleteGroupById(tab.value, {
      group: group,
    });
    ElNotification({
      title: t("status.success"),
      message: t("tip.updateSuccess"),
      type: "success",
    });
    data.value = data.value.filter((item: any) => item !== group);
  } catch (e) {
    console.error(e);
    ElNotification({
      title: t("status.failed"),
      message: t("tip.updateFailed"),
      type: "error",
    });
  }
  isSubmitting.value = false;
};

const addGroup = async () => {
  if (newGroup.value.trim() === "") return;
  isSubmitting.value = true;
  try {
    await UserService.addGroupById(tab.value, {
      group: newGroup.value,
    });
    ElNotification({
      title: t("status.success"),
      message: t("tip.updateSuccess"),
      type: "success",
    });
    data.value.unshift(newGroup.value);
    tempAddedGroup.value.push(newGroup.value);
  } catch (e) {
    console.error(e);
    ElNotification({
      title: t("status.failed"),
      message: t("tip.updateFailed"),
      type: "error",
    });
  }
  isSubmitting.value = false;
};

const filteredData = computed(() => {
  return data.value.filter(
    (item: any) =>
      !searchTerm.value ||
      item.toLowerCase().includes(searchTerm.value.toLowerCase())
  );
});

const changeTab = async () => {
  await fetchData();
  tempAddedGroup.value = [];
  newGroup.value = "";
  searchTerm.value = "";
};

const fetchData = async () => {
  isLoading.value = true;
  try {
    const results = await UserService.getServiceById(tab.value);
    data.value = results;

    data.value = data.value.sort((a: any, b: any) => a.localeCompare(b));
  } catch (e) {
    console.error(e);
  }
  isLoading.value = false;
};

onMounted(async () => {
  await fetchData();
});
</script>
<style scoped>
.group {
  padding: 10px;
  border: 1px solid #ccc;
}
:deep .el-tabs__item {
  font-size: 16px !important;
}
</style>
