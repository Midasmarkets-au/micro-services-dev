<template>
  <!--begin::Row-->
  <div>
    <headerMenu activeMenuItem="report" />
  </div>

  <div style="border: 0 !important">
    <div class="card p-2">
      <div class="d-flex py-5">
        <div class="d-flex justify-content-end" style="width: 100%">
          <div class="d-flex align-items-center">
            <el-date-picker
              v-model="formData.month"
              type="month"
              value-format="YYYY-MM"
              placeholder="Pick a month"
            />
            <button
              class="btn btn-primary btn-sm ms-5 me-4"
              style="width: 120px"
              @click="exportReport()"
              :disabled="isLoading"
            >
              {{ $t("action.export") }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <div class="card mt-5" style="white-space: nowrap">
      <div class="card-header">
        <div class="card-title">{{ $t("title.ibRebateReport") }}</div>
        <div class="card-toolbar"></div>
      </div>

      <div class="card-body">
        <table
          class="table align-middle table-row-bordered gy-5"
          id="table_accounts_requests"
        >
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th class="">{{ $t("fields.name") }}</th>
              <th class="">{{ $t("fields.accountNumber") }}</th>
              <th class="">{{ $t("fields.createdOn") }}</th>
              <th class="">{{ $t("fields.status") }}</th>
              <th class="">{{ $t("fields.action") }}</th>
            </tr>
          </thead>

          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && reports.length === 0">
            <NoDataBox />
          </tbody>

          <tbody v-else class="fw-semibold">
            <tr v-for="(item, index) in reports" :key="index">
              <td>{{ formatDate(item.date) }}</td>
              <td>{{ item.accountNumber }}</td>
              <td><TimeShow :date-iso-string="item.createdOn" /></td>
              <td style="color: rgb(124, 143, 162)">
                <template v-if="item.status == 10"
                  >{{ $t("status.readyForDownload") }}
                </template>
                <template v-else> {{ $t("status.processing") }} ... </template>
              </td>
              <td>
                <a
                  v-if="item.status == 10"
                  href="#"
                  @click="downloadReportFile(item)"
                  ><i
                    class="fa-solid fa-download fa-xl"
                    style="color: #5b6b86"
                  ></i
                ></a>
              </td>
            </tr>
          </tbody>
        </table>

        <TableFooter @page-change="fetchData" :criteria="criteria" />
      </div>
    </div>
  </div>

  <!--end::Row-->
</template>

<script lang="ts" setup>
import { ref, onMounted } from "vue";
import IbService from "../services/IbService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import TableFooter from "@/components/TableFooter.vue";
import TimeShow from "@/components/TimeShow.vue";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import IbReportService from "@/projects/client/modules/ib/services/IbReportService";
import { useStore } from "@/store";
import TradeFilter from "@/projects/client/components/TradeFilter.vue";
import headerMenu from "../components/menu/headerMenu.vue";
import ReportService from "@/projects/tenant/modules/report/services/ReportService";
import { getImageUrl } from "@/core/plugins/ProcessImageLink";
import TenantGlobalService from "@/projects/tenant/services/TenantGlobalService";

const reports = ref<Array<any>>([]);
const isLoading = ref(false);
const criteria = ref({
  page: 1,
  size: 10,
});
const store = useStore();
const rebateFilterRef = ref<InstanceType<typeof TradeFilter>>();
const month = ref("");
const exporting = ref(false);

const formData = ref({
  tenancy: store.state.AuthModule.user.tenancy,
  uid: store.state.AgentModule.agentAccount?.uid,
  month: month.value,
});

const fetchData = async (_page: number) => {
  criteria.value.page = _page;
  isLoading.value = true;

  await new Promise((resolve) => setTimeout(resolve, 3000));

  try {
    const res = await IbService.queryRequests(criteria.value);
    reports.value = res.data;
    criteria.value = res.criteria;
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

const downloadReportFile = async (_item) => {
  const downloadUrl = getImageUrl(_item.dataFile);
  try {
    await TenantGlobalService.downloadFileByLink(downloadUrl, _item.dataFile);
  } catch (error) {
    MsgPrompt.error(error);
  }
};

const exportReport = async () => {
  exporting.value = true;

  await IbReportService.createIbMonthlyReport(formData.value).then(
    MsgPrompt.success(
      "Request has been submitted, please check the Report Record"
    ).then(() => {
      exporting.value = false;
      fetchData(1);
    })
  );
};

const formatDate = (dateString) => {
  const date = new Date(dateString);
  console.log(date);
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1); // Months are zero-based
  return `${year}-${month}`;
};

onMounted(async () => {
  await fetchData(1);
});
</script>
