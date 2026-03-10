<template>
  <div class="d-flex flex-column flex-lg-row">
    <div
      class="flex-column flex-lg-row-auto w-100 w-lg-400px w-xl-400px mb-10 mb-lg-0 report-list-container"
    >
      <div class="card">
        <div class="card-header">
          <h3 class="card-title">Equity Report Types</h3>
        </div>
        <div class="card-body">
          <div class="align-middle" v-if="isLoadingTypes">
            <LoadingCentralBox />
          </div>
          <div
            class="align-middle"
            v-else-if="!isLoadingTypes && reportTypes.length === 0"
          >
            <NoDataCentralBox />
          </div>

          <ul v-else class="report-list">
            <li
              v-for="item in reportTypes"
              :key="item.id"
              :class="{
                'selected-background': item.id === idSelected,
              }"
              @click="selectBackground(item.id)"
            >
              <FileIcon :type="item.type"></FileIcon
              ><a href="#" @click.prevent="showReports(item)">{{
                item.title
              }}</a>
            </li>
          </ul>
        </div>
      </div>
    </div>

    <div class="flex-lg-row-fluid ms-lg-7 ms-xl-10">
      <div class="card">
        <div class="card-header">
          <h3 class="card-title">
            <div>Equity Report</div>
            <div class="ms-4">
              <el-input
                v-model="accountNumber"
                placeholder="Account Number"
                clearable
                :disabled="isLoadingReportData"
                @onkeyup.enter="searchAccountData"
                @click="searchAccountData"
              >
                <template #append>
                  <el-button :icon="Search" />
                </template>
              </el-input>
            </div>
            <el-button
              type="primary"
              class="ms-4"
              @click="accountInfoRef.show()"
            >
              test
            </el-button>
          </h3>
        </div>
        <div class="card-body">
          <table class="table align-middle table-row-dashed fs-6 gy-5">
            <thead>
              <tr
                class="text-start text-muted fw-bold fs-7 text-uppercase gs-0"
              >
                <th>Currency</th>
                <th>Office</th>
                <th>Number of Accounts</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody v-if="isLoadingReportData">
              <LoadingRing />
            </tbody>
            <tbody v-else-if="!isLoadingReportData && reportData.length === 0">
              <NoDataBox />
            </tbody>
            <tbody v-else>
              <tr
                v-for="(item, index) in reportData"
                :key="index"
                :class="{
                  'tr-select': item.id === tableColId,
                }"
              >
                <td>{{ item.currency }}</td>
                <td>{{ item.office }}</td>
                <td>{{ item.numberOfAccounts }}</td>
                <td>
                  <el-button
                    type="primary"
                    size="mini"
                    @click="showEditReportInfo"
                  >
                    {{ $t("action.edit") }}
                  </el-button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
  <AccountInfo ref="accountInfoRef" />
  <EditReportInfo ref="editReportInfoRef" />
</template>
<script setup lang="ts">
import { onMounted, ref, computed } from "vue";
import LoadingCentralBox from "@/components/LoadingCentralBox.vue";
import NoDataCentralBox from "@/components/NoDataCentralBox.vue";
import { Search } from "@element-plus/icons-vue";
import ReportService from "../services/ReportService";
import AccountInfo from "../components/equityReport/AccountInfo.vue";
import EditReportInfo from "../components/equityReport/EditReportInfo.vue";
import notification from "@/core/plugins/notification";
const isLoadingTypes = ref(false);
const isLoadingReportData = ref(false);
const reportTypes = ref<any>([]);
const reportData = ref<any>([]);

const accountInfoRef = ref<any>(null);
const editReportInfoRef = ref<any>(null);
const idSelected = ref(null);
const accountNumber = ref<any>(null);
const tableColId = ref(null);
const selectBackground = (id) => {
  idSelected.value = id;
};

const searchAccountData = async () => {
  if (accountNumber.value) {
    accountInfoRef.value.show(accountNumber.value);
  }
};

const showEditReportInfo = () => {
  editReportInfoRef.value.show();
};

const showReports = async (item) => {
  isLoadingReportData.value = true;
  // try{
  //     const response = await ReportService.queryEquityReport(item.id);
  //     reportData.value = response.data;
  // } catch (error) {
  //     console.log(error);
  //     notification.danger();
  // }
  isLoadingReportData.value = false;
};

const fecthReportTypes = async () => {
  isLoadingTypes.value = true;
  // try{
  //     const response = await ReportService.queryEquityReportTypes();
  //     reportTypes.value = response.data;
  // } catch (error) {
  //     console.log(error);
  //     notification.danger();
  // }
  isLoadingTypes.value = false;
};

onMounted(async () => {
  await fecthReportTypes();
});
</script>

<style scoped>
.selected-background {
  background-color: #f1e0e0;
}
.report-list-container .card-body {
  padding: 10px;
}
.report-list {
  list-style-type: none;
  padding-left: 0;
}

.report-list li {
  padding: 10px;
  border-bottom: 1px solid #ebedf2;
  font-size: 14px;
}
.report-list li i {
  margin-right: 10px;
  font-size: 18px;
}
</style>
