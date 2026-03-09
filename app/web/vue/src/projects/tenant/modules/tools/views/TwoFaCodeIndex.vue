<template>
  <div class="card">
    <div class="card-header">
      <div class="d-flex align-items-center">
        <el-input
          v-model="criteria.email"
          class="w-300px"
          placeholder="Email"
          :disabled="isLoading"
          clearable
        />
        <el-input
          v-model="criteria.code"
          class="ms-3 w-100px"
          placeholder="Code"
          :disabled="isLoading"
          clearable
        />
        <el-input
          v-model="criteria.partyId"
          class="ms-3 w-100px"
          placeholder="Party ID"
          :disabled="isLoading"
          clearable
        />
        <el-button
          type="warning"
          :disabled="isLoading"
          class="ms-3"
          plain
          @click="fetchData(1)"
        >
          {{ $t("action.search") }}
        </el-button>
        <el-button :disabled="isLoading" @click="reset">
          {{ $t("action.reset") }}
        </el-button>
      </div>
    </div>
    <div class="card-body">
      <table class="table align-middle table-row-dashed fs-6 gy-5">
        <thead>
          <tr>
            <th>ID</th>
            <th>{{ $t("fields.status") }}</th>
            <th>{{ $t("fields.code") }}</th>
            <th>{{ $t("fields.event") }}</th>
            <th>{{ $t("fields.user") }}</th>
            <th>{{ $t("fields.createdOn") }}</th>
            <th>{{ $t("fields.expireDate") }}</th>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && detail?.length === 0">
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr v-for="(item, index) in detail" :key="index">
            <td>{{ item.id }}</td>
            <td>
              <el-tag v-if="item.status == 1" type="warning">
                {{ $t("status.used") }}
              </el-tag>
              <el-tag v-else-if="item.expireOn < currentTime" type="info">
                {{ $t("fields.expired") }}
              </el-tag>
              <el-tag v-else-if="item.status == 0" type="success">
                {{ $t("fields.valid") }}
              </el-tag>
            </td>
            <td>{{ item.code }}</td>
            <td>{{ item.event }}</td>
            <td>
              <UserInfo :user="item.user" class="me-2" />
            </td>
            <td>
              <TimeShow :date-iso-string="item.createdOn" type="GMToneLiner" />
            </td>
            <td>
              <TimeShow :date-iso-string="item.expireOn" type="GMToneLiner" />
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, computed } from "vue";
import ToolServices from "../services/ToolServices";
import TableFooter from "@/components/TableFooter.vue";

const isLoading = ref(false);
const detail = ref(Array<any>());
const criteria = ref<any>({
  page: 1,
  size: 25,
});
const currentTime = computed(() => new Date().toISOString());
const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const data = await ToolServices.getTwoFaCode(criteria.value);
    criteria.value = data.criteria;
    detail.value = data.data;
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const reset = () => {
  criteria.value = {
    page: 1,
    size: 25,
  };
  fetchData(1);
};

onMounted(() => {
  fetchData(1);
});
</script>
