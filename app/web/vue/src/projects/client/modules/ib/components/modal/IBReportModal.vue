<template>
  <div
    class="modal fade"
    id="kt_modal_iblibk_detail"
    tabindex="-1"
    aria-hidden="true"
    ref="IBReportModalRef"
  >
    <div class="modal-dialog modal-dialog-centered mw-750px">
      <div class="modal-content">
        <div class="modal-header" id="kt_modal_new_address_header">
          <h2 class="fs-2">{{ $t("title.report") }}</h2>

          <div data-bs-dismiss="modal">
            <span class="svg-icon svg-icon-1">
              <inline-svg src="/images/icons/arrows/arr061.svg" />
            </span>
          </div>
        </div>
        <!-- ------------------------------------------------------------------ -->
        <div class="form-outer ps-5 pe-5">
          <table
            class="table align-middle table-row-bordered gy-5"
            id="table_accounts_requests"
          >
            <thead>
              <tr
                class="text-start text-muted fw-bold fs-7 text-uppercase gs-0"
              >
                <th class="">{{ $t("fields.date") }}</th>
                <th class="">{{ $t("fields.accountNumber") }}</th>
                <th class="">{{ $t("fields.createdOn") }}</th>
                <th class="">{{ $t("fields.status") }}</th>
                <th class="text-center">{{ $t("fields.action") }}</th>
              </tr>
            </thead>

            <tbody v-if="isLoading">
              <LoadingRing />
            </tbody>
            <tbody v-else-if="!isLoading && reports.length === 0">
              <NoDataBox />
            </tbody>

            <tbody v-else class="fw-semibold">
              <tr v-for="(item, key) in reports" :key="key">
                <td>{{ key }}</td>
                <td>{{ item.accountNumber }}</td>
                <td>
                  <TimeShow
                    v-if="item.createdOn"
                    :date-iso-string="item.createdOn"
                  />
                </td>
                <td style="color: rgb(124, 143, 162)">
                  <template v-if="item.status == 10"
                    >{{ $t("status.readyForDownload") }}
                  </template>
                  <template v-else-if="item.status == 0">
                    {{ $t("status.processing") }} ...
                  </template>
                  <template v-else> {{ $t("status.none") }} </template>
                </td>
                <td class="text-center">
                  <a
                    v-if="item.status == 10"
                    href="#"
                    @click="downloadReportFile(item)"
                    ><i
                      class="fa-solid fa-download fa-xl"
                      style="color: #5b6b86"
                    ></i
                  ></a>
                  <a
                    v-if="item.dataFile == null"
                    href="#"
                    @click="exportReport(key)"
                    :disabled="exporting"
                    >{{ $t("action.export") }}</a
                  >
                </td>
              </tr>
            </tbody>
          </table>

          <TableFooter @page-change="fetchData" :criteria="criteria" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useStore } from "@/store";
import IbService from "../../services/IbService";
import { showModal } from "@/core/helpers/dom";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { getImageUrl } from "@/core/plugins/ProcessImageLink";
import TableFooter from "@/components/TableFooter.vue";
import TenantGlobalService from "@/projects/tenant/services/TenantGlobalService";
import IbReportService from "@/projects/client/modules/ib/services/IbReportService";

const month = ref("");
const store = useStore();
const ibLinkDetail = ref();
const exporting = ref(false);
const isLoading = ref(true);
const reports = ref<Array<any>>([]);
const IBReportModalRef = ref<null | HTMLElement>(null);

const criteria = ref({
  page: 1,
  size: 10,
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

const exportReport = async (month: any) => {
  exporting.value = true;

  const formData = ref({
    tenancy: store.state.AuthModule.user.tenancy,
    uid: store.state.AgentModule.agentAccount?.uid,
    month: month,
  });

  await IbReportService.createIbMonthlyReport(formData.value).then(
    MsgPrompt.success(
      "Request has been submitted, please check the Report later"
    ).then(() => {
      exporting.value = false;
      fetchData(1);
    })
  );
};

const show = async () => {
  isLoading.value = true;
  fetchData(1);
  showModal(IBReportModalRef.value);
};

defineExpose({
  show,
});
</script>
