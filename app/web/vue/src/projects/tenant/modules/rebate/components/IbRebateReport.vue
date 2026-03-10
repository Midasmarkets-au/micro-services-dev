<template>
  <div>
    <div class="card">
      <div class="card-header">
        <div class="card-title">{{ $t("title.ibRebateReport") }}</div>
        <div class="card-toolbar">
          <el-input
            class="w-150px"
            v-model="criteria.keywords"
            :placeholder="$t('fields.name')"
            clearable
          />
          <el-button
            style="margin-left: 8px"
            type="primary"
            @click="fetchData(1)"
            >{{ $t("action.search") }}</el-button
          >
          <button
            style="margin-left: 8px"
            class="btn btn-sm btn-success fs-6 ml-8"
            @click="newReportRequestModalRef?.show()"
          >
            <i class="fa-solid fa-plus fa-xl" style="color: #ffffff"></i>
            {{ $t("action.createNewRebateReport") }}
          </button>
        </div>
      </div>

      <div class="card-body">
        <table
          class="table align-middle table-row-dashed fs-6 gy-5"
          id="table_accounts_requests"
        >
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th class="">id</th>
              <th class="">{{ $t("fields.name") }}</th>
              <th class="">{{ $t("fields.createdOn") }}</th>
              <th class="">{{ $t("fields.hasData") }}</th>
              <th class="">{{ $t("fields.startTime") }}</th>
              <th class="">{{ $t("fields.endTime") }}</th>
              <th class="">{{ $t("fields.rebateStatus") }}</th>
              <th class="">{{ $t("fields.valid") }}</th>
              <th class="">{{ $t("fields.status") }}</th>
              <th class="">{{ $t("fields.action") }}</th>
            </tr>
          </thead>

          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && rebateReports.length === 0">
            <NoDataBox />
          </tbody>

          <tbody v-else class="fw-semibold">
            <tr v-for="(item, index) in rebateReports" :key="index">
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
                  <i class="fa-solid fa-xmark fa-xl" style="color: #d92626"></i>
                </template>
              </td>
              <td><TimeShow :date-iso-string="item.query.from" /></td>
              <td><TimeShow :date-iso-string="item.query.to" /></td>
              <td>
                {{
                  getTradeRebateStatusSelections.find(
                    (it) => it.value === item.query.stateId
                  )?.label || $t("fields.all")
                }}
              </td>
              <td>
                <template v-if="!item.isExpired"
                  ><i
                    class="fa-solid fa-check fa-xl"
                    style="color: #4ed06e"
                  ></i>
                </template>
                <template v-else>
                  <i class="fa-solid fa-xmark fa-xl" style="color: #d92626"></i>
                </template>
              </td>

              <td style="color: rgb(124, 143, 162)">
                <template v-if="item.isGenerated"
                  >{{ $t("status.readyForDownload") }}
                </template>
                <template v-else> {{ $t("status.processing") }} ... </template>
              </td>
              <td>
                <a
                  v-if="item.isGenerated"
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
    <NewReportRequestModal
      ref="newReportRequestModalRef"
      @fetchData="fetchData"
    />
  </div>
</template>

<script setup lang="ts">
import NewReportRequestModal from "@/projects/tenant/modules/rebate/components/modal/NewReportRequestModal.vue";
import { onMounted, ref, inject } from "vue";
import { ReportRequestTypes } from "@/core/types/ReportRequestTypes";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import TenantGlobalService from "@/projects/tenant/services/TenantGlobalService";
import TimeShow from "@/components/TimeShow.vue";
import { getImageUrl } from "@/core/plugins/ProcessImageLink";
import { getTradeRebateStatusSelections } from "@/core/types/RebateStatus";
const newReportRequestModalRef = ref();
const criteria = ref({
  page: 1,
  size: 15,
  type: ReportRequestTypes.Rebate,
  keywords: "",
});
const isLoading = inject<any>("isLoading");

const rebateReports = ref(Array<any>());
const fetchData = async (_page) => {
  criteria.value.page = _page;
  isLoading.value = true;
  try {
    const res = await TenantGlobalService.queryReportRequest(criteria.value);
    rebateReports.value = res.data.map((item) => ({
      ...item,
      query: item.query ? JSON.parse(item.query) : null,
    }));
    criteria.value = res.criteria;
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

const downloadReportFile = async (_item) => {
  const downloadUrl = getImageUrl(_item.fileName);
  try {
    await TenantGlobalService.downloadFileByLink(downloadUrl, _item.fileName);
  } catch (error) {
    MsgPrompt.error(error);
  }
};

onMounted(() => {
  fetchData(1);
});
</script>

<style scoped></style>
