<template>
  <div class="d-flex flex-column flex-column-fluid">
    <div v-if="!selectedKYC" class="card">
      <div class="card-header">
        <div class="card-title">
          <div>
            <el-input
              class="w-400px"
              v-model="criteria.email"
              :clearable="true"
              placeholder="Email"
            >
              <template #append>
                <el-button :icon="Search" @click="fetchData(1)" />
              </template>
            </el-input>
            <el-button class="ms-4" type="info" plain @click="reset">{{
              $t("action.reset")
            }}</el-button>
          </div>
        </div>
      </div>
      <div class="card-body py-4">
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
                <button
                  class="btn btn-light btn-danger btn-sm me-3"
                  @click="deleteUserKycForm(item.partyId)"
                >
                  {{ $t("action.delete") }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>

        <TableFooter @page-change="fetchData" :criteria="criteria" />
      </div>
    </div>

    <UserKycForm
      v-else
      :selectedPartyId="selectedPartyId"
      @fetchData="fetchData"
      @returnTable="returnTable"
    ></UserKycForm>
  </div>
</template>
<script setup lang="ts">
import { inject, ref, onMounted } from "vue";
import TableFooter from "@/components/TableFooter.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import UserService from "../services/UserService";
import UserKycForm from "../components/UserKycForm.vue";
import { VerificationStatusTypes } from "@/core/types/VerificationInfos";
import TimeShow from "@/components/TimeShow.vue";
import InjectKeys from "@/core/types/TenantGlobalInjectionKeys";
import { Search } from "@element-plus/icons-vue";

const selectedKYC = ref(false);
const selectedPartyId = ref("-1");

const isLoading = ref(true);
const items = ref(Array<any>());

const openConfirmModal = inject<(handleConfirm: any) => Promise<void>>(
  InjectKeys.OPEN_CONFIRM_MODAL
);

onMounted(() => {
  fetchData(1);
});
const criteria = ref({
  page: 1,
  size: 20,
  sortField: "updatedOn",
  statuses: [
    VerificationStatusTypes.AwaitingReview,
    VerificationStatusTypes.UnderReview,
  ],
});

const returnTable = () => {
  selectedKYC.value = false;
};

const fetchData = async (selectedPage: number) => {
  criteria.value.page = selectedPage;
  isLoading.value = true;
  selectedKYC.value = false;

  try {
    const res = await UserService.queryKycForms(criteria.value);
    items.value = res.data;
    criteria.value = res.criteria;
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

const showUserKycForm = async (_partyId) => {
  selectedPartyId.value = _partyId;
  selectedKYC.value = true;
};

const reset = () => {
  criteria.value.email = null;
  fetchData(1);
};

const deleteUserKycForm = async (_partyId: number) => {
  openConfirmModal?.(() =>
    UserService.rejectKycForm(_partyId)
      .then(() => MsgPrompt.success("KYC Form Rejected"))
      .then(() => fetchData(1))
  );
};
</script>
