<template>
  <IBAccountsSelector v-if="!agentAccount" />

  <!--begin::Row-->
  <div v-if="agentAccount" class="row mb-5 mb-xl-10 mx-0">
    <div class="card mb-1">
      <div class="card-header py-4">
        <h3 class="card-title align-items-start flex-column fw-bold fs-3">
          Referral
        </h3>
        <div class="card-toolbar">
          <!-- v-if="!pendingApplications" -->
          <!-- v-if="pendingApplications.length === 0" -->
          <button
            class="btn btn-sm btn-light"
            @click="showIBReferralLinkPanel('undefine')"
          >
            Generate Referral Link
          </button>
          <!-- <div v-else>
          {{ "Wait until the pending application to be processed" }}
        </div> -->
        </div>
      </div>
      <div class="card-body">
        <table
          class="table align-middle table-row-bordered gy-5"
          id="table_accounts_requests"
        >
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th>Account ID</th>
              <th>Name</th>
              <th>Referral Code</th>
              <th>Updated</th>
              <th>Created</th>
              <th class="text-center min-w-150px">
                {{ $t("action.action") }}
              </th>
            </tr>
          </thead>

          <tbody v-if="isLoading">
            <tr>
              <td colspan="8">{{ $t("status.loading") }}</td>
            </tr>
          </tbody>

          <tbody v-else-if="!isLoading && items.length == 0">
            <tr>
              <td colspan="8">{{ $t("tip.nodata") }}</td>
            </tr>
          </tbody>

          <tbody v-else>
            <tr v-for="(item, index) in items" :key="index">
              <td>{{ item.accountId }}</td>
              <td>{{ item.name }}</td>
              <td>{{ item.code }}</td>
              <td><TimeShow :date-iso-string="item.updatedOn" /></td>
              <td><TimeShow :date-iso-string="item.createdOn" /></td>
              <td class="text-center">
                <button
                  class="btn btn-light btn-success btn-sm me-3"
                  data-kt-menu-trigger="click"
                  data-kt-menu-placement="bottom-end"
                  @click="showIBReferralLinkPanel(item.code)"
                >
                  View
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
  <!--end::Row-->

  <IBReferralLinkModal ref="IBReferralLinkRef" @fetch-data="fetchData" />
</template>

<script lang="ts" setup>
import "../assets/css/style.css";

import IBService from "../services/IbService";
import TimeShow from "@/components/TimeShow.vue";
import { ref, computed, onMounted } from "vue";
import IBAccountsSelector from "../components/IBAccountsSelector.vue";
import IBReferralLinkModal from "../components/IBReferralLink.vue";
import { useStore } from "@/store";
import { moibleNavScroller } from "@/core/utils/mobileNavScroller";

const store = useStore();
const isLoading = ref(true);
const items = ref(Array<any>());
const IBReferralLinkRef = ref<InstanceType<typeof IBReferralLinkModal>>();
const agentAccount = computed(() => store.state.AgentModule.agentAccount);

const showIBReferralLinkPanel = (_code?: string) => {
  IBReferralLinkRef.value?.show(agentAccount.value, _code);
};

const fetchData = async () => {
  isLoading.value = true;

  try {
    const response = await IBService.getReferralCode({ status: 0 });
    items.value = response.data;

    isLoading.value = false;
  } catch (error) {
    // console.log(error);
  }
};

onMounted(async () => {
  moibleNavScroller(".ib-menu", ".scroll-to");

  await fetchData();
});
</script>
