<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <el-date-picker
          v-model="selectedDate"
          placeholder="Pick a day"
        ></el-date-picker>
        <el-input
          v-model="criteria.AccountNumber"
          placeholder="Account Number"
          class="mx-4"
          clearable
        />
        <el-select
          v-model="criteria.status"
          placeholder="Status"
          class="mx-4"
          clearable
        >
          <el-option
            v-for="item in AccountReportStatusOptions"
            :key="item.value"
            :label="item.label"
            :value="item.value"
          />
        </el-select>
        <el-button @click="fetchData(1)" type="primary">
          {{ $t("action.search") }}
        </el-button>
        <el-button @click="reset" type="info" plain>
          {{ $t("action.reset") }}
        </el-button>
      </div>
    </div>
    <div class="card-body">
      <table class="table align-middle table-row-dashed fs-6 gy-3 table-hover">
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th>{{ $t("fields.date") }}</th>
            <th>{{ $t("fields.accountNumber") }}</th>
            <th>Generated</th>
            <th>{{ $t("fields.email") }}</th>
            <!-- <th>Check</th> -->
            <!-- <th>{{ $t("title.email") }}</th> -->
            <!-- <th>{{ $t("fields.remark") }}</th> -->
            <th>{{ $t("fields.updatedOn") }}</th>
            <th>{{ $t("fields.createdOn") }}</th>
            <th>{{ $t("action.action") }}</th>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && data.length == 0">
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr
            v-for="(item, index) in data"
            :key="index"
            :class="{
              'user-select': item.id === userSelected,
            }"
            @click="tagUser(item)"
          >
            <td>
              <TimeShow type="reportDate" :date-iso-string="item.date" />
            </td>
            <td>{{ item.accountNumber }}</td>
            <td>
              <el-icon size="17" v-if="item.status >= 10" color="#67c23a"
                ><Select
              /></el-icon>
              <el-icon size="17" v-else color="#979b95"><Clock /></el-icon>
            </td>
            <td>
              <el-icon size="17" v-if="item.status > 10" color="#67c23a"
                ><Select
              /></el-icon>
              <el-icon size="17" v-else color="#979b95"><Clock /></el-icon>
            </td>
            <td>
              <TimeShow type="reportTime" :date-iso-string="item.updatedOn" />
            </td>
            <td>
              <TimeShow type="reportTime" :date-iso-string="item.createdOn" />
            </td>
            <td>
              <el-button
                type="primary"
                size="small"
                :disabled="item.status < 10"
                @click="send(item)"
                >{{ $t("action.send") }}</el-button
              >
              <el-button
                type="warning"
                size="small"
                :disabled="item.status < 10"
                @click="viewDetail(item)"
                >{{ $t("action.view") }}</el-button
              >
              <!-- <el-button type="primary" size="small" @click="download(item)">{{
                $t("action.download")
              }}</el-button> -->
              <!-- 
              <el-button type="primary" size="small" @click="regenerate(item)"
                >Regenerate</el-button
              > -->
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="changePage" :criteria="criteria" />
    </div>
  </div>
  <ConfirmationDetail ref="viewDetailModalRef" />
  <SendConfirmation ref="sendConfirmationModalRef" />
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";
import ReportService from "../services/ReportService";
import { Select, CloseBold, Clock } from "@element-plus/icons-vue";
import { ElNotification } from "element-plus";
import ConfirmationDetail from "../components/clientConfirmation/ConfirmationDetail.vue";
import SendConfirmation from "../components/clientConfirmation/SendConfirmation.vue";
import {
  AccountReportStatusTypes,
  AccountReportStatusOptions,
} from "@/core/types/AccountReportStatusTypes";
import moment from "moment";
const data = ref(Array<any>());
const viewDetailModalRef = ref<any>(null);
const sendConfirmationModalRef = ref<any>(null);
const isLoading = ref(false);
const userSelected = ref(0);
const selectedDate = ref(moment().format("YYYY-MM-DD"));
const criteria = ref({
  page: 1,
  pageSize: 20,
  date: "",
});
// console.log(moment().format("YYYY-MM-DD") + "T00:00:00.000Z")
const tagUser = (item: any) => {
  userSelected.value = item.id;
};

const changePage = (page: number) => {
  fetchData(page);
};

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  criteria.value.date =
    moment(selectedDate.value).format("YYYY-MM-DD") + "T00:00:00.000Z";
  try {
    const res = await ReportService.queryAccountReport(criteria.value);
    data.value = res.data;
    criteria.value = res.criteria;
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const viewDetail = (item: any) => {
  viewDetailModalRef.value?.show(item);
};

const reset = async () => {
  selectedDate.value = moment().format("YYYY-MM-DD");
  criteria.value = {
    page: 1,
    pageSize: 20,
    date: moment().format("YYYY-MM-DD") + "T00:00:00.000Z",
  };
  fetchData(1);
};

const send = async (item: any) => {
  sendConfirmationModalRef.value?.show(item);
};

const download = async (item: any) => {
  try {
    // const res = await ReportService.downloadClientConfirmation(item.id);
    ElNotification({
      title: "Success",
      message: "Download successfully",
      type: "success",
    });
  } catch (error) {
    ElNotification({
      title: "Error",
      message: "Download failed",
      type: "error",
    });
  }
};

const regenerate = async (item: any) => {
  try {
    // const res = await ReportService.regenerateClientConfirmation(item.id);
    ElNotification({
      title: "Success",
      message: "Regenerate successfully",
      type: "success",
    });
  } catch (error) {
    ElNotification({
      title: "Error",
      message: "Regenerate failed",
      type: "error",
    });
  }
};
onMounted(() => {
  fetchData(1);
});
</script>
<style>
.user-select {
  background-color: rgba(254, 215, 215, 0.5) !important;
}
</style>
