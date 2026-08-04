<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <el-select
          v-model="criteria.status"
          clearable
          :placeholder="$t('fields.status')"
          class="w-200px me-3"
          @change="fetchData(1)"
        >
          <el-option
            :label="$t('status.awaitingReview')"
            :value="VerificationStatusTypes.AwaitingReview"
          />
          <el-option
            :label="$t('status.approved')"
            :value="VerificationStatusTypes.Approved"
          />
          <el-option
            :label="$t('status.rejected')"
            :value="VerificationStatusTypes.Rejected"
          />
        </el-select>
        <el-input
          class="w-300px"
          v-model="criteria.email"
          :clearable="true"
          placeholder="Email"
        >
          <template #append>
            <el-button :icon="Search" @click="fetchData(1)" />
          </template>
        </el-input>
        <el-button class="ms-4" type="info" plain @click="reset">
          {{ $t("action.reset") }}
        </el-button>
      </div>
    </div>
    <div class="card-body py-4">
      <table class="table align-middle table-row-dashed fs-6 gy-5">
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th>{{ $t("fields.client") }}</th>
            <th>{{ $t("fields.status") }}</th>
            <th>{{ $t("fields.updatedOn") }}</th>
            <th>{{ $t("fields.createdOn") }}</th>
            <th class="text-center min-w-150px">{{ $t("action.action") }}</th>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && items.length === 0">
          <NoDataBox />
        </tbody>
        <tbody v-else class="fw-semibold text-gray-900">
          <tr v-for="(item, index) in items" :key="index">
            <td class="d-flex align-items-center">
              <UserInfo :user="item.user" class="me-2" />
            </td>
            <td>{{ $t(`status.${VerificationStatus[item.status]}`) }}</td>
            <td><TimeShow :date-iso-string="item.updatedOn" /></td>
            <td><TimeShow :date-iso-string="item.createdOn" /></td>
            <td class="text-center">
              <button
                class="btn btn-light btn-primary btn-sm"
                @click="detailModalRef?.show(item.id)"
              >
                {{ $t("action.view") }}
              </button>
            </td>
          </tr>
        </tbody>
      </table>

      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
  </div>

  <MaterialRequestDetailModal
    ref="detailModalRef"
    @updated="fetchData(criteria.page)"
  />
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import TableFooter from "@/components/TableFooter.vue";
import TimeShow from "@/components/TimeShow.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import MaterialRequestService from "../services/MaterialRequestService";
import MaterialRequestDetailModal from "../components/materialRequest/MaterialRequestDetailModal.vue";
import {
  VerificationStatusTypes,
  VerificationStatus,
} from "@/core/types/VerificationInfos";
import { Search } from "@element-plus/icons-vue";

const isLoading = ref(true);
const items = ref(Array<any>());
const detailModalRef = ref<InstanceType<typeof MaterialRequestDetailModal>>();

const criteria = ref<any>({
  page: 1,
  size: 20,
  sortField: "updatedOn",
});

const fetchData = async (selectedPage: number) => {
  criteria.value.page = selectedPage;
  isLoading.value = true;
  try {
    const res = await MaterialRequestService.queryMaterialRequests(
      criteria.value
    );
    items.value = res.data;
    criteria.value = res.criteria;
  } catch (error) {
    console.error(error);
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

const reset = () => {
  criteria.value.email = null;
  criteria.value.status = null;
  fetchData(1);
};

onMounted(() => {
  fetchData(1);
});
</script>
