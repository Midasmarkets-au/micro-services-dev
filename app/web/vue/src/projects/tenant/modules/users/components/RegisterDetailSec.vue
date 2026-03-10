<template>
  <DropDownBox :title="title" :show="show">
    <DetailInput
      :label="$t('label.userId')"
      :value="userId"
      :disabled="true"
      :size="10"
    />
    <DetailInput
      :label="$t('label.userName')"
      :value="userName"
      :disabled="true"
      :size="10"
    />
    <div>
      <div>{{ $t("label.userStatue") }}</div>
      <div class="badge py-3 px-4 fs-7 badge-light-primary">In Process</div>
      <div class="badge py-3 px-4 fs-7 badge-light-warning">On Hold</div>
      <div class="badge py-3 px-4 fs-7 badge-light-success">Complated</div>
    </div>
    <DetailInput
      :label="$t('label.agentAccount')"
      :value="`N/A`"
      :disabled="true"
      :size="10"
    />
    <DetailInput
      :label="$t('label.statusNote')"
      :value="statusNote"
      :disabled="false"
      :size="10"
    />
  </DropDownBox>
</template>

<script setup lang="ts">
import DetailInput from "./DetailInput.vue";
import DropDownBox from "@/components/DropDownBox.vue";

withDefaults(
  defineProps<{
    userId: string | number;
    nameuserName: string | number;
    userStatus: string | number;
    isAgentAccount: boolean;
    statusNote: string | number;
    title: string | number;
    show: boolean;
  }>(),
  {
    show: false,
  }
);
</script>

<style scoped></style>
