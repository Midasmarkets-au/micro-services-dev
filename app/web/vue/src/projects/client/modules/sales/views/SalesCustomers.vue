<template>
  <!-- <div v-if="!salesAccount">{{ $t("action.noSalesAccount") }}</div>
    <div v-else>
      <SalesCenterMenu activeMenuItem="customers" />
    </div> -->
  <SalesLayout activeMenuItem="customers">
    <div class="card mb-2 round-bl-br" v-if="accountUid === -1">
      <div class="card-header">
        <SalesFilter />
      </div>
    </div>
    <div
      :class="accountUid === -1 ? 'card round-tl-tr flex-1' : ''"
      v-if="!isMobile"
    >
      <div
        class="card-header"
        v-if="ibLevelTree.length > 0 && accountUid === -1"
      >
        <div class="card-title">
          <SalesLevel />
        </div>
      </div>
      <div class="card-body overflow-auto" style="white-space: nowrap">
        <SalesTable />
      </div>
    </div>
    <div v-else>
      <div class="card mb-3" v-if="ibLevelTree.length > 0 && accountUid === -1">
        <div class="card-header">
          <div class="card-title">
            <SalesLevel />
          </div>
        </div>
      </div>
      <div class="card card-body">
        <SalesCustomerMobile />
      </div>
    </div>
  </SalesLayout>
</template>

<script setup lang="ts">
import { useStore } from "@/store";
import { useRoute } from "vue-router";
import SalesService from "../services/SalesService";
import SalesCenterMenu from "../components/SalesCenterMenu.vue";
import SalesTable from "../components/SalesCustomers/SalesTable.vue";
import SalesLevel from "../components/SalesCustomers/SalesLevel.vue";
import SalesFilter from "../components/SalesCustomers/SalesFilter.vue";
import { computed, onMounted, ref, watch, provide } from "vue";
import GlobalService from "@/projects/client/services/ClientGlobalService";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import { isMobile } from "@/core/config/WindowConfig";
import SalesCustomerMobile from "../components/modal/SalesCustomerMobile.vue";
import SalesLayout from "../components/SalesLayout.vue";
const store = useStore();
const route = useRoute();
const salesAccount = computed(() => store.state.SalesModule.salesAccount);
const isLoading = ref(true);
const data = ref(<any>[]);
const productCategory = ref(Array<any>());
const serviceMap = ref({} as any);
const currentRole = ref(AccountRoleTypes.IB);
const isDeposit = ref(null);
const multiLevel = ref(false);
const ibLevelTree = ref(<any>[]);
const accountUid = computed(
  () => parseInt(route.params.accountId as string) || -1
);

const criteria = ref({
  page: 1,
  size: 30,
  role: currentRole.value,
  multiLevel: multiLevel.value,
  sortField: "createdOn",
  sortFlag: true,
  searchText: null,
} as any);

const goToIbLevel = async (_ibAccount) => {
  criteria.value.parentAccountUid = _ibAccount.uid;
  criteria.value.searchText = null;
  await fetchData(1);
};

const IbSearch = async (_ibAccount) => {
  criteria.value.parentAccountUid = _ibAccount.uid;
  criteria.value.searchText = null;
  await fetchData(1);
};

const resetIbTree = async () => {
  ibLevelTree.value = [];
  criteria.value = {
    page: 1,
    size: 30,
    role: currentRole.value,
    multiLevel: multiLevel.value,
    sortField: "createdOn",
    sortFlag: true,
    searchText: null,
  };
  await fetchData(1);
};

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const res = await SalesService.queryAccounts(criteria.value);
    criteria.value = res.criteria;
    data.value = res.data;
    ibLevelTree.value =
      criteria.value.levelAccountsInBetween === null
        ? []
        : criteria.value.levelAccountsInBetween;
  } catch (e) {
    console.log(e);
  }
  isLoading.value = false;
};

const fetchCategory = async () => {
  try {
    productCategory.value = await SalesService.getCategory();
  } catch (e) {
    console.log(e);
  }
};

const fetchServiceMap = async () => {
  try {
    serviceMap.value = await GlobalService.getServiceMap();
  } catch (e) {
    console.log(e);
  }
};

const getUserName = (item: any) => {
  if (
    !item?.user?.nativeName ||
    item?.user?.nativeName === "" ||
    item?.user?.nativeName === " "
  ) {
    if (
      !item?.user?.displayName ||
      item?.user?.displayName === "" ||
      item?.user?.displayName === " "
    ) {
      if (
        !item?.user?.firstName ||
        !item?.user?.lastName ||
        item?.user?.firstName === "" ||
        item?.user?.lastName === "" ||
        item?.user?.firstName === " " ||
        item?.user?.lastName === " "
      ) {
        return "No Name";
      } else {
        return item?.user?.firstName + " " + item?.user?.lastName;
      }
    } else {
      return item?.user?.displayName;
    }
  } else {
    return item?.user?.nativeName;
  }
};

provide("data", data);
provide("isLoading", isLoading);
provide("criteria", criteria);
provide("currentRole", currentRole);
provide("productCategory", productCategory);
provide("serviceMap", serviceMap);
provide("fetchData", fetchData);
provide("accountUid", accountUid);
provide("getUserName", getUserName);
provide("isDeposit", isDeposit);

// ib levels
provide("multiLevel", multiLevel);
provide("ibLevelTree", ibLevelTree);
provide("IbSearch", IbSearch);
provide("goToIbLevel", goToIbLevel);
provide("resetIbTree", resetIbTree);

watch(currentRole, (newValue) => {
  criteria.value.role = newValue;
  isDeposit.value = null;
  fetchData(1);
});

watch(multiLevel, (newValue) => {
  criteria.value.multiLevel = newValue;
  fetchData(1);
});
watch(salesAccount, async (newVal, oldVal) => {
  if (newVal.uid !== oldVal.uid) {
    await fetchData(1);
  }
});

onMounted(async () => {
  isLoading.value = true;
  await fetchData(1);
  await fetchCategory();
  await fetchServiceMap();
  isLoading.value = false;
});
</script>
