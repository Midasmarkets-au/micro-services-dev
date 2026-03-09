<template>
  <div
    class="d-flex overflow-auto align-right"
    v-if="showSalesSelector && salesAccounts.length > 1"
  >
    <button
      class="btn btn-small me-2 cursor-pointer"
      :class="{
        'btn-primary': salesAccount.uid === item.uid,
        'btn-light': salesAccount.uid !== item.uid,
        'mobile-btn': isMobile === true,
      }"
      v-for="item in salesAccounts"
      :key="item.uid"
      @click="
        changeSalesAccount(item.uid, item.salesSelfGroupName, item.siteId)
      "
    >
      {{ item.uid }}({{ $t("type.siteType." + item.siteId) }}) {{ item.alias }}
    </button>
    <button class="btn btn-small cursor-pointer" @click="editAlias">
      <div>
        <i class="fa fa-edit"></i>
      </div>
    </button>
  </div>
  <EditAlias ref="EditAliasRef" @update="fetchData" />
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from "vue";
import { useStore } from "@/store";
import { useRoute } from "vue-router";
import AccountService from "../../accounts/services/AccountService";
import { Actions } from "@/store/enums/StoreEnums";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import EditAlias from "@/projects/client/modules/sales/components/modal/EditAlias.vue";
import { isMobile } from "@/core/config/WindowConfig";
const store = useStore();
const route = useRoute();
const EditAliasRef = ref(null);
const salesAccount = ref(store.state.SalesModule.salesAccount);
const salesAccounts = ref([]);
const defaultSalesAccount = store.state.AuthModule.user?.defaultSalesAccount;

onMounted(async () => {
  const res = await AccountService.queryAccounts({
    role: AccountRoleTypes.Sales,
  });
  if (res.data.length === 0) return;
  salesAccounts.value = res.data;

  if (salesAccounts.value.length > 0 && !salesAccount.value) {
    if (defaultSalesAccount && defaultSalesAccount !== "0") {
      var defaultAccount = salesAccounts.value.find(
        (item) => item.uid == defaultSalesAccount
      );
      if (defaultAccount) {
        await store.dispatch(Actions.PUT_SALES_ACCOUNT, {
          uid: defaultAccount.uid,
          salesSelfGroupName: defaultAccount.code,
          siteId: defaultAccount.siteId,
        });
        salesAccount.value = {
          uid: defaultAccount.uid,
          salesSelfGroupName: defaultAccount.code,
        };
        return;
      }
    }

    await store.dispatch(Actions.PUT_SALES_ACCOUNT, {
      uid: res.data[0].uid,
      siteId: res.data[0].siteId,
      salesSelfGroupName: res.data[0].code,
    });
    salesAccount.value = {
      uid: res.data[0].uid,
      salesSelfGroupName: res.data[0].code,
    };
  }
});

const fetchData = async () => {
  const res = await AccountService.queryAccounts({
    role: AccountRoleTypes.Sales,
  });
  salesAccounts.value = res.data;
};

const showSalesSelector = computed(() => /^\/sales/.test(route.path));

const changeSalesAccount = (
  uid: number,
  salesSelfGroupName: string,
  siteId: number
) => {
  if (uid == salesAccount.value?.uid) return;
  store.dispatch(Actions.PUT_SALES_ACCOUNT, {
    uid: uid,
    salesSelfGroupName: salesSelfGroupName,
    siteId: siteId,
  });
  salesAccount.value = { uid };
  window.location.reload();
};

const editAlias = () => {
  EditAliasRef.value.show(salesAccounts.value);
};
</script>
<style scoped type="scss">
.fa-edit:hover {
  color: #ffce00;
}
.mobile-btn {
  font-size: 10px !important;
  padding: 1px 5px !important;
}
</style>
