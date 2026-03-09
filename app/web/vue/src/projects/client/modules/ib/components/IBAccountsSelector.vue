<template>
  <div class=""></div>
  <!-- <button class="btn btn-primary">{{ route.fullPath }}</button> -->
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from "vue";
import { useStore } from "@/store";
import { Actions } from "../store/StoreEnums";
import { useRouter, useRoute } from "vue-router";
import svc from "../services/IbService";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import TradeAccountCard from "@/components/TradeAccountCard.vue";
import AccountService from "@/projects/client/modules/accounts/services/AccountService";

const store = useStore();
const router = useRouter();
const route = useRoute();
const agentAccount = ref(store.state.AgentModule.agentAccount);
const ibAccounts = ref(Array<any>());

const isLoading = ref(true);
const showErrorMsg = ref(false);

watch(agentAccount, () => {
  if (agentAccount.value) showErrorMsg.value = false;
});

const changeAccountHandle = async (val) => {
  agentAccount.value = val;
  await changeAccount();
};

const changeAccount = async () => {
  if (!agentAccount.value) {
    showErrorMsg.value = true;
    return;
  }
  isLoading.value = true;
  const selectedAccount = ibAccounts.value.find(
    (x) => x.uid === agentAccount.value
  );
  await store.dispatch(Actions.PUT_IB_CURRENT_ACCOUNT, {
    agentUid: agentAccount.value,
    accountRole: selectedAccount?.role,
    code: selectedAccount?.code,
  });
  // console.log(store.state.IBModule);
  isLoading.value = false;
  await router.push(route.fullPath);
};

onMounted(async () => {
  isLoading.value = true;
  ibAccounts.value = await store.dispatch(Actions.GET_IB_ACCOUNT_LIST);
  if (ibAccounts.value.length === 0) {
    const res = await AccountService.queryAccounts({
      role: AccountRoleTypes.IB,
    });
    ibAccounts.value = res.data.map((x: any) => ({
      agentUid: x.uid,
      accountRole: x.role,
      type: x.type,
      name: x.name,
      createdOn: x.createdOn,
      code: x.code,
      alias: x.alias,
    }));
    store.dispatch(Actions.PUT_IB_ACCOUNTS, ibAccounts.value);
  }

  // Only for developing: set default account
  if (ibAccounts.value.length > 0) {
    changeAccountHandle(ibAccounts.value[0].uid);
  }
  isLoading.value = false;
});
</script>

<style scoped></style>
