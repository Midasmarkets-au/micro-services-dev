<template>
  <div
    class="d-flex align-right"
    v-if="showIbSelector && ibAccounts.length > 1 && !isMobile"
  >
    <!-- <button
      class="btn btn-small me-2 cursor-pointer"
      v-for="item in ibAccounts"
      :key="item.uid"
    >
      {{ item.uid }}
    </button> -->
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from "vue";
import { useStore } from "@/store";
import { useRoute } from "vue-router";
import AccountService from "../../accounts/services/AccountService";
import { Actions } from "@/store/enums/StoreEnums";
import { AgentAccount } from "@/projects/client/modules/ib/store/IbModule";
import { FundTypes } from "@/core/types/FundTypes";
import { CurrencyTypes } from "@/core/types/CurrencyTypes";
import { isMobile } from "@/core/config/WindowConfig";
const store = useStore();
const route = useRoute();

const ibAccounts = ref(Array<AgentAccount>());
const ibAccountUids = ref(store.state.AuthModule.user.ibAccount);
const agentAccount = ref(store.state.AgentModule.agentAccount);
const defaultIbAccount = ref(store.state.AuthModule.user.defaultAgentAccount);

onMounted(async () => {
  const res = await AccountService.queryAccounts({
    uids: ibAccountUids.value,
  });

  ibAccounts.value = res.data.map(
    (x): AgentAccount => ({
      uid: x.uid,
      currencyId: x.currencyId as CurrencyTypes,
      fundType: x.fundType as FundTypes,
      role: x.role,
      type: x.type,
      name: x.name,
      siteId: x.siteId,
      hasLevelRule: x.hasLevelRule,
      salesGroupName: x.code,
      createdOn: x.createdOn,
      agentSelfGroupName: x.group,
      alias: x.alias,
    })
  );

  if (agentAccount.value?.uid) return;
  var ibCurrentAccount = ibAccounts.value[0];
  if (defaultIbAccount.value && defaultIbAccount.value !== "0") {
    ibCurrentAccount = ibAccounts.value.find(
      (x) => x.uid == defaultIbAccount.value
    );
  }

  await store.dispatch(Actions.PUT_IB_ACCOUNTS, ibAccounts.value);
  await store.dispatch(Actions.PUT_IB_CURRENT_ACCOUNT, ibCurrentAccount);
  console.log("");
});

const showIbSelector = computed(() => 1 || /^\/ib/.test(route.path));
</script>
