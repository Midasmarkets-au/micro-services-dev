<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <el-select
          v-model="selectedOption"
          placeholder="Select option"
          class="w-150px me-3"
          :disabled="isLoading"
        >
          <el-option label="Wallet ID" value="walletId"></el-option>
          <el-option label="Email" value="email"></el-option>
          <el-option label="Party Id" value="partyId"></el-option>
        </el-select>
        <el-input
          class="w-200px me-3"
          v-model="inputValue"
          :disabled="isLoading"
        />
        <el-date-picker
          class="w-400px me-3"
          v-model="period"
          type="daterange"
          :start-placeholder="$t('fields.startDate')"
          :end-placeholder="$t('fields.endDate')"
          format="YYYY-MM-DD"
          value-format="YYYY-MM-DD HH:mm:ss"
        />
        <el-button type="primary" @click="search" plain :disabled="isLoading"
          >{{ $t("action.search") }}
        </el-button>
        <el-button @click="reset" plain :disabled="isLoading"
          >{{ $t("action.clear") }}
        </el-button>
        <!-- <el-button
          type="success"
          @click="submitReportRequest"
          :loading="exporting"
          :disabled="isLoading"
          plain
          >{{ $t("action.export") }}
        </el-button> -->
      </div>
      <div class="card-toolbar">
        <el-button type="primary" @click="create()"
          >{{ $t("action.create") }}
        </el-button>
      </div>
    </div>
    <div class="card-body">
      <table class="table table-tenant">
        <thead>
          <tr>
            <th>{{ $t("fields.user") }}</th>
            <th>Wallet ID</th>
            <th class="">{{ $t("fields.isPrimary") }}</th>
            <th>{{ $t("fields.amount") }}</th>
            <th>{{ $t("fields.comment") }}</th>
            <th>
              {{ $t("fields.operatedBy") }}
            </th>
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
            <td>
              <UserInfo
                :email="item.email"
                :name="item.nativeName"
                class="me-2"
              />
            </td>
            <td>{{ item.walletId }}</td>
            <td>{{ item.isPrimary ? $t(`action.yes`) : $t(`action.no`) }}</td>
            <td><BalanceShow :balance="item.amount" /></td>
            <td>{{ item.comment }}</td>
            <td>{{ item.operatorName }}</td>
            <td>
              <TimeShow :date-iso-string="item.createdOn" type="oneLiner" />
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
    <CreateWalletAdjust
      ref="CreateWalletAdjustRef"
      @event-submit="fetchData(1)"
    />
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";
import PaymentService from "../services/PaymentService";
import CreateWalletAdjust from "../components/walletAdjust/CreateWalletAdjust.vue";
import { ReportRequestTypes } from "@/core/types/ReportRequestTypes";
import { CreateReportSpec } from "@/projects/tenant/services/TenantGlobalService";
import { convertToUTC } from "@/core/utils/DateUtils";
const isLoading = ref(false);
const CreateWalletAdjustRef = ref<InstanceType<typeof CreateWalletAdjust>>();
const data = ref(<any>[]);
const criteria = ref<any>({
  page: 1,
  size: 25,
});
const selectedOption = ref("walletId");
const inputValue = ref("");
const period = ref([] as any);
const defaultTime = ref<[Date, Date]>([
  new Date(2000, 1, 1, 0, 0, 0),
  new Date(2000, 2, 1, 23, 59, 59),
]);

const formData = ref<CreateReportSpec>({
  name: "",
  type: ReportRequestTypes.SalesRebateForTenant,
  query: criteria.value,
});

const search = () => {
  delete criteria.value.walletId;
  delete criteria.value.email;
  delete criteria.value.partyId;
  criteria.value[selectedOption.value] = inputValue.value;
  fetchData(1);
};

const reset = () => {
  criteria.value = { page: 1, size: 25 };
  period.value = [];
  inputValue.value = "";
  fetchData(1);
};

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  const datesRange = convertToUTC(period.value);
  criteria.value.from = datesRange.from;
  criteria.value.to = datesRange.to;
  try {
    const res = await PaymentService.getWalletAdjust(criteria.value);
    criteria.value = res.criteria;
    data.value = res.data;
  } catch (error) {
    console.error(error);
  } finally {
    isLoading.value = false;
  }
};

const create = () => {
  CreateWalletAdjustRef.value?.show();
};

// const submitReportRequest = async () => {
//   exporting.value = true;
//   const [from, to] = period.value;
//   const isDST = isDateInDST_US();
//   criteria.value.from = from
//     ? moment(from).format(`YYYY-MM-DD[T]${isDST ? 21 : 22}:00:00.000[Z]`)
//     : null;
//   criteria.value.to = to
//     ? moment(to).format(`YYYY-MM-DD[T]${isDST ? 20 : 21}:59:59.000[Z]`)
//     : null;

//   formData.value.query = criteria.value;

//   try {
//     const media = await TenantGlobalService.createReportRequestDownload(
//       formData.value
//     );
//     await TenantGlobalService.downloadFileByGuid(media.guid, media.fileName);
//   } catch (error) {
//     MsgPrompt.error(error);
//   } finally {
//     exporting.value = false;
//   }
// };

// watch(
//   () => period.value,
//   (periodVal) => {
//     if (periodVal && periodVal.length > 0 && typeof periodVal[0] !== "string") {
//       periodVal = [
//         moment(periodVal[0]).local().toISOString(),
//         moment(periodVal[1]).local().toISOString(),
//       ];
//     }
//     criteria.value.from = periodVal ? periodVal[0] : null;
//     criteria.value.to = periodVal ? periodVal[1] : null;
//   }
// );

onMounted(() => {
  fetchData(1);
});
</script>
