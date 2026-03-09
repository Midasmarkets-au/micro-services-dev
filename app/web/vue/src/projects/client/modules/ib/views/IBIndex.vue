<template>
  <!--begin::Row-->

  <!--  <IBAccountsSelector />-->
  <IBLayout activeMenuItem="index">
    <template v-if="projectConfig?.rebateEnabled">
      <div class="row mb-5">
        <div class="col-12 col-md-3" :class="!isMobile ? 'h-375px' : ''">
          <IBWidgetSelector />
        </div>
        <template v-if="!isMobile">
          <div class="col-12 col-md-9">
            <div class="row h-100">
              <div class="col-12 col-md-6">
                <div class="h-50 pb-3">
                  <IBWidgetRebate />
                </div>
                <div class="h-50 pt-3">
                  <IBWidgetNewCustomer />
                </div>
              </div>
              <div class="col-12 col-md-6">
                <div class="h-50 pb-3">
                  <IBWidgetTradeVol />
                </div>
                <div class="h-50 pt-3">
                  <IBWidgetFunding />
                </div>
              </div>
            </div>
          </div>
        </template>
        <div v-else>
          <div class="mt-2 mb-2"><h2>Today's Report</h2></div>
          <div class="col-12 card mb-6">
            <div class="d-flex align-items-end justify-content-around py-4">
              <IBWidgetTradeVol />
              <IBWidgetNewCustomer />
              <IBWidgetFunding />
            </div>
          </div>
          <div class="col-12">
            <IBWidgetRebate />
          </div>
        </div>
      </div>
      <div class="row mb-4">
        <div class="col-12 h-450px col-md-3 mb-4 mb-lg-0">
          <IBWidgetIbLink />
        </div>
        <div class="col-12 h-450px col-md-9">
          <IBWidgetRebateChart />
        </div>
      </div>
      <div class="row">
        <div class="col-12 h-lg-450px col-md-4 mb-4 mb-lg-0">
          <IBWidgetCustomerList />
        </div>
        <div class="col-12 h-lg-450px col-md-4 mb-4 mb-lg-0">
          <IBWidgetLots />
        </div>
        <div class="col-12 h-450px col-md-4">
          <IBWidgetTopFundings />
        </div>
      </div>
    </template>
    <template v-else>
      <div class="row h-400px mb-5">
        <div class="col-12 col-md-4">
          <IBWidgetSelector />
        </div>
        <template v-if="!isMobile">
          <div class="col-12 col-md-4">
            <div class="row h-100">
              <div class="mb-5">
                <IBWidgetTradeVol />
              </div>
              <div class="">
                <IBWidgetFunding />
              </div>
            </div>
          </div>
        </template>
        <template v-if="isMobile">
          <div class="col-12 col-md-4">
            <div class="row h-100" style="padding: 10px">
              <div class="mb-5 col-6">
                <IBWidgetTradeVol />
              </div>
              <div class="col-6">
                <IBWidgetFunding />
              </div>
            </div>
          </div>
        </template>

        <div class="col-12 col-md-4">
          <IBWidgetCustomerList />
        </div>
      </div>

      <div class="row mb-4">
        <div class="col-12 h-450px col-md-4 mb-4 mb-lg-0">
          <IBWidgetIbLink />
        </div>
        <div class="col-12 h-lg-450px col-md-4 mb-4 mb-lg-0">
          <IBWidgetLots />
        </div>
        <div class="col-12 h-450px col-md-4">
          <IBWidgetTopFundings />
        </div>
      </div>
    </template>
  </IBLayout>
</template>

<script lang="ts" setup>
import { computed, ref, watch } from "vue";
import IBLayout from "../components/IBLayout.vue";
import { isMobile } from "@/core/config/WindowConfig";
import IBWidgetSelector from "../components/IBWidgetSelector.vue";
import IBWidgetRebate from "../components/IBWidgetRebate.vue";
import IBWidgetTradeVol from "../components/IBWidgetTradeVol.vue";
import IBWidgetNewCustomer from "../components/IBWidgetNewCustomer.vue";
import IBWidgetFunding from "../components/IBWidgetFunding.vue";
import IBWidgetIbLink from "../components/IBWidgetIbLink.vue";
import IBWidgetRebateChart from "../components/IBWidgetRebateChart.vue";
import IBWidgetCustomerList from "../components/IBWidgetNewCustomerList.vue";
import IBWidgetLots from "../components/IBWidgetLots.vue";
import IBWidgetTopFundings from "../components/IBWidgetTopFundings.vue";
import { PublicSetting } from "@/core/types/ConfigTypes";
import { useStore } from "@/store";

const isLoading = ref(true);

const store = useStore();

const agentAccount = ref(store.state.AgentModule.agentAccount);

const projectConfig = computed<PublicSetting>(
  () => store.state.AuthModule.config
);

watch(
  () => store.state.AgentModule.agentAccount,
  (newAgentAccount) => {
    isLoading.value = true;
    agentAccount.value = newAgentAccount;
    isLoading.value = false;
  }
);
</script>
<style scoped></style>
