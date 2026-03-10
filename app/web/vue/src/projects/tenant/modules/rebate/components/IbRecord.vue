<template>
  <div class="card mb-5 mb-xl-8">
    <div class="card-header">
      <div class="card-title">
        <el-select :disabled="isLoading" v-model="searchType" class="w-175px">
          <el-option :value="0" label="Account Number" />
          <el-option :value="1" label="Account Uid" />
          <el-option :value="2" label="Email" />
          <el-option :value="3" label="Ticket Number" />
        </el-select>
        <el-input
          :disabled="isLoading"
          v-model="searchValue"
          class="ms-3 w-250px"
          placeholder="Search"
          clearable
        />
        <el-button
          type="primary"
          class="ms-3"
          @click="search"
          :loading="isLoading"
          >{{ $t("action.search") }}</el-button
        >
        <el-button type="primary" @click="reset" :disabled="isLoading">
          {{ $t("action.reset") }}
        </el-button>
      </div>
    </div>

    <div class="card-body">
      <table
        class="table align-middle table-row-dashed fs-6 gy-5"
        id="table_accounts_requests"
      >
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th class="">Receiver User</th>
            <th class="">Receiver Account</th>
            <th class="">{{ $t("fields.currency") }}</th>
            <th class="">Amount</th>
            <th class="">Ticket</th>
            <th class="">Symbol</th>
            <th class="">Volume</th>
            <!--            <th class="">CMD</th>-->
            <th class="">Date Time</th>
          </tr>
        </thead>

        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && rebateList.length === 0">
          <NoDataBox />
        </tbody>

        <tbody v-else class="fw-semibold text-gray-900">
          <tr v-for="(item, index) in rebateList" :key="index">
            <td class="d-flex align-items-center">
              <UserInfo
                v-if="item.targetAccount.user"
                :user="item.targetAccount.user"
                class="me-2"
              />
            </td>
            <td v-if="item.targetAccount.role == 100">
              [{{ item.targetAccount.code }}]{{ item.targetAccount.uid }}
            </td>
            <td v-else>
              [{{ item.targetAccount.group }}]{{ item.targetAccount.uid }}
            </td>
            <td>{{ t(`type.currency.${item.currencyId}`) }}</td>
            <td class="">
              <BalanceShow
                :currency-id="item.currencyId"
                :balance="item.amount"
              />
            </td>
            <td>{{ item.trade.ticket }}</td>
            <td>{{ item.trade.symbol }}</td>
            <td>{{ item.trade.volume }}</td>
            <!--            <td>{{ item.trade.cmd }}</td>-->
            <td>
              <TimeShow type="exactTime" :date-iso-string="item.postedOn" />
            </td>
          </tr>
        </tbody>
      </table>

      <TableFooter @page-change="pageChange" :criteria="criteria" />
    </div>
    <RebateDetail ref="RebateDetailRef" />
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, inject } from "vue";
import RebateService from "../services/RebateService";
import BalanceShow from "@/components/BalanceShow.vue";
import TimeShow from "@/components/TimeShow.vue";
import i18n from "@/core/plugins/i18n";
import TableFooter from "@/components/TableFooter.vue";
import RebateDetail from "./RebateDetail.vue";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import {
  TenantRebateCriteria,
  TenantRebateViewModel,
} from "@/core/models/Rebate";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import { TimeZoneService } from "@/core/plugins/TimerService";

const { t } = i18n.global;
const RebateDetailRef = ref<any>(null);
const isLoading = inject<any>("isLoading");
const rebateList = ref(Array<TenantRebateViewModel>());
const searchType = ref(0);
const searchValue = ref("");

const addDateRange = () => {
  const today = new Date();
  const from = new Date(today);
  const to = new Date(today);

  // Set the from date to one month prior to today
  from.setMonth(today.getMonth() - 1);

  // Format dates to 'YYYY-MM-DD' string format
  criteria.value.from = TimeZoneService.adjustDateToKeepsYearMonthDate(from);
  criteria.value.to = TimeZoneService.adjustDateToKeepsYearMonthDate(to);
};

const criteria = ref<TenantRebateCriteria>({
  page: 1,
  size: 10,
  sortField: "id",
} as TenantRebateCriteria);

addDateRange();

const search = async () => {
  if (searchValue.value === "") {
    return;
  }
  await resetCriteria();
  switch (searchType.value) {
    case 0:
      criteria.value.accountNumber = searchValue.value;
      break;
    case 1:
      criteria.value.accountUid = searchValue.value;
      break;
    case 2:
      criteria.value.email = searchValue.value;
      break;
    case 3:
      criteria.value.ticketNumber = searchValue.value;
      break;
  }
  await fetchData(1);
};

const reset = async () => {
  searchValue.value = "";

  await resetCriteria();
  await fetchData(1);
};

const fetchData = async (_page: number) => {
  criteria.value.page = _page;
  criteria.value.sortField = "id";
  try {
    isLoading.value = true;
    const res = await RebateService.queryRebates(criteria.value);
    rebateList.value = res.data;
    criteria.value = res.criteria;
  } catch (error) {
    MsgPrompt.error(error);
  }
  isLoading.value = false;
};

const resetCriteria = () => {
  criteria.value = {
    page: 1,
    size: 10,
    sortField: "id",
  } as TenantRebateCriteria;
  addDateRange();
};

const pageChange = (_page: number) => {
  fetchData(_page);
};

onMounted(async () => {
  await fetchData(1);
});
</script>

<style scoped>
.filterOption {
  padding: 8px;
  border-radius: 5px;
  background-color: #ffce00;
  width: fit-content;
}
.clear-all-filter {
  color: white;
  margin-top: 6px;
  border-radius: 10px;
  background-color: #569754;
  width: fit-content;
  padding-left: 5px;
  padding-right: 5px;
}
</style>
