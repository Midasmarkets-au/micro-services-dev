<template>
  <el-dialog v-model="modalRef" width="800" align-center class="rounded">
    <template #header>
      <div class="d-flex gap-5 align-items-center mb-2">
        <el-tag type="info">
          {{ $t("fields.totalAccounts") }}:
          <span>{{ totalData.result.TotalAccounts }}</span>
        </el-tag>
        <el-tag>
          {{ $t("fields.accountsInSystem") }}:
          <span>{{ totalData.result.AccountsInOurSystem }}</span>
        </el-tag>
        <el-tag type="success">
          {{ $t("fields.totalSuccess") }}:
          <span>{{ totalData.result.SuccessCount }}</span>
        </el-tag>
        <el-tag type="danger">
          {{ $t("fields.totalFailed") }}:
          <span>{{ totalData.result.FailedRecords?.length }}</span>
        </el-tag>
        <el-tag type="warning">
          {{ $t("title.totalAmount") }}:
          <BalanceShow :balance="totalData.result.TotalAmount" />
        </el-tag>
      </div>
      <div>
        <el-checkbox-group v-model="criteria.statuses">
          <el-checkbox
            v-for="item in adjustRecordStatusOptions"
            :key="item.value"
            :label="item.value"
            :name="item.label"
          >
            {{ item.label }}
          </el-checkbox>
        </el-checkbox-group>
      </div>
    </template>
    <table class="table table-row-dashed">
      <thead>
        <tr class="text-muted fw-bold text-uppercase">
          <th>{{ $t("fields.accountNumber") }}</th>
          <th>{{ $t("fields.type") }}</th>
          <th>{{ $t("fields.status") }}</th>
          <th>{{ $t("fields.amount") }}</th>
          <th>{{ $t("fields.note") }}</th>
          <th>{{ $t("fields.operatedBy") }}</th>
          <th>{{ $t("fields.createdOn") }}</th>
        </tr>
      </thead>
      <tbody v-if="isLoading">
        <LoadingRing />
      </tbody>
      <tbody v-else-if="!isLoading && data.length === 0">
        <NoDataBox />
      </tbody>
      <tbody v-else>
        <tr v-for="(item, index) in data" :key="index">
          <td>{{ item.accountNumber }}</td>
          <td>
            {{ creditAdjustTypes[item.type] }}
          </td>
          <td>
            <el-tag size="small" :type="getType(item.status)">{{
              $t(`status.${AdjustRecordStatusTypes[item.status]}`)
            }}</el-tag>
          </td>
          <td>{{ item.amount / 100 }}</td>
          <td class="comment">
            <el-popover width="300" :content="item.comment" placement="top"
              ><template #reference
                ><div class="comment">{{ item.comment }}</div></template
              ></el-popover
            >
          </td>
          <td>{{ item.operatorName }}</td>
          <td>
            <TimeShow type="oneLiner" :date-iso-string="item.createdOn" />
          </td>
        </tr>
      </tbody>
    </table>
    <TableFooter @page-change="fetchData" :criteria="criteria" />
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="modalRef = false">{{
          $t("action.close")
        }}</el-button>
        <el-button type="primary" v-if="status" @click="confirmBatch()">
          {{ $t("action.confirmToProcess") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script setup lang="ts">
import { ref } from "vue";
import PaymentService from "@/projects/tenant/modules/Payment/services/PaymentService";
import {
  creditAdjustTypes,
  adjustRecordStatusOptions,
  AdjustRecordStatusTypes,
} from "@/core/types/CreditAdjustTypes";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { watch } from "vue";
const modalRef = ref(false);
const isLoading = ref(false);
const BatchId = ref(0);
const totalData = ref<any>({});
const status = ref(false);
const data = ref<any>({});
const criteria = ref({
  adjustBatchId: BatchId.value,
  size: 10,
  page: 1,
  statuses: [],
});
const show = (item: any) => {
  modalRef.value = true;
  totalData.value = item;
  BatchId.value = item.id;
  if (item.status === AdjustRecordStatusTypes.Created) {
    status.value = true;
  } else {
    status.value = false;
  }
  criteria.value.adjustBatchId = BatchId.value;
  fetchData(1);
};
watch(
  () => criteria.value.statuses,
  (value) => {
    criteria.value.statuses = value;
    fetchData(1);
  }
);
const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  const tempStatus = criteria.value.statuses;
  try {
    const response = await PaymentService.getBatchDetail(criteria.value);
    data.value = response.data;
    criteria.value = response.criteria;
    criteria.value.statuses = tempStatus;
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};
const getType = (status: number) => {
  if (status === AdjustRecordStatusTypes.Completed) {
    return "success";
  } else if (status === AdjustRecordStatusTypes.Failed) {
    return "danger";
  } else if (status === AdjustRecordStatusTypes.Processing) {
    return "warning";
  } else {
    return "primary";
  }
};
const confirmBatch = async () => {
  try {
    const response = await PaymentService.confirmBatch(BatchId.value);
    MsgPrompt.success(response.message);
    modalRef.value = false;
  } catch (error) {
    MsgPrompt.error(error);
  }
};
defineExpose({
  show,
});
</script>
<style scoped lang="scss">
.comment {
  max-width: 150px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>
