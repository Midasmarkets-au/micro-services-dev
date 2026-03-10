<template>
  <div class="d-flex flex-column flex-column-fluid">
    <ul
      class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
    >
      <li class="nav-item">
        <a
          class="nav-link text-active-primary pb-4"
          :class="[
            { active: tab === tabStatus.Pending },
            { 'disabled opacity-50 pe-none': isLoading },
          ]"
          data-bs-toggle="tab"
          href="#"
          @click="(tab = tabStatus.Pending), fetchData(1, tabStatus.Pending)"
          >Pending</a
        >
      </li>

      <li class="nav-item">
        <a
          class="nav-link text-active-primary pb-4"
          :class="[
            { active: tab === tabStatus.Completed },
            { 'disabled opacity-50 pe-none': isLoading },
          ]"
          data-bs-toggle="tab"
          href="#"
          @click="
            (tab = tabStatus.Completed), fetchData(1, tabStatus.Completed)
          "
          >Complete</a
        >
      </li>

      <li class="nav-item">
        <a
          class="nav-link text-active-primary pb-4"
          :class="[
            { active: tab === tabStatus.Pause },
            { 'disabled opacity-50 pe-none': isLoading },
          ]"
          data-bs-toggle="tab"
          href="#"
          @click="(tab = tabStatus.Pause), fetchData(1, tabStatus.Pause)"
          >Pause</a
        >
      </li>

      <li class="nav-item">
        <a
          class="nav-link text-active-primary pb-4"
          :class="[
            { active: tab === tabStatus.All },
            { 'disabled opacity-50 pe-none': isLoading },
          ]"
          data-bs-toggle="tab"
          href="#"
          @click="(tab = tabStatus.All), fetchData(1, tabStatus.All)"
          >All (Export)</a
        >
      </li>
    </ul>
    <div class="card mb-5 mb-xl-8">
      <div class="card-header">
        <div
          class="card-title d-flex justify-content-between"
          style="width: 100%"
        >
          <div class="d-flex">
            <div class="me-5">
              <el-date-picker
                class="w-250px"
                v-model="period"
                type="daterange"
                :start-placeholder="$t('fields.startDate')"
                :end-placeholder="$t('fields.endDate')"
                :disabled="isLoading"
                format="YYYY-MM-DD"
                value-format="YYYY-MM-DD HH:mm:ss"
              />
            </div>
            <div class="me-5">
              <el-select
                v-if="tab == tabStatus.All"
                v-model="allPageStatus"
                @change="confirmSearch"
                class="w-150px"
                :placeholder="$t('fields.status')"
                clearable
                :disabled="isLoading"
              >
                <el-option value="0" label="Pending" />
                <el-option value="1" label="Complete" />
              </el-select>
            </div>
            <el-input
              v-model="criteria.salesAccountUid"
              placeholder="Sales UID"
              clearable
              :disabled="isLoading"
            >
            </el-input>

            <el-input
              class="ms-5"
              v-model="criteria.tradeAccountNumber"
              placeholder="Account Number"
              clearable
              :disabled="isLoading"
            >
            </el-input>

            <el-input
              class="ms-5"
              v-model="criteria.ticket"
              placeholder="Ticket Number"
              clearable
              :disabled="isLoading"
            >
            </el-input>

            <el-button
              class="ms-5"
              @click="fetchData(1, tab)"
              :disabled="isLoading"
              >Search</el-button
            >
            <el-button class="ms-5" @click="reset" :disabled="isLoading"
              >Reset</el-button
            >

            <el-button
              v-if="tab == tabStatus.All"
              type="success"
              @click="openSalesRebateExportModal"
              :disabled="isLoading"
              plain
              >{{ $t("action.export") }}
            </el-button>
          </div>
        </div>
      </div>

      <div class="card-body">
        <table
          class="table align-middle table-row-dashed fs-6 table-hover"
          id="table_accounts_requests"
        >
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th>Sales Uid</th>
              <th>Trade Account No.</th>
              <th>Rebate ID</th>
              <th>Ticket</th>
              <th>Rebate Type</th>
              <th>Rebate Base</th>
              <th>Wallet Adjust ID</th>
              <th>Wallet ID</th>
              <th>Amount</th>
              <th>Status</th>
              <th>CreatedOn</th>
              <th>Pause</th>
            </tr>
          </thead>

          <tbody v-if="isLoading" style="height: 300px">
            <tr>
              <td colspan="12"><scale-loader></scale-loader></td>
            </tr>
          </tbody>
          <tbody v-if="!isLoading && data.length === 0">
            <tr>
              <td colspan="12">{{ $t("tip.nodata") }}</td>
            </tr>
          </tbody>
          <TransitionGroup
            v-if="!isLoading && data.length != 0"
            tag="tbody"
            name="table-delete-fade"
            class="table-delete-fade-container text-gray-600 fw-semibold"
          >
            <tr
              v-for="item in data"
              :key="item"
              :class="{
                'account-select': item.id === accountSelected,
              }"
              @click="selectedAccount(item.id)"
            >
              <td>{{ item.salesAccountUid }}</td>
              <td>{{ item.tradeAccountNumber }}</td>
              <td>{{ item.tradeRebateId }}</td>
              <td>{{ item.ticket }}</td>
              <td>{{ item.rebateType }}</td>
              <td><BalanceShow :balance="item.rebateBase" /></td>
              <td>
                <span v-if="item.status == 0">--</span
                ><span v-if="item.status == 1">{{ item.walletAdjustId }}</span>
              </td>
              <td>
                <span v-if="item.status == 0">--</span
                ><span v-if="item.status == 1">{{ item.walletId }}</span>
              </td>
              <td>$ {{ item.amount / 10000 }}</td>
              <td>
                <el-tag
                  :type="getTagType(item.status)"
                  effect="dark"
                  class="fs-6"
                >
                  <span v-if="item.status == 0">Pending</span>
                  <span v-if="item.status == 1">Complete</span>
                  <span v-if="item.status == 6">Pause</span>
                </el-tag>
              </td>
              <!-- <td @click="showSchemaNote(item)">
                <i
                  class="fa-regular fa-comment-dots"
                  :class="{ 'text-secondary': item.note == null }"
                ></i>
              </td> -->
              <td><TimeShow :date-iso-string="item.createdOn" /></td>
              <td v-if="item.status == 0 || item.status == 6">
                <el-switch
                  v-model="item.status"
                  :active-value="6"
                  :inactive-value="0"
                  @change="toggleActiveStatus(item)"
                  :disabled="isSubmitting"
                ></el-switch>
              </td>
            </tr>
          </TransitionGroup>
        </table>
        <TableFooter @page-change="pageChange" :criteria="criteria" />
      </div>
      <SchemaDetail ref="SchemaDetailRef" @refresh="refresh" />
      <SchemaNote ref="SchemaNoteRef" />
      <SalesRebateExportModal
        ref="salesRebateExportModalRef"
      ></SalesRebateExportModal>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, watch } from "vue";
import TimeShow from "@/components/TimeShow.vue";
import RebateService from "../services/RebateService";
import SchemaDetail from "../components/SalesRebateSchemaDetail.vue";
import SchemaNote from "../components/SalesRebateSchemaNote.vue";
import TableFooter from "@/components/TableFooter.vue";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import BalanceShow from "@/components/BalanceShow.vue";
import SalesRebateExportModal from "../components/modal/SalesRebateExportModal.vue";
import { convertToUTC } from "@/core/utils/DateUtils";
// import { isDateInDST_US } from "@/core/plugins/TimerService";
// import { useI18n } from "vue-i18n";
// import moment from "moment";

const isLoading = ref(true);
const SchemaNoteRef = ref<any>(null);
const SchemaDetailRef = ref<any>(null);
const data = ref({} as any);
const accountSelected = ref(0);
const period = ref([] as any);
const isSubmitting = ref(false);

const tabStatus = ref({
  Pending: 0,
  Completed: 1,
  Pause: 6,
  All: null,
});

const allPageStatus = ref(tabStatus.value.All) as any;
const tab = ref(tabStatus.value.Pending) as any;
const salesRebateExportModalRef = ref<any>(null);
const criteria = ref<any>({
  page: 1,
  size: 20,
  salesAccountUid: "",
  tradeAccountNumber: "",
  ticket: "",
});

const reset = () => {
  criteria.value.page = 1;
  criteria.value.size = 20;
  criteria.value.salesAccountUid = "";
  criteria.value.tradeAccountNumber = "";
  criteria.value.ticket = "";
  criteria.value.from = null;
  criteria.value.to = null;
  period.value = [];
  allPageStatus.value = tabStatus.value.All;
  fetchData(1, tab.value);
};

onMounted(async () => {
  fetchData(1, tabStatus.value.Pending);
});

const refresh = () => {
  fetchData(1, tabStatus.value.Pending);
};

const getTagType = (status: number) => {
  switch (status) {
    case 1:
      return "success";
    case 0:
      return "warning";
    case 6:
      return "info";
    default:
      return "primary";
  }
};

const fetchData = async (_page: number, status: any) => {
  isLoading.value = true;
  criteria.value.page = _page;
  const datesRange = convertToUTC(period.value);
  criteria.value.from = datesRange.from;
  criteria.value.to = datesRange.to;
  if (allPageStatus.value !== tabStatus.value.All) {
    criteria.value.status = allPageStatus.value;
  } else {
    criteria.value.status = status;
  }

  try {
    const res = await RebateService.querySalesRebate(criteria.value);
    criteria.value = res.criteria;
    data.value = res.data;
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
  }
};

const selectedAccount = (id: number) => {
  accountSelected.value = id;
};

const pageChange = (_page: number) => {
  fetchData(_page, tab.value);
};

const confirmSearch = () => {
  fetchData(1, allPageStatus.value);
};

const showSchemaNote = (_item: any) => {
  SchemaNoteRef.value.show(_item);
};

const toggleActiveStatus = async (item: any) => {
  console.log(status);
  isSubmitting.value = true;

  try {
    const res = await RebateService.updateSalesRebateActiveStatus(item.id, {
      status: item.status,
    });
  } catch (error) {
    console.log(error);
  } finally {
    isSubmitting.value = false;
  }
};

const openSalesRebateExportModal = () => {
  const currentQuery = {
    from: criteria.value.from,
    to: criteria.value.to,
    salesAccountUid: criteria.value.salesAccountUid,
    tradeAccountNumber: criteria.value.tradeAccountNumber,
    ticket: criteria.value.ticket,
    status: allPageStatus.value,
  };

  salesRebateExportModalRef.value?.show(currentQuery, period.value);
};

// watch(
//   () => period.value,
//   (periodVal) => {
//     const [from, to] = periodVal;
//     const isDST = isDateInDST_US();
//     criteria.value.from = from
//       ? moment(from)
//           .subtract(1, "days")
//           .format(`YYYY-MM-DD[T]${isDST ? 21 : 22}:00:00.000[Z]`)
//       : null;
//     criteria.value.to = to
//       ? moment(to)
//           .subtract(1, "days")
//           .format(`YYYY-MM-DD[T]${isDST ? 20 : 21}:59:59.000[Z]`)
//       : null;
//   }
// );
</script>
<style>
.account-select {
  background-color: rgba(254, 215, 215, 0.5) !important;
}
</style>
