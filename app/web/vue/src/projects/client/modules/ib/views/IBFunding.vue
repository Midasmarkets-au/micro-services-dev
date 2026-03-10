<template>
  <div class="sub-menu d-flex">
    <router-link to="/ib" class="sub-menu-item">{{
      $t("title.overview")
    }}</router-link>
    <router-link to="/ib/new-customers" class="sub-menu-item">{{
      $t("title.newCustomers")
    }}</router-link>
    <router-link to="/ib/new-customers" class="sub-menu-item">{{
      $t("title.newCustomers")
    }}</router-link>
    <router-link to="/ib/customers" class="sub-menu-item">{{
      $t("title.customer")
    }}</router-link>
    <router-link to="/ib/trade" class="sub-menu-item">{{
      $t("title.trade")
    }}</router-link>
    <!--    <router-link to="/ib/funding" class="sub-menu-item active">{{-->
    <!--      $t("title.funding")-->
    <!--    }}</router-link>-->
    <router-link
      to="/ib/rebate"
      class="sub-menu-item"
      v-if="projectConfig?.rebateEnabled"
      >{{ $t("title.rebate") }}</router-link
    >

    <router-link to="/ib/iblink" class="sub-menu-item">{{
      $t("title.ibLink")
    }}</router-link>
  </div>
  <IBAccountsSelector v-if="!agentAccount" />

  <!--begin::Row-->
  <div v-if="agentAccount" class="row mb-5 mb-xl-10 mx-0">
    <!-- <IBTransactionTotal /> -->
    <!-- <IBDepositTotal /> -->
    <!-- <IBWithdrawTotal /> -->
  </div>
  <!--end::Row-->
</template>

<script lang="ts" setup>
import { computed } from "vue";
import { useStore } from "@/store";
import IBAccountsSelector from "../components/IBAccountsSelector.vue";
import { RouterLink } from "vue-router";
import { PublicSetting } from "@/core/types/ConfigTypes";
const store = useStore();
const projectConfig: PublicSetting = store.state.AuthModule.config;
const agentAccount = computed(() => store.state.AgentModule.agentAccount);
</script>
