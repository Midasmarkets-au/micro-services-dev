<template>
  <div class="sub-menu sub-h2 d-inline-flex" style="white-space: nowrap">
    <router-link to="/supports" class="sub-menu-item">{{
      $t("title.contactUs")
    }}</router-link>
    <router-link to="/supports/notices" class="sub-menu-item">{{
      $t("title.notices")
    }}</router-link>
    <router-link to="/supports/documents" class="sub-menu-item">{{
      $t("title.documents")
    }}</router-link>
    <router-link
      v-if="$cans(['TenantAdmin'])"
      to="/supports/cases"
      class="sub-menu-item active"
      >{{ $t("title.cases") }}</router-link
    >
  </div>
  <div class="card">
    <div class="card-header">
      <div class="card-title">Support Cases</div>
      <div class="card-toolbar">
        <button
          class="btn btn-success btn-sm btn-radius me-4"
          v-if="currentView != ClientView.CaseList"
          @click="changeView(ClientView.CaseList)"
        >
          Return
        </button>
        <button
          v-if="currentView != ClientView.CreateCase"
          class="btn btn-primary btn-sm btn-radius"
          @click="changeView(ClientView.CreateCase)"
        >
          {{ $t("action.create") }}
        </button>
      </div>
    </div>
    <CaseList v-if="currentView == ClientView.CaseList" />
    <CreateCase v-else-if="currentView == ClientView.CreateCase" />
    <CaseDetail v-else-if="currentView == ClientView.CaseDetail" />
  </div>
</template>
<script lang="ts" setup>
import { ref, provide } from "vue";
import CaseList from "../components/Case/CaseList.vue";
import CreateCase from "../components/Case/CreateCase.vue";
import CaseDetail from "../components/Case/CaseDetail.vue";
import { ClientView } from "@/core/types/SupportStatusTypes";

const currentView = ref(ClientView.CaseList);
const item = ref(<any>[]);
const changeView = (status: ClientView) => {
  currentView.value = status;
  console.log("changeView", currentView.value);
};

provide("changeView", changeView);
provide("item", item);
</script>
