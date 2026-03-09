<template>
  <div class="d-flex gap-10 justify-content-between">
    <CommonTabs />
    <div>
      <el-input
        class="w-250px"
        v-model="criteria.email"
        :clearable="true"
        placeholder="Email"
      >
        <template #append>
          <el-button :icon="Search" @click="search" :loading="isLoading" />
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
</template>
<script setup lang="ts">
import { inject, provide } from "vue";
import { VerificationStatusTypes } from "@/core/types/VerificationInfos";
import { Search } from "@element-plus/icons-vue";
import CommonTabs from "@/projects/tenant/components/CommonTabs.vue";
import { useI18n } from "vue-i18n";
const { t } = useI18n();
const isLoading = inject<any>("isLoading");
const criteria = inject<any>("criteria");
const search = inject<any>("search");
const reset = inject<any>("reset");

const tabsData = [
  {
    index: 0,
    label: t("status.pending"),
    status: VerificationStatusTypes.AwaitingReview,
  },
  {
    index: 1,
    label: t("status.awaitingIdVerify"),
    status: VerificationStatusTypes.AwaitingAddressVerify,
  },
  {
    index: 2,
    label: t("status.awaitingCodeVerify"),
    status: VerificationStatusTypes.AwaitingCodeVerify,
  },
  {
    index: 3,
    label: t("status.codeVerified"),
    status: VerificationStatusTypes.CodeVerified,
  },
  {
    index: 4,
    label: t("status.approved"),
    status: VerificationStatusTypes.Approved,
  },
  {
    index: 5,
    label: t("status.rejected"),
    status: VerificationStatusTypes.Rejected,
  },
  {
    index: 6,
    label: t("status.incomplete"),
    status: VerificationStatusTypes.Incomplete,
  },
  {
    index: 7,
    label: t("status.all"),
    status: null,
  },
];

provide("tabsData", tabsData);
</script>
