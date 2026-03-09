<template>
  <el-menu
    :default-active="currentTab"
    class="mb-5"
    mode="horizontal"
    @select="handleSelect"
  >
    <el-menu-item :index="VerificationTab.Info">{{
      $t("fields.info")
    }}</el-menu-item>

    <el-menu-item :index="VerificationTab.Financial">{{
      $t("fields.financial")
    }}</el-menu-item>

    <el-menu-item :index="VerificationTab.Agreement">{{
      $t("fields.agreement")
    }}</el-menu-item>
    <el-menu-item :index="VerificationTab.Review">{{
      $t("title.review")
    }}</el-menu-item>
    <el-menu-item :index="VerificationTab.PDF">PDF</el-menu-item>
    <el-menu-item :index="VerificationTab.VerificationCode">{{
      $t("fields.authenticationNumber")
    }}</el-menu-item>
  </el-menu>
</template>
<script setup lang="ts">
import { ref, inject } from "vue";

const VerificationTab = inject<any>("VerificationTab");
const currentTab = inject<any>("currentTab");
const handleSelect = (VerificationTab) => {
  currentTab.value = VerificationTab;
};
</script>
