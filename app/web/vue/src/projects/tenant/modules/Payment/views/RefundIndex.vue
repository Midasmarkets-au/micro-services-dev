<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <div class="d-flex align-items-end gap-4">
          <div>
            <el-input
              v-model="criteria.targetId"
              :placeholder="t('fields.walletId')"
            >
            </el-input>
          </div>
          <div>
            <el-input
              v-model="criteria.email"
              class="w-250px ms-2"
              :placeholder="$t('fields.email')"
              clearable
            >
            </el-input>
          </div>
          <div>
            <el-button @click="fetchData(1)" type="primary">
              {{ $t("action.search") }}
            </el-button>
          </div>
          <div>
            <el-button @click="reset"> {{ $t("action.reset") }} </el-button>
          </div>
        </div>
      </div>
      <div
        v-if="$can('TenantAdmin') || $can('SuperAdmin')"
        class="card-toolbar"
      >
        <el-button type="success" @click="showCreateRefund()">
          {{ $t("action.createRefund") }}</el-button
        >
      </div>
    </div>
    <div class="card-body">
      <table class="table align-middle table-row-dashed fs-6 gy-5">
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th>{{ $t("fields.client") }}</th>
            <th>{{ $t("fields.walletId") }}</th>
            <th class="">{{ $t("fields.currency") }}</th>
            <th>{{ $t("fields.amount") }}</th>
            <th>{{ $t("fields.createdOn") }}</th>
            <th>{{ $t("fields.comment") }}</th>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && data.length == 0">
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr v-for="(item, index) in data" :key="index">
            <td class="d-flex align-items-center">
              <UserInfo v-if="item.user" :user="item.user" class="me-2" />
            </td>
            <td>
              <div class="d-flex align-items-center gap-2">
                <div>{{ item.targetId }}</div>
                <el-button
                  :icon="Document"
                  circle
                  @click="historyRef.show(item.id)"
                />
              </div>
            </td>
            <td>
              {{ $t(`type.currency.${item.currencyId}`) }}
            </td>
            <td>
              <BalanceShow
                :currency-id="item.currencyId"
                :balance="item.amount"
              />
            </td>
            <td>
              <TimeShow :date-iso-string="item.createdOn" />
            </td>
            <td>{{ item.comment }}</td>
          </tr>
        </tbody>
      </table>
    </div>
    <TableFooter @page-change="fetchData" :criteria="criteria" />
  </div>
  <CreateRefund ref="createRefundRef" @event-Submit="fetchData(1)" />
  <HistoryRecord ref="historyRef" />
</template>

<script lang="ts" setup>
import { ref, onMounted } from "vue";
import PaymentService from "../services/PaymentService";
import CreateRefund from "../components/modal/CreateRefund.vue";
import { useI18n } from "vue-i18n";
import { Document } from "@element-plus/icons-vue";
import HistoryRecord from "../components/modal/HistoryRecord.vue";

const historyRef = ref<any>(null);
const t = useI18n().t;
const isLoading = ref(false);
const data = ref<any>([]);
const createRefundRef = ref<InstanceType<typeof CreateRefund>>();

const criteria = ref<any>({
  page: 1,
  size: 20,
  targetId: null,
  targetType: 1,
});
const fetchData = async (_page: number) => {
  isLoading.value = true;
  try {
    const res = await PaymentService.queryRefunds(criteria.value);
    criteria.value = res.criteria;
    data.value = res.data;
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const showCreateRefund = () => {
  createRefundRef.value?.show();
};

const reset = async () => {
  criteria.value = {
    page: 1,
    size: 20,
    targetType: 1,
    targetId: null,
  };
  fetchData(1);
};

onMounted(() => {
  fetchData(1);
});
</script>
