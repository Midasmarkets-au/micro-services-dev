<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <el-select
          v-model="criteria.type"
          :placeholder="$t('fields.type')"
          :disabled="isLoading"
          clearable
          @change="fetchData(1)"
        >
          <el-option
            v-for="item in commentTypeOptions"
            :key="item.value"
            :label="item.label"
            :value="item.value"
          />
        </el-select>
        <el-input
          v-model="criteria.partyId"
          clearable
          placeholder="Party ID"
          :disabled="isLoading"
          class="ms-4 input-with-select"
          @keyup.enter="fetchData(1)"
        >
          <template #append>
            <el-button
              :icon="Search"
              @click="fetchData(1)"
              :disabled="isLoading"
            />
          </template>
        </el-input>
        <el-button @click="reset()" :disabled="isLoading" class="ms-4">
          {{ $t("action.clear") }}
        </el-button>
      </div>
    </div>
    <div class="card-body">
      <table class="table table-tenant">
        <thead>
          <tr>
            <th>{{ $t("fields.id") }}</th>
            <th>Party ID</th>
            <th>Row ID</th>
            <th>{{ $t("fields.type") }}</th>
            <th>{{ $t("fields.content") }}</th>
            <th>{{ $t("fields.createdOn") }}</th>
          </tr>
        </thead>
        <tbody v-if="isLoading" style="height: 300px">
          <tr>
            <td colspan="12">
              <scale-loader :color="'#ffc730'"></scale-loader>
            </td>
          </tr>
        </tbody>
        <tbody v-else-if="!isLoading && data.length == 0">
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr v-for="item in data" :key="item.id">
            <td>{{ item.id }}</td>
            <td>{{ item.partyId }}</td>
            <td>{{ item.rowId }}</td>
            <td>{{ CommentType[item.type] }}</td>
            <td>
              {{ item.content }}
            </td>
            <td>
              <TimeShow type="GMToneLiner" :date-iso-string="item.createdOn" />
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";
import ReportService from "../services/ReportService";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import { CommentType, commentTypeOptions } from "@/core/types/CommentType";
import { Search } from "@element-plus/icons-vue";
const isLoading = ref(false);
const data = ref<any>({});

const criteria = ref<any>({
  page: 1,
  size: 30,
});

const fetchData = async (_page: number) => {
  isLoading.value = true;
  console.log(commentTypeOptions.value);
  criteria.value.page = _page;
  try {
    const res = await ReportService.queryComments(criteria.value);
    data.value = res.data;
    criteria.value = res.criteria;
  } catch (error) {
    console.error(error);
  } finally {
    isLoading.value = false;
  }
};

const reset = async () => {
  criteria.value = {
    page: 1,
    size: 30,
  };
  await fetchData(1);
};

onMounted(() => {
  fetchData(1);
});
</script>
<style scoped>
:deep .input-with-select .el-input-group__append {
  background-color: var(--el-fill-color-blank) !important;
}
</style>
