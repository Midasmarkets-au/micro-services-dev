<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title"></div>
      <div class="card-toolbar">
        <el-button type="primary" @click="showCreate()">
          {{ $t("action.create") }}
        </el-button>
        <el-button type="success" @click="fetchData(1)">
          {{ $t("action.refresh") }}
        </el-button>
      </div>
    </div>
    <div class="card-body">
      <table
        class="table align-middle fs-6 gy-5"
        id="kt_ecommerce_add_product_reviews"
      >
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th>{{ $t("fields.batchId") }}</th>
            <th>{{ $t("fields.type") }}</th>
            <th>{{ $t("fields.status") }}</th>
            <th>{{ $t("fields.totalAccounts") }}</th>
            <th>{{ $t("fields.note") }}</th>
            <th>{{ $t("fields.operatedBy") }}</th>
            <th>{{ $t("fields.createdOn") }}</th>
            <th>{{ $t("action.action") }}</th>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && data.length === 0">
          <NoDataBox />
        </tbody>
        <tbody v-else class="fw-semibold text-gray-900">
          <tr v-for="(item, index) in data" :key="index">
            <td>{{ item.id }}</td>
            <td>
              {{ creditAdjustTypes[item.type] }}
            </td>
            <td>
              <el-tag size="small" :type="getType(item.status)">
                {{ $t(`status.${BacthStatus[item.status]}`) }}</el-tag
              >
            </td>
            <td>{{ item.totalAccounts }}</td>
            <td>
              <el-popover width="300" :content="item.note" placement="top"
                ><template #reference
                  ><div class="comment">{{ item.note }}</div></template
                ></el-popover
              >
            </td>
            <td>{{ item.operatorName }}</td>
            <td>
              <TimeShow type="inFields" :date-iso-string="item.createdOn" />
            </td>
            <td>
              <el-button type="primary" @click="showDetail(item)">
                {{ $t("action.detail") }}
              </el-button>
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
  </div>
  <CreateBatch ref="createRef" @file-uploaded="fetchData(1)" />
  <BatchDetail ref="detailRef" />
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";
import CreateBatch from "./CreateBatch.vue";
import BatchDetail from "./BatchDetail.vue";
import { BacthStatus, creditAdjustTypes } from "@/core/types/CreditAdjustTypes";
import PaymentService from "../../services/PaymentService";
const isLoading = ref(false);
const createRef = ref<InstanceType<typeof CreateBatch>>();
const detailRef = ref<InstanceType<typeof BatchDetail>>();
const data = ref(<any>[]);

const criteria = ref({
  page: 1,
  size: 20,
  status: "",
  operator: "",
  createdOn: "",
});
const showCreate = () => {
  createRef.value.show();
};

const showDetail = (item: any) => {
  detailRef.value.show(item);
};

const getType = (status: number) => {
  if (status === BacthStatus.completed) {
    return "success";
  } else if (status === BacthStatus.failed) {
    return "danger";
  } else if (status === BacthStatus.processing) {
    return "warning";
  } else if (status === BacthStatus.pending) {
    return "info";
  } else {
    return "";
  }
};
const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const response = await PaymentService.getBatchList(criteria.value);
    data.value = response.data;
    criteria.value = response.criteria;
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
  }
};

onMounted(() => {
  fetchData(1);
});
</script>
<style scoped lang="scss">
.comment {
  max-width: 150px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.box-item {
  width: 300px !important;
}
</style>
