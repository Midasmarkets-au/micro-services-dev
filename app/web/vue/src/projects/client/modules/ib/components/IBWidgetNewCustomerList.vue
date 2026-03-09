<template>
  <div class="card h-100">
    <div class="card-header card-header-bottom">
      <div class="card-title-noicon fs-2">
        {{ $t("title.newCustomers") }}
      </div>
      <div class="card-toolbar">
        <router-link to="/ib/new-customers" class="view-more"
          >{{ $t("action.viewMore") }}
          <inline-svg
            src="/images/icons/general/show_more.svg"
            class="ml-2"
          ></inline-svg>
        </router-link>
      </div>
    </div>
    <!-- <div class="separator mx-auto" style="width: 85%"></div> -->
    <div class="h-100">
      <table
        v-if="isLoading || incompleteCustomers.length === 0"
        class="table align-middle fs-6 gy-5 h-100"
      >
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && incompleteCustomers.length === 0">
          <NoDataBox />
        </tbody>
      </table>

      <div v-else class="d-flex flex-column">
        <div
          v-for="(item, index) in incompleteCustomers"
          :key="index"
          class="d-flex justify-content-between align-items-center px-10 py-6"
          style="flex: 1"
        >
          <div class="d-flex align-items-center">
            <div class="customer-avatar">
              <UserAvatar
                :avatar="item?.user?.avatar"
                :name="item.user?.displayName"
                size="32px"
                class="me-3"
                side="client"
              />
            </div>

            <span class="text-black ms-3">
              {{
                item?.user.displayName?.substring(0, 11) +
                (item?.user.displayName.length > 11 ? "..." : "")
              }}
            </span>
            <span
              v-if="false"
              class="ms-3 custome-badge"
              :class="{
                'ib-badge':
                  item.role === AccountRoleTypes.IB ||
                  item.role === AccountRoleTypes.Sales,
                'client-badge': item.role === AccountRoleTypes.Client,
              }"
              >{{ $t(`type.accountRole.${item?.role}`) }}</span
            >
          </div>
          <div class="">
            <span
              class="text-end d-flex align-items-center gap-1 custome-badge normal-badge"
              v-if="!item.verification.isEmpty"
            >
              {{
                $t("title.verification") +
                " " +
                $t(
                  `type.verificationStatus.${item.verification.status}`
                ).toLowerCase()
              }}
            </span>
            <span class="text-end custome-badge normal-badge" v-else>
              {{
                $t("title.verification") +
                " " +
                $t(`status.notStarted`).toLowerCase()
              }}
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import IbService from "../services/IbService";

import { useStore } from "@/store";
import { ref, watch, onMounted } from "vue";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import MsgPrompt from "@/core/plugins/MsgPrompt";

const store = useStore();

const isLoading = ref(true);

const incompleteCustomers = ref();

const criteria = ref({
  page: 1,
  size: 5,
  IsUnverified: true,
} as any);

const agentAccount = ref(store.state.AgentModule.agentAccount);

watch(
  () => store.state.AgentModule.agentAccount,
  () => {
    agentAccount.value = store.state.AgentModule.agentAccount;
    incompleteCustomers.value = [];
    criteria.value = {
      page: 1,
      size: 5,
    };
    if (agentAccount.value) {
      fetchData(criteria.value.page);
    }
  }
);

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

onMounted(async () => {
  // console.log("route list accountid: ", route.params.accountId);
  if (agentAccount.value) {
    await fetchData(criteria.value.page);
  }
});
</script>

<style lang="scss" scoped>
.customer-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  overflow: hidden;
}

.custome-badge {
  padding: 1px 10px;
  border-radius: 10px;
  font-size: 12px;
}

.client-badge {
  color: #0a46aa;
  background: rgba(138, 195, 255, 0.18);
}

.ib-badge {
  color: #ff9900;
  background: rgba(255, 212, 0, 0.2);
}

.imcomplete-badge {
  padding: 3px 15px;
  border-radius: 16px;
  color: #e02b1d;
  background: rgba(224, 43, 29, 0.13);
}

.normal-badge {
  color: #009262;
  padding: 4px 15px;
  border-radius: 16px;
  background: rgba(0, 146, 98, 0.07);
}
</style>
