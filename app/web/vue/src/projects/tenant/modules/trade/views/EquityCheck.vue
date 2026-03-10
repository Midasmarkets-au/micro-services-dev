<template>
  <div class="card mb-5 mb-xl-8">
    <div class="card-header">
      <div class="card-title">
        <h2 class="card-label">Equity Check</h2>
        <el-input
          class="w-200px"
          v-model="criteria.accountNumber"
          :clearable="true"
          placeholder="Account Number"
        >
        </el-input>
        <el-input
          class="w-200px ms-3"
          v-model="criteria.email"
          :clearable="true"
          placeholder="Email"
        >
        </el-input>
        <el-button class="ms-3" type="success" @click="fetchData(1)"
          >Search</el-button
        >
        <el-button class="ms-3" type="info" @click="clear">Clear</el-button>
      </div>
      <div class="card-toolbar gap-4">
        <el-button
          type="primary"
          @click="showExcludeList"
          :disabled="isReloading"
          >Excluded List</el-button
        >
        <el-button type="warning" @click="reload">Reload</el-button>
      </div>
    </div>

    <div class="card-body">
      <table class="table align-middle table-row-dashed fs-6 gy-3 table-hover">
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th>{{ $t("fields.user") }}</th>
            <th>{{ $t("title.service") }}</th>
            <th>{{ $t("fields.group") }}</th>
            <th>{{ $t("fields.accountNumber") }}</th>
            <th>{{ $t("fields.margin") }}</th>
            <th>{{ $t("fields.credit") }}</th>
            <th>{{ $t("fields.equity") }}</th>
            <th>{{ $t("fields.balance") }}</th>
            <th>DIFF</th>
            <th>{{ $t("fields.action") }}</th>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && checklist.length === 0">
          <NoDataBox />
        </tbody>
        <tbody v-else class="fw-semibold text-gray-900">
          <tr v-for="(item, index) in checklist" :key="index">
            <td>
              <div>
                <div>{{ item.nativeName }}</div>
                <div>{{ item.email }}</div>
              </div>
            </td>
            <td>
              <div>
                <div>{{ item.serviceName }}</div>
                <div>{{ item.mtGroup }}</div>
              </div>
            </td>
            <td>{{ item.agentGroup }}</td>
            <td>{{ item.accountNumber }}</td>
            <td>{{ item.margin }}</td>
            <td>{{ item.credit }}</td>
            <td>{{ item.equity }}</td>
            <td>{{ item.balance }}</td>
            <td>{{ (item.credit - item.equity).toFixed(2) }}</td>
            <td>
              <el-button
                type="danger"
                size="small"
                @click="showEmailTargetSelection(item)"
                >{{ $t("action.sendEmail") }}</el-button
              >
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
    <ExcludeList ref="excludeListRef" @event-submit="onEventSubmitted" />
    <EmailTargetSelection ref="emailTargetSelectionRef" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import TradeServices from "../services/TradeServices";
import ExcludeList from "@/projects/tenant/modules/trade/components/ExcludeList.vue";
import EmailTargetSelection from "@/projects/tenant/modules/trade/modals/EmailTargetSelection.vue";
import SystemService from "@/projects/tenant/modules/system/services/SystemService";
import TableFooter from "@/components/TableFooter.vue";

const isLoading = ref(false);
const checklist = ref(Array<any>());
const excludeListRef = ref<any>(null);
const emailTargetSelectionRef = ref<any>(null);
const isReloading = ref(false);

const criteria = ref<any>({
  page: 1,
  size: 20,
  numPages: 1,
  total: 0,
});

const showExcludeList = () => {
  excludeListRef.value?.show();
};

const showEmailTargetSelection = (_item: any) => {
  emailTargetSelectionRef.value?.show(_item);
};

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;

  try {
    const res = await TradeServices.getEquityBelowCredit(criteria.value);
    checklist.value = res.data;
    criteria.value = res.criteria;
  } catch (error) {
    console.log(error);
  }

  isLoading.value = false;
};

const reload = () => {
  isReloading.value = true;
  SystemService.reloadConfiguration().then(() => {
    isReloading.value = false;
  });
};

const clear = () => {
  criteria.value = {
    page: 1,
    size: 20,
    numPages: 1,
    total: 0,
  };

  fetchData(1);
};

const onEventSubmitted = () => {
  fetchData(1);
};

onMounted(() => {
  fetchData(1);
});
</script>
