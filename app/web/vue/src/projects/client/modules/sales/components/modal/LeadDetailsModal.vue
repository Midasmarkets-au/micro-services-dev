<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('title.leadDetail')"
    :is-loading="isLoading"
    :width="1000"
    disable-footer
  >
    <el-descriptions
      class="margin-top"
      :title="$t('title.personalInfo')"
      :column="3"
      :size="size"
      border
    >
      <template #extra>
        <div class="d-flex gap-2">
          <button
            @click="openAddedCommentModal"
            class="btn btn-sm btn-secondary me-4 d-flex align-items-center gap-2"
          >
            <i class="fa-solid fa-plus fa-sm"></i>

            <span>{{ $t("action.AddComment") }}</span>
          </button>
        </div>
      </template>

      <el-descriptions-item>
        <template #label>
          <div class="cell-item">
            <el-icon :style="iconStyle">
              <user />
            </el-icon>
            {{ $t("fields.clientName") }}
          </div>
        </template>
        {{ selectedLead.name }}
      </el-descriptions-item>

      <el-descriptions-item>
        <template #label>
          <div class="cell-item">
            <el-icon :style="iconStyle">
              <Message />
            </el-icon>
            {{ $t("fields.email") }}
          </div>
        </template>
        {{ selectedLead.email }}
      </el-descriptions-item>

      <el-descriptions-item>
        <template #label>
          <div class="cell-item">
            <el-icon :style="iconStyle">
              <iphone />
            </el-icon>
            {{ $t("fields.phoneNum") }}
          </div>
        </template>
        {{ selectedLead.phoneNumber }}
      </el-descriptions-item>

      <el-descriptions-item>
        <template #label>
          <div class="cell-item">
            <el-icon :style="iconStyle">
              <tickets />
            </el-icon>
            Remarks
          </div>
        </template>
        <el-tag type="warning" size="small">Live Trading Account</el-tag>
      </el-descriptions-item>

      <el-descriptions-item>
        <template #label>
          <div class="cell-item">
            <el-icon :style="iconStyle">
              <tickets />
            </el-icon>
            {{ $t("fields.currentStatus") }}
          </div>
        </template>
        <el-tag
          :type="
            {
              [LeadStatusTypes.UserNotRegistered]: 'info',
              [LeadStatusTypes.UserRegistered]: 'warning',
              [LeadStatusTypes.TradeAccountCreated]: 'success',
              [LeadStatusTypes.AgentAccountCreated]: 'success',
              [LeadStatusTypes.UserVerificationRejected]: 'danger',
              [LeadStatusTypes.AccountApplicationRejected]: 'danger',
            }[selectedLead.status] ?? ''
          "
          size="small"
          >{{ $t(`type.leadStatus.${selectedLead.status}`) }}</el-tag
        >
      </el-descriptions-item>

      <el-descriptions-item>
        <template #label>
          <div class="cell-item">
            <el-icon :style="iconStyle">
              <office-building />
            </el-icon>
            {{ $t("fields.createdOn") }}
          </div>
        </template>
        <TimeShow :date-iso-string="selectedLead.createdOn" />
      </el-descriptions-item>
    </el-descriptions>

    <div class="demo-collapse mt-5">
      <el-collapse v-model="activeNames">
        <el-collapse-item name="1">
          <template #title>
            <div class="fs-5 fw-bold">{{ $t("title.assignments") }}</div>
          </template>
          <el-timeline v-if="selectedLead.assignedAccounts?.length > 0">
            <el-timeline-item
              v-for="(acc, index) in selectedLead.assignedAccounts"
              :key="index"
              :timestamp="
                acc.role === AccountRoleTypes.Sales
                  ? `Sales Code: ${acc.code}`
                  : `IB Group: ${acc.group}`
              "
            >
              <span>{{ acc.name }} (Uid:{{ acc.uid }})</span>
            </el-timeline-item>
          </el-timeline>
          <div v-else class="text-gray-700">Lead not assigned yet</div>
        </el-collapse-item>
        <el-collapse-item name="2">
          <template #title>
            <div class="fs-6 fw-bold">{{ $t("title.updateHistory") }}</div>
          </template>
          <el-timeline>
            <el-timeline-item
              v-for="(cmt, index) in selectedLead.updateLogs"
              :key="index"
              :timestamp="moment(cmt.createdOn).format('YYYY-MM-DD HH:mm:ss')"
            >
              {{ cmt.content }}
            </el-timeline-item>
          </el-timeline>
        </el-collapse-item>
      </el-collapse>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import SimpleForm from "@/components/SimpleForm.vue";
import { ref, computed, inject } from "vue";
import {
  Iphone,
  Message,
  OfficeBuilding,
  Tickets,
  User,
} from "@element-plus/icons-vue";
import SalesService from "@/projects/client/modules/sales/services/SalesService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import TimeShow from "@/components/TimeShow.vue";
import { LeadStatusTypes } from "@/core/types/LeadStatusTypes";
import moment from "moment";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import ClientGlobalInjectionKeys from "@/core/types/ClientGlobalInjectionKeys";

const selectedLead = ref({} as any);

const size = ref("");
const iconStyle = computed(() => {
  const marginMap = {
    large: "8px",
    default: "6px",
    small: "4px",
  };
  return {
    marginRight: marginMap[size.value] || marginMap.default,
  };
});

const activeNames = ref(["1"]);

const modalRef = ref<any>();
const isLoading = ref(true);

const show = async (_lead) => {
  modalRef.value?.show();
  isLoading.value = true;
  activeNames.value = [];
  selectedLead.value = _lead;
  await getLeadDetails();
  isLoading.value = false;
};

const openCommentModal = inject(
  ClientGlobalInjectionKeys.OPEN_ADD_COMMENT_MODAL
);
const openAddedCommentModal = () => {
  openCommentModal?.((cmt: string) =>
    SalesService.addCommentToLead(selectedLead.value.id, {
      content: cmt,
    }).then(getLeadDetails)
  );
};

const getLeadDetails = async () => {
  try {
    isLoading.value = true;
    selectedLead.value = await SalesService.getLeadDetails(
      selectedLead.value.id
    );
  } catch (e) {
    MsgPrompt.error(e);
  } finally {
    isLoading.value = false;
  }
};

defineExpose({
  show,
  hide: () => {
    modalRef.value?.hide();
  },
});
</script>

<style scoped></style>
