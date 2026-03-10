<template>
  <IBLayout activeMenuItem="newCustomers">
    <IBAccountsSelector v-if="!agentAccount" />
    <div v-if="agentAccount" class="h-full flex-1">
      <div v-if="isMobile">
        <NewCustomersMobile />
      </div>
      <div v-else class="h-full">
        <div
          class="card card-flush h-full d-flex flex-column"
          v-if="accountUid === -1"
        >
          <div class="card-header">
            <div class="card-title">{{ $t("title.incompleteCustomers") }}</div>
            <div class="card-toolbar"></div>
          </div>

          <div
            class="card-body overflow-auto flex-1"
            style="white-space: nowrap"
          >
            <table class="table align-middle table-row-bordered gy-3">
              <thead>
                <tr class="text-center gs-0">
                  <th class="text-start" width="*">
                    {{ $t("fields.customer") }}
                  </th>

                  <th class="text-start">{{ $t("title.email") }}</th>

                  <th class="text-start">{{ $t("fields.createdOn") }}</th>
                  <th class="text-start">{{ $t("fields.status") }}</th>
                </tr>
              </thead>

              <tbody v-if="isLoading">
                <LoadingRing />
              </tbody>
              <tbody v-else-if="!isLoading && incompleteCustomers.length === 0">
                <NoDataBox />
              </tbody>

              <tbody v-else>
                <tr v-for="(item, index) in incompleteCustomers" :key="index">
                  <td class="">
                    <div class="d-flex align-items-center">
                      <UserAvatar
                        :avatar="item.user?.avatar"
                        :name="item.user?.displayName"
                        size="40px"
                        class="me-3"
                        side="client"
                        rounded
                      />

                      <span>
                        {{ item.user?.displayName }}
                      </span>
                    </div>
                  </td>
                  <td class="text-start">
                    {{ item.user?.email }}
                  </td>

                  <td class="text-start">
                    <TimeShow
                      :date-iso-string="item.verification.updatedOn"
                      type="inFields"
                    />
                  </td>
                  <td class="text-start">
                    <template v-if="!item.verification.isEmpty">
                      {{
                        $t("title.verification") +
                        " " +
                        $t(
                          `type.verificationStatus.${item.verification.status}`
                        ).toLowerCase()
                      }}
                    </template>

                    <template v-else>
                      {{
                        $t("title.verification") +
                        " " +
                        $t(`status.notStarted`).toLowerCase()
                      }}
                    </template>
                  </td>
                </tr>
              </tbody>
            </table>
            <TableFooter @page-change="pageChange" :criteria="criteria" />
          </div>
        </div>
      </div>
      <IBCustomerDetail
        v-if="accountUid !== -1"
        :service-map="serviceMap"
        :customer-accounts="customerAccounts"
      />
    </div>
  </IBLayout>
</template>

<script lang="ts" setup>
import { useRoute } from "vue-router";
import IBLayout from "../components/IBLayout.vue";
//import headerMenu from "../components/menu/headerMenu.vue";
import { useStore } from "@/store";
import { ref, watch, onMounted, computed, provide } from "vue";
import IBCustomerDetail from "./IBCustomerDetail.vue";
import IBAccountsSelector from "../components/IBAccountsSelector.vue";
import IbService from "../services/IbService";
import GlobalService from "@/projects/client/services/ClientGlobalService";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import TimeShow from "@/components/TimeShow.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import TableFooter from "@/components/TableFooter.vue";
import { PublicSetting } from "@/core/types/ConfigTypes";
import { moibleNavScroller } from "@/core/utils/mobileNavScroller";
import { isMobile } from "@/core/config/WindowConfig";
import NewCustomersMobile from "../components/newCustomers/NewCustomersMobile.vue";

const store = useStore();
const route = useRoute();

const ibClientSwitch = ref(false);
const accountUid = computed(
  () => parseInt(route.params.accountId as string) || -1
);
const isLoading = ref(true);

const incompleteCustomers = ref(Array<any>());
const customerAccounts = ref();
const serviceMap = ref();
const projectConfig: PublicSetting = store.state.AuthModule.config;

provide("data", incompleteCustomers);
provide("isLoading", isLoading);
const criteria = ref({
  page: 1,
  size: 10,
  IsUnverified: true,
} as any);

const agentAccount = ref(store.state.AgentModule.agentAccount);

watch([() => store.state.AgentModule.agentAccount, ibClientSwitch], () => {
  agentAccount.value = store.state.AgentModule.agentAccount;
  customerAccounts.value = [];
  criteria.value = {
    page: 1,
    size: 10,
    IsUnverified: true,
  };
  if (agentAccount.value) {
    fetchData(criteria.value.page);
  }
});

const fetchData = async (selectedPage: number) => {
  isLoading.value = true;

  criteria.value.page = selectedPage;
  try {
    const res = await IbService.queryAgentClientReferralHistory(criteria.value);
    incompleteCustomers.value = res.data;
    criteria.value = res.criteria;
    isLoading.value = false;
  } catch (error: any) {
    MsgPrompt.error(error);
  }
};

const pageChange = (page: number) =>
  page !== criteria.value.page && fetchData(page);

onMounted(async () => {
  if (agentAccount.value) {
    await fetchData(criteria.value.page);
  }
  serviceMap.value = await GlobalService.getServiceMap();
  moibleNavScroller(".ib-menu", ".scroll-to");
});
</script>

<style scoped lang="scss">
.svg-container {
  transition: transform 0.3s ease-in-out;
}

.arrow.rotate-up .svg-container {
  transform: rotate(-180deg);
}

.collapse-content {
  max-height: 0;
  overflow: hidden;
  transition: max-height 0.3s ease-in-out;
}
</style>
