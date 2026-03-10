<template>
  <div class="d-flex flex-column flex-lg-row">
    <div
      class="flex-column flex-lg-row-auto w-100 w-lg-300px w-xl-300px mb-10 mb-lg-0 report-list-container"
    >
      <div class="card">
        <div class="card-header">
          <div class="card-title">{{ $t("title.reportTypes") }}</div>
          <div class="card-toolbar">
            <el-button
              type="warning"
              @click="showCreateTypes()"
              size="small"
              v-if="$can('SuperAdmin')"
            >
              Create (Super)
            </el-button>
          </div>
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
          <div class="card-title">
            {{ title }}
          </div>
          <div class="card-toolbar" v-if="reportType">
            <el-button type="success" plain @click="fetchData(1)">
              {{ $t("action.refresh") }}
            </el-button>

            <el-button type="success" @click="showCreateReport()">
              {{ $t("action.export") }}
            </el-button>
          </div>
        </div>

        <div class="card-body">
          <table
            class="table align-middle table-row-dashed fs-6 gy-5"
            id="table_accounts_requests"
          >
            <thead>
              <tr
                class="text-start text-muted fw-bold fs-7 text-uppercase gs-0"
              >
                <th>id</th>
                <th>{{ $t("fields.name") }}</th>
                <th>{{ $t("fields.createdOn") }}</th>
                <th>{{ $t("fields.hasData") }}</th>
                <th>{{ $t("fields.valid") }}</th>
                <th>{{ $t("fields.status") }}</th>
                <th>{{ $t("fields.action") }}</th>
                <th>{{ $t("fields.viewQuery") }}</th>
              </tr>
            </thead>

            <tbody v-if="isLoading">
              <LoadingRing />
            </tbody>
            <tbody v-else-if="!isLoading && reports.length === 0">
              <NoDataBox />
            </tbody>

            <tbody v-else class="fw-semibold">
              <tr
                v-for="(item, index) in reports"
                :key="index"
                :class="{
                  'tr-select': item.partyId === partyId,
                }"
              >
                <td>{{ item.id }}</td>
                <td>{{ item.name }}</td>
                <td><TimeShow :date-iso-string="item.createdOn" /></td>
                <td>
                  <template v-if="!item.isEmpty"
                    ><i
                      class="fa-solid fa-check fa-xl"
                      style="color: #4ed06e"
                    ></i>
                  </template>
                  <template v-else>
                    <i
                      class="fa-solid fa-xmark fa-xl"
                      style="color: #d92626"
                    ></i>
                  </template>
                </td>

                <td>
                  <template v-if="!item.isExpired"
                    ><i
                      class="fa-solid fa-check fa-xl"
                      style="color: #4ed06e"
                    ></i>
                  </template>
                  <template v-else>
                    <i
                      class="fa-solid fa-xmark fa-xl"
                      style="color: #d92626"
                    ></i>
                  </template>
                </td>

                <td style="color: rgb(124, 143, 162)">
                  <template v-if="item.isExpired"
                    >{{ $t("status.failed") }}
                  </template>
                  <template v-else-if="item.isGenerated"
                    >{{ $t("status.readyForDownload") }}
                  </template>
                  <template v-else>
                    {{ $t("status.processing") }} ...
                  </template>
                </td>
                <td>
                  <el-button
                    v-if="item.isGenerated"
                    type="primary"
                    :icon="Download"
                    circle
                    @click="downloadReportFile(item)"
                    :disabled="downloading"
                  ></el-button>
                </td>
                <td>
                  <span
                    v-for="(value, index) in processQuery(item.query)"
                    :key="index"
                  >
                    <span class="me-3">{{ index }}: {{ value }}</span>
                  </span>
                </td>
              </tr>
            </tbody>
          </table>

          <TableFooter @page-change="fetchData" :criteria="criteria" />
        </div>
      </div>
    </div>
  </div>

  <CreateTypes ref="createTypesRef"></CreateTypes>
  <CreateReport ref="createReportRef" @fetch-data="fetchData(1)"></CreateReport>
</template>
<script lang="ts" setup>
import { onMounted, ref, computed } from "vue";
import ReportService from "@/projects/tenant/modules/report/services/ReportService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import LoadingCentralBox from "@/components/LoadingCentralBox.vue";
import NoDataCentralBox from "@/components/NoDataCentralBox.vue";
import TimeShow from "@/components/TimeShow.vue";
import { getImageUrl } from "@/core/plugins/ProcessImageLink";
import TenantGlobalService from "@/projects/tenant/services/TenantGlobalService";
import CreateTypes from "./CreateTypes.vue";
import CreateReport from "./jpComponent/CreateReport.vue";
import { Download } from "@element-plus/icons-vue";
import { useStore } from "@/store";
import { ElLoading } from "element-plus";

const title = ref("");
const createTypesRef = ref<any>(null);
const createReportRef = ref<any>(null);
const reports = ref<Array<any>>([]);
const reportTypes = ref<Array<any>>([]);
const reportType = ref(<any>null);
const isLoadingTypes = ref(false);
const isLoading = ref(false);
const downloading = ref(false);
const groups = ref([]);
const partyId = ref(null);
const criteria = ref({
  page: 1,
  size: 10,
  type: null,
});
const idSelected = ref(null);
const selectBackground = (id) => {
  idSelected.value = id;
};

const fetchTypes = async () => {
  isLoadingTypes.value = true;
  try {
    const res = await ReportService.queryReqortTypes();
    reportTypes.value = res.data;
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoadingTypes.value = false;
  }
};

const fetchGroups = async () => {
  isLoadingTypes.value = true;
  try {
    const res = await ReportService.queryGroups();
    groups.value = res;
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoadingTypes.value = false;
  }
};

const fetchData = async (_page: number) => {
  criteria.value.page = _page;
  if (reportType.value) {
    criteria.value.type = reportType.value.id;
  } else {
    return false;
  }
  isLoading.value = true;
  try {
    const res = await ReportService.queryRequests(criteria.value);
    reports.value = res.data;
    criteria.value = res.criteria;
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

const fetchUserData = async () => {
  const store = useStore();
  const user = computed(() => store.state.AuthModule?.user);
  isLoading.value = false;
  try {
    const res = await ReportService.getUserDataByUid(user.value.uid);
    partyId.value = res.partyId;
  } catch (error) {
    MsgPrompt.error(error);
  }
  isLoading.value = false;
};

const processQuery = (_query) => {
  var query = JSON.parse(_query);
  return query;
};

const downloadReportFile = async (_item) => {
  downloading.value = true;
  const loading = ElLoading.service({
    lock: true,
    text: "Downloading...",
    background: "rgba(0, 0, 0, 0.7)",
  });
  const downloadUrl = getImageUrl(_item.fileName);
  try {
    await TenantGlobalService.downloadFileByLink(downloadUrl, _item.fileName);
  } catch (error) {
    MsgPrompt.error(error);
  }
  loading.close();
  downloading.value = false;
};

const showReports = async (type: any) => {
  reportType.value = type;
  title.value = type.title;
  await fetchData(1);
};

const showCreateTypes = () => {
  createTypesRef.value.show();
};

const showCreateReport = () => {
  createReportRef.value.show(reportType.value, groups.value);
};

onMounted(async () => {
  await fetchUserData();
  await fetchTypes();
  await fetchGroups();
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
