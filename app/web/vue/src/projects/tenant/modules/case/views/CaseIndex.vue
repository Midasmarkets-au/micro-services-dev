<template>
  <ul
    class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
  >
    <li class="nav-item">
      <a
        :class="[
          'nav-link text-active-primary pb-4',
          { active: tab === tabStatus.Pending },
        ]"
        data-bs-toggle="tab"
        href="#"
        @click="changeTab(tabStatus.Pending)"
        >{{ $t("status.pending") }}</a
      >
    </li>

    <li class="nav-item">
      <a
        :class="[
          'nav-link text-active-primary pb-4',
          { active: tab === tabStatus.Resolved },
        ]"
        data-bs-toggle="tab"
        href="#"
        @click="changeTab(tabStatus.Resolved)"
        >{{ $t("status.completed") }}</a
      >
    </li>

    <li class="nav-item">
      <a
        :class="[
          'nav-link text-active-primary pb-4',
          { active: tab === tabStatus.All },
        ]"
        data-bs-toggle="tab"
        href="#"
        @click="changeTab(tabStatus.All)"
        >{{ $t("status.all") }}</a
      >
    </li>
  </ul>

  <!-- First Table -->
  <div class="card">
    <div class="card-header">
      <div class="card-title" v-if="tab === SupportTabStatus.Pending">
        <p>Unclaimed Cases</p>
      </div>
    </div>
    <div class="card-body">
      <table class="table align-middle table-row-dashed fs-6 gy-5">
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <td>Case Id</td>
            <td>{{ $t("fields.category") }}</td>
            <td>{{ $t("fields.status") }}</td>
            <td>{{ $t("fields.createdOn") }}</td>
            <td v-if="tab != SupportTabStatus.Pending">Claimed By</td>
            <td>{{ $t("fields.action") }}</td>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && data.length === 0">
          <NoDataBox />
        </tbody>
        <tbody v-else class="fw-semibold text-gray-900">
          <tr v-for="(item, index) in data" :key="index">
            <td>{{ item.caseId }}</td>
            <td>{{ $t("title." + item.categoryName) }}</td>
            <td>
              <span class="badge" :class="getTagType(item.status)">{{
                CaseStatusTypes[item.status]
              }}</span>
            </td>
            <td><TimeShow :date-iso-string="item.createdOn" /></td>
            <td v-if="tab != SupportTabStatus.Pending">
              {{ item.claimedAdminName }}
            </td>
            <td>
              <el-button
                v-if="item.status == CaseStatusTypes.Created"
                type="success"
                @click="openConfirmPanel(ActionType.Claim, item.id)"
                >Claim</el-button
              >
              <el-button type="primary" @click="show(item)">{{
                $t("action.view")
              }}</el-button>
              <el-button
                v-if="item.status != CaseStatusTypes.Closed"
                type="danger"
                @click="openConfirmPanel(ActionType.Close, item.id)"
                >{{ $t("action.close") }}</el-button
              >
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
  </div>

  <!-- Second Table -->
  <div class="card mt-5" v-if="tab === SupportTabStatus.Pending">
    <div class="card-header">
      <div class="card-title">Processing Cases</div>
    </div>
    <div class="card-body">
      <table class="table align-middle table-row-dashed fs-6 gy-5">
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <td>Case Id</td>
            <td>{{ $t("fields.category") }}</td>
            <td>{{ $t("fields.status") }}</td>
            <td>{{ $t("fields.createdOn") }}</td>
            <td>Claimed By</td>
            <td>{{ $t("fields.action") }}</td>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && processingData.length === 0">
          <NoDataBox />
        </tbody>
        <tbody v-else class="fw-semibold text-gray-900">
          <tr v-for="(item, index) in processingData" :key="index">
            <td>{{ item.caseId }}</td>
            <td>{{ $t("title." + item.categoryName) }}</td>
            <td>
              <span class="badge" :class="getTagType(item.status)">{{
                CaseStatusTypes[item.status]
              }}</span>
            </td>
            <td><TimeShow :date-iso-string="item.createdOn" /></td>
            <td>{{ item.claimedAdminName }}</td>
            <td>
              <el-button type="primary" @click="show(item)">{{
                $t("action.view")
              }}</el-button>
              <el-button
                type="danger"
                @click="openConfirmPanel(ActionType.Close, item.id)"
                >{{ $t("action.close") }}</el-button
              >
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
  </div>
  <CaseDetail ref="caseDetailRef" />
</template>
<script setup lang="ts">
import { ref, onMounted, inject } from "vue";
import {
  SupportTabStatus,
  CaseStatusTypes,
} from "@/core/types/SupportStatusTypes";
import SupportService from "../services/SupportService";
import CaseDetail from "../components/CaseDetail.vue";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";

const isLoading = ref(false);
const tabStatus = SupportTabStatus;
const data = ref(<any>[]);
const processingData = ref(<any>[]);
const tab = ref(tabStatus.Pending);
const caseDetailRef = ref<any>(null);

enum ActionType {
  Close = "Close",
  Claim = "Claim",
}

const criteria = ref({
  page: 1,
  size: 25,
  statuses: [CaseStatusTypes.Created],
});

const processingCriteria = ref({
  page: 1,
  size: 25,
  statuses: [CaseStatusTypes.Processing, CaseStatusTypes.Replied],
});

const changeTab = (_tab: any) => {
  tab.value = _tab;
  if (tab.value == tabStatus.All) {
    criteria.value.statuses = null;
  } else {
    criteria.value.statuses = [tab.value];
  }
  fetchData(1);
};

const fetchData = async (_page: number) => {
  isLoading.value = true;
  try {
    criteria.value.page = _page;

    const res = await SupportService.queryCasesList(criteria.value);

    if (tab.value === tabStatus.Pending) {
      fetchProcessingData(_page);
    }
    data.value = res.data;
    criteria.value = res.criteria;
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const fetchProcessingData = async (_page: number) => {
  isLoading.value = true;
  try {
    processingCriteria.value.page = _page;
    const res = await SupportService.queryCasesList(processingCriteria.value);
    processingData.value = res.data;
    processingCriteria.value = res.criteria;
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const show = (item: any) => {
  caseDetailRef.value.show(item);
};

const claimCase = async (id: number) => {
  try {
    const res = await SupportService.claimCase(id);
    console.log(res);
  } catch (error) {
    console.log(error);
  }
};

const closeCase = async (id: number) => {
  try {
    const res = await SupportService.closeCase(id);
    console.log(res);
  } catch (error) {
    console.log(error);
  }
};

const openConfirmModal = inject<any>(
  TenantGlobalInjectionKeys.OPEN_CONFIRM_MODAL
);
const openConfirmPanel = (_action: ActionType, id: number) => {
  openConfirmModal?.(() => {
    return {
      [ActionType.Claim]: () => claimCase(id),
      [ActionType.Close]: () => closeCase(id),
    }
      [_action]()
      .then(() => {
        fetchData(criteria.value.page);
      });
  });
};

const getTagType = (status) => {
  switch (status) {
    case CaseStatusTypes.Processing:
      return "badge-primary";
    case CaseStatusTypes.Replied:
      return "badge-warning";
    case CaseStatusTypes.Created:
      return "badge-success";
    case CaseStatusTypes.Closed:
      return "badge-danger";
    default:
      return ""; // Default type or you can define a default type
  }
};

onMounted(() => {
  fetchData(1);
});
</script>
