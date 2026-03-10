<template>
  <div class="d-flex flex-column flex-column-fluid">
    <div v-if="!selectedKYC" class="card">
      <div class="card-body py-4">
        <div class="d-flex gap-10 justify-content-between">
          <!-- <ul
            class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
          >
            <li class="nav-item">
              <a
                class="nav-link text-active-primary pb-4"
                :class="{
                  active: tab === VerificationStatusTypes.AwaitingApprove,
                }"
                href="#"
                @click="changeTab(VerificationStatusTypes.AwaitingApprove)"
                >{{ $t("status.pending") }}</a
              >
            </li>

            <li class="nav-item">
              <a
                class="nav-link text-active-primary pb-4"
                :class="{
                  active: tab === VerificationStatusTypes.Approved,
                }"
                href="#"
                @click="changeTab(VerificationStatusTypes.Approved)"
                >{{ $t("status.finalized") }}</a
              >
            </li>
            <li class="nav-item">
              <a
                class="nav-link text-active-primary pb-4"
                :class="{
                  active: tab === 6,
                }"
                href="#"
                @click="changeTab(6)"
                >{{ $t("status.all") }}</a
              >
            </li>
          </ul> -->
          <CommonTabs />
          <div>
            <el-input
              class="w-400px"
              v-model="criteria.email"
              :clearable="true"
              placeholder="Email"
              @keyup.enter="search"
              :disabled="isLoading"
            >
              <template #append>
                <el-button
                  :icon="Search"
                  @click="search"
                  :disabled="isLoading"
                  :loading="isLoading"
                />
              </template>
            </el-input>
            <el-button
              class="ms-4"
              type="info"
              plain
              @click="reset"
              :disabled="isLoading"
              >{{ $t("action.reset") }}</el-button
            >
          </div>
        </div>
        <table
          class="table align-middle table-row-dashed fs-6 gy-5"
          id="table_accounts_requests"
        >
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th class="">{{ $t("fields.client") }}</th>
              <th class="">{{ $t("fields.updatedOn") }}</th>
              <th class="">{{ $t("fields.createdOn") }}</th>
              <th class="text-center min-w-150px">
                {{ $t("action.action") }}
              </th>
            </tr>
          </thead>

          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && items.length === 0">
            <NoDataBox />
          </tbody>

          <tbody v-else class="fw-semibold text-gray-900">
            <tr v-for="(item, index) in items" :key="index">
              <td class="d-flex align-items-center">
                <UserInfo url="#" :user="item.user" class="me-2" />
              </td>
              <td><TimeShow :date-iso-string="item.updatedOn" /></td>
              <td><TimeShow :date-iso-string="item.createdOn" /></td>
              <td class="text-center">
                <button
                  class="btn btn-light btn-primary btn-sm me-3"
                  @click="showUserKycForm(item.partyId)"
                >
                  {{ $t("action.view") }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <UserKycFinalize
      v-else
      :selectedPartyId="selectedPartyId"
      :viewFinalize="criteria.status == VerificationStatusTypes.Approved"
      @fetchData="fetchData"
      @returnTable="returnTable"
    ></UserKycFinalize>
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted, provide } from "vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import UserService from "../services/UserService";
import UserKycFinalize from "../components/UserKycFinalize.vue";
import { VerificationStatusTypes } from "@/core/types/VerificationInfos";
import { Search } from "@element-plus/icons-vue";
import CommonTabs from "@/projects/tenant/components/CommonTabs.vue";
import { useI18n } from "vue-i18n";

const { t } = useI18n();
const selectedKYC = ref(false);
const selectedPartyId = ref("-1");

const isLoading = ref(true);
const items = ref(Array<any>());
const tab = ref<any>(VerificationStatusTypes.AwaitingApprove);

const changeTab = (_tab) => {
  tab.value = _tab;
  criteria.value.statuses = tab.value;
  if (tab.value === 6) {
    criteria.value.statuses = [
      VerificationStatusTypes.AwaitingApprove,
      VerificationStatusTypes.Approved,
    ];
  }
  fetchData();
};

const criteria = ref<any>({
  sortField: "updatedOn",
  // statuses: [VerificationStatusTypes.AwaitingApprove],
  status: VerificationStatusTypes.AwaitingApprove,
});

const returnTable = () => {
  selectedKYC.value = false;
};

onMounted(() => {
  fetchData();
});
const search = () => {
  changeTab(6);
  fetchData();
};
const fetchData = async () => {
  isLoading.value = true;
  selectedKYC.value = false;

  try {
    const res = await UserService.queryKycForms(criteria.value);
    items.value = res.data;

    /**
     * TODO: Show the first item in the list, only for DEV
     */
    // if (items.value.length > 0) {
    //   showUserKycForm(items.value[0].partyId);
    // }
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};
const reset = () => {
  criteria.value.email = null;
  fetchData();
};
const showUserKycForm = (_id) => {
  selectedPartyId.value = _id;
  selectedKYC.value = true;
};

const tabsData = [
  {
    index: 0,
    label: t("status.pending"),
    status: VerificationStatusTypes.AwaitingApprove,
  },
  {
    index: 1,
    label: t("status.finalized"),
    status: VerificationStatusTypes.Approved,
  },
  {
    index: 2,
    label: t("status.all"),
    status: null,
  },
];

provide("criteria", criteria);
provide("isLoading", isLoading);
provide("tabsData", tabsData);
provide("fetchData", fetchData);
</script>
