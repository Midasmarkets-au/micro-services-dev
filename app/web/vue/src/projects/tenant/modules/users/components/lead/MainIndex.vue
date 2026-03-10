<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <el-input
          v-model="criteria.assignedAccountUid"
          placeholder="AccountUid"
          clearable
          class="me-5"
        >
          <template #append>
            <el-button
              :icon="Search"
              @click="fetchData(1)"
              :loading="isLoading"
            />
          </template>
        </el-input>

        <el-select
          v-model="criteria.isArchived"
          class="me-5"
          placeholder="Is Archived"
          clearable
          @change="fetchData(1)"
          :disabled="isLoading"
        >
          <el-option :value="0" :label="$t('status.active')" />
          <el-option :value="1" :label="$t('status.archived')" />
        </el-select>

        <el-select
          v-model="criteria.isAssigned"
          class="me-5"
          placeholder="Is Assigned"
          clearable
          @change="fetchData(1)"
          :disabled="isLoading"
        >
          <el-option :value="true" :label="$t('status.assigned')" />
          <el-option :value="false" :label="$t('status.unassigned')" />
        </el-select>

        <el-select
          v-model="criteria.hasReferCode"
          class="me-5"
          placeholder="Has ReferCode"
          clearable
          @change="fetchData(1)"
          :disabled="isLoading"
        >
          <el-option :value="true" :label="$t('status.hasReferCode')" />
          <el-option :value="false" :label="$t('status.noReferCode')" />
        </el-select>

        <el-button :disabled="isLoading" @click="reset" class="me-5">
          {{ $t("action.reset") }}
        </el-button>
      </div>

      <div class="card-toolbar">
        <el-button
          type="warning"
          @click="openAutoAssignSetting"
          :disabled="isLoading"
          >{{ $t("fields.autoAssignSetting") }}</el-button
        >
        <el-button
          type="warning"
          @click="openAssignSalesModal"
          v-if="selectedLeads.length > 0"
        >
          {{ $t("action.assign") }}
        </el-button>
        <el-button type="success" @click="openCreateLeadModal">
          {{ $t("action.create") }}
        </el-button>
      </div>
    </div>
    <div class="card-body py-4">
      <table
        class="table align-middle table-row-dashed fs-6 gy-5 table-hover"
        id="table_accounts_requests"
      >
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th class="">{{ $t("fields.user") }}</th>
            <th class="">{{ $t("fields.email") }}</th>
            <th class="">{{ $t("fields.phone") }}</th>
            <th class="">{{ $t("fields.source") }}</th>
            <th class="">{{ $t("fields.status") }}</th>
            <th class="">{{ $t("fields.hasAssigned") }}</th>
            <th class="">{{ $t("fields.createdOn") }}</th>
            <th class="">{{ $t("fields.updatedOn") }}</th>
            <th class="">{{ $t("fields.action") }}</th>
          </tr>
        </thead>

        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && data.length === 0">
          <NoDataBox />
        </tbody>

        <tbody v-else class="fw-semibold text-gray-600">
          <tr
            v-for="(item, index) in data"
            :key="index"
            :class="{
              'tr-select': item.id === accountSelected,
            }"
            @click="selectedAccount(item.id)"
          >
            <td>
              <div class="d-md-flex align-items-center">
                <UserAvatar
                  :avatar="item.user?.avatar ?? ''"
                  :name="item.user?.displayName || item.name"
                  size="40px"
                  class="me-3"
                  side="client"
                  rounded
                />
                <span>
                  {{
                    item.user?.nativeName || item.user?.displayName || item.name
                  }}
                </span>
              </div>
            </td>

            <td>{{ item.email }}</td>
            <td>{{ item.phoneNumber }}</td>
            <td>{{ $t(`type.leadSource.${item.sourceType}`) }}</td>
            <td>{{ $t(`type.leadStatus.${item.status}`) }}</td>
            <td>
              <template v-if="item.hasAssignedToSales"
                ><i class="fa-solid fa-check fa-xl" style="color: #4ed06e"></i>
              </template>
              <template v-else>
                <i class="fa-solid fa-xmark fa-xl" style="color: #d92626"></i>
              </template>
            </td>

            <td>
              <TimeShow type="inFields" :date-iso-string="item.createdOn" />
            </td>

            <td>
              <TimeShow type="inFields" :date-iso-string="item.updatedOn" />
            </td>
            <td>
              <span
                class="cursor-pointer"
                style="color: #7c8fa2"
                @click="openLeadDetailsModal(item)"
              >
                {{ $t("action.showDetails") }}
              </span>
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>

    <LeadCreateModal
      ref="leadCreateModalRef"
      @create-success="assignSalesSuccess"
    />
    <LeadDetailsModal ref="leadDetailsModalRef" @update="assignSalesSuccess" />
    <AssignToSalesModal
      ref="assignToSalesModalRef"
      @assign-success="assignSalesSuccess"
    />
    <AutoAssignModal ref="autoAssignModalRef" />
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted, inject } from "vue";
import UserService from "../../services/UserService";
import { LeadIsArchivedTypes } from "@/core/types/LeadIsArchivedTypes";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import LeadCreateModal from "./LeadCreateModal.vue";
import AssignToSalesModal from "./AssignToSalesModal.vue";
import LeadDetailsModal from "./LeadDetailsModal.vue";
import AutoAssignModal from "./AutoAssignModal.vue";
import { Search } from "@element-plus/icons-vue";

const isLoading = inject<any>("isLoading");
const data = ref<any>([]);
const accountSelected = ref(0);
const leadCreateModalRef = ref<InstanceType<typeof LeadCreateModal>>();
const assignToSalesModalRef = ref<InstanceType<typeof AssignToSalesModal>>();
const leadDetailsModalRef = ref<InstanceType<typeof LeadDetailsModal>>();
const autoAssignModalRef = ref<InstanceType<typeof AutoAssignModal>>();
const selectedLeads = ref(Array<any>());
const criteria = ref<any>({
  page: 1,
  size: 20,
});

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const res = await UserService.queryRepLeads(criteria.value);
    data.value = res.data;
    criteria.value = res.criteria;
  } catch (e) {
    MsgPrompt.error(e);
  } finally {
    isLoading.value = false;
  }
};

const reset = async () => {
  criteria.value = {
    page: 1,
    size: 20,
  };
  await fetchData(1);
};

const openAssignSalesModal = () => {
  assignToSalesModalRef.value?.show(selectedLeads.value);
};
const openCreateLeadModal = () => {
  leadCreateModalRef.value?.show();
};

const openAutoAssignSetting = () => {
  autoAssignModalRef.value?.show();
};

const assignSalesSuccess = () => {
  selectedLeads.value = [];
  fetchData(1);
};

const openLeadDetailsModal = (_lead) => {
  leadDetailsModalRef.value?.show(_lead);
};

const selectedAccount = (id: number) => {
  accountSelected.value = id;
};
onMounted(async () => {
  await fetchData(1);
});
</script>
