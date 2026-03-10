<template>
  <div
    class="d-flex align-right"
    v-if="showRepSelector && repAccounts.length >= 1"
  >
    <button
      class="btn btn-small me-2 cursor-pointer"
      :class="{
        'btn-light': repAccount.uid === item.uid,
        'btn-primary': repAccount.uid !== item.uid,
      }"
      v-for="item in repAccounts"
      :key="item.uid"
      @click="changeRepAccount(item.uid)"
    >
      {{ item.uid }}
    </button>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from "vue";
import { useStore } from "@/store";
import { useRoute } from "vue-router";
import AccountService from "../../accounts/services/AccountService";
import { Actions } from "@/store/enums/StoreEnums";
import { AccountRoleTypes } from "@/core/types/AccountInfos";

const store = useStore();
const route = useRoute();

const repAccount = ref(store.state.RepModule.repAccount);
const repAccounts = ref([]);
onMounted(async () => {
  // console.log("mounted");
  const res = await AccountService.queryAccounts({
    role: AccountRoleTypes.Rep,
  });
  if (res.data.length === 0) return;
  repAccounts.value = res.data;

  if (repAccounts.value.length > 0) {
    await store.dispatch(Actions.PUT_REP_ACCOUNT, {
      uid: res.data[0].uid,
    });
  }
  repAccount.value = {
    uid: res.data[0].uid,
  };
});

const showRepSelector = computed(() => /^\/rep/.test(route.path));

const changeRepAccount = (uid: number) => {
  if (uid === repAccount.value.uid) return;
  store.dispatch(Actions.PUT_REP_ACCOUNT, {
    uid,
  });
  repAccount.value = { uid };
};
</script>
