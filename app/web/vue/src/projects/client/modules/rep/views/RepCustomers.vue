<template>
  <div>
    <div v-if="!repAccount">{{ $t("action.noRepAccount") }}</div>
    <div v-else>
      <div class="overflow-auto">
        <div class="sub-menu d-flex">
          <router-link to="/rep" class="sub-menu-item active">{{
            $t("title.customer")
          }}</router-link>
          <router-link to="/rep/trade" class="sub-menu-item">{{
            $t("title.trade")
          }}</router-link>
          <router-link to="/rep/transaction" class="sub-menu-item">{{
            $t("title.transfer")
          }}</router-link>
          <router-link to="/rep/deposit" class="sub-menu-item">{{
            $t("title.deposit")
          }}</router-link>
          <router-link to="/rep/withdrawal" class="sub-menu-item">{{
            $t("title.withdrawal")
          }}</router-link>
          <!-- <router-link to="/rep/lead" class="sub-menu-item">{{
            $t("title.repLeadSystem")
          }}</router-link> -->
        </div>
      </div>
    </div>
    <div v-if="repAccount">
      <div class="card mb-5" v-if="accountUid === -1">
        <div class="card-header">
          <div class="card-title">
            <div>{{ $t("title.accounts") }}</div>
          </div>

          <div class="card-toolbar">
            <el-radio-group
              v-if="criteria.role != 100"
              v-model="criteria.role"
              label="label position"
              text-color=""
              text-hover-color="#0053ad"
              fill="#0053ad"
              @change="fetchData(1)"
            >
              <el-radio-button
                :label="AccountRoleTypes.IB"
                :value="AccountRoleTypes.IB"
                >IB</el-radio-button
              >
              <el-radio-button
                :label="AccountRoleTypes.Client"
                :value="AccountRoleTypes.Client"
                >Client</el-radio-button
              >
            </el-radio-group>
            <SearchFilter
              class="ms-3"
              :custom-search-handler="handleRepSearch"
              @get-results-ids="handleSearchResults"
              :defaultCriteria="criteria"
              :search-trigger="searchTrigger"
              :place-holder="$t('tip.emailOrAccountNumber')"
              multiple-selection
            />
          </div>
        </div>

        <div
          class="card-header"
          v-if="selectedIbAccountsChain && selectedIbAccountsChain.length > 0"
        >
          <div class="d-flex align-items-center">
            <div v-for="(acc, idx) in selectedIbAccountsChain" :key="idx">
              <div class="ps-3 fs-4 d-flex align-items-center gap-2">
                <div class="position-relative">
                  <div
                    class="cursor-pointer text-hover-primary"
                    @click="goToIbLevel(idx)"
                  >
                    {{ acc.user?.nativeName || acc.user?.displayName }}
                  </div>
                  <div class="level-tool-tip">Lv{{ idx + 1 }}</div>
                </div>
                <span class="fw-bold fs-2 text-gray-600">/</span>
              </div>
            </div>

            <button
              @click="clearIbSearch"
              class="ms-4 btn btn-sm btn-primary return-btn text-nowrap"
            >
              {{ $t("action.clear") }}
            </button>
          </div>
        </div>
        <div class="card-body">
          <table class="table align-middle table-row-bordered gy-5">
            <thead>
              <tr class="text-start gs-0">
                <th>{{ $t("fields.customer") }}</th>
                <th class="text-start">
                  <template v-if="isSelectingClient">{{
                    $t("fields.accountNo")
                  }}</template>
                  <template v-else>{{ $t("fields.uid") }}</template>
                </th>
                <th class="text-start">{{ $t("title.email") }}</th>
                <th class="text-start">{{ $t("fields.type") }}</th>
                <th class="text-start">{{ $t("fields.group") }}</th>
                <th class="text-start">{{ $t("fields.code") }}</th>
                <th class="text-start">{{ $t("fields.createdOn") }}</th>
                <th class="text-start" v-if="isSelectingClient">
                  {{ $t("fields.balance") }}
                </th>
                <th class="text-start">{{ $t("fields.actions") }}</th>
              </tr>
            </thead>
            <tbody v-if="isLoading">
              <LoadingRing />
            </tbody>
            <tbody v-else-if="!isLoading && accountsUnderRep?.length === 0">
              <NoDataBox />
            </tbody>
            <tbody v-else>
              <tr v-for="(item, index) in accountsUnderRep" :key="index">
                <td>
                  <div class="d-md-flex align-items-center">
                    <UserAvatar
                      :avatar="item.user.avatar"
                      :name="item.user.displayName"
                      size="40px"
                      class="me-3"
                      side="client"
                      rounded
                    />
                    <span>
                      {{
                        item.user?.nativeName ||
                        item.user?.displayName ||
                        item.user?.firstName + " " + item.user?.lastName
                      }}
                    </span>
                  </div>
                </td>
                <td>
                  <span class="text-start">{{
                    item.role === AccountRoleTypes.Client
                      ? item?.tradeAccount?.accountNumber
                      : item?.uid
                  }}</span>
                </td>
                <td class="text-start">{{ item.user.email }}</td>
                <td class="text-start">
                  {{ $t(`type.account.${item.type}`) }}
                </td>
                <td class="text-start">{{ item.group }}</td>
                <td class="text-start">{{ item.code }}</td>
                <td class="text-start">
                  <TimeShow :date-iso-string="item.createdOn" type="inFields" />
                </td>
                <td class="text-start" v-if="isSelectingClient">
                  <BalanceShow
                    :balance="item?.tradeAccount?.balanceInCents"
                    :currency-id="item.currencyId"
                  />
                </td>
                <td class="text-start">
                  <router-link
                    v-if="criteria.role == AccountRoleTypes.Client"
                    :to="'/rep/customers/' + item.uid"
                    style="color: #7c8fa2"
                  >
                    {{ $t("action.viewDetails") }}
                  </router-link>
                  <span
                    v-else
                    class="cursor-pointer"
                    style="color: #7c8fa2"
                    @click="IbSearch(item)"
                  >
                    {{ $t("action.viewAccounts") }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
          <TableFooter @page-change="fetchData" :criteria="criteria" />
        </div>
      </div>
      <!--for showing customer details-->
      <RepCustomerDetail
        v-if="accountUid !== -1"
        :service-map="serviceMap"
        :customer-accounts="accountsUnderRep"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue";
import { useStore } from "@/store";
import RepService from "../services/RepService";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import TableFooter from "@/components/TableFooter.vue";
import TimeShow from "@/components/TimeShow.vue";
import BalanceShow from "@/components/BalanceShow.vue";
import UserAvatar from "@/components/UserAvatar.vue";
import { useRoute } from "vue-router";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import SearchFilter from "@/components/SearchFilter.vue";
import RepCustomerDetail from "@/projects/client/modules/rep/views/RepCustomerDetail.vue";
import GlobalService from "@/projects/client/services/ClientGlobalService";

const route = useRoute();
const store = useStore();
const isLoading = ref(true);
const repAccount = computed(() => store.state.RepModule.repAccount);
const accountsUnderRep = ref(Array<any>());
const accountUid = computed(
  () => parseInt(route.params.accountId as string) || -1
);
const selectedIbAccountsChain = ref<any>(null);

const searchTrigger = ref(false);
const isSelectingClient = ref(false);

const criteria = ref({
  page: 1,
  size: 10,
  role: AccountRoleTypes.Sales,
} as any);

const onIsSelectingClientChange = () => {
  criteria.value = {
    page: 1,
    size: 10,
    role: isSelectingClient.value
      ? AccountRoleTypes.Client
      : AccountRoleTypes.IB,
    childParentAccountUid: criteria.value.childParentAccountUid,
  };
  handleRepSearch.value = (q: string) =>
    RepService.fuzzySearchAccount({
      ...criteria.value,
      keywords: q,
      role: isSelectingClient.value
        ? AccountRoleTypes.Client
        : AccountRoleTypes.IB,
    });
  fetchData(criteria.value.page);
};

const goToIbLevel = (idx) => {
  if (idx === selectedIbAccountsChain.value.length - 1) {
    // console.log(selectedIbAccountsChain.value[idx]);
    return;
  }
  selectedIbAccountsChain.value.splice(idx + 1);
  criteria.value.childParentAccountUid = selectedIbAccountsChain.value[idx].uid;

  fetchData(1);
};

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const res = await RepService.queryAccounts(criteria.value);
    criteria.value = res.criteria;
    accountsUnderRep.value = res.data;
  } catch (e) {
    console.log(e);
  }
  isLoading.value = false;
};

watch(repAccount, async (newVal, oldVal) => {
  if (newVal.uid !== oldVal.uid) {
    await fetchData(1);
  }
});

const serviceMap = ref({} as any);
onMounted(async () => {
  if (repAccount.value) {
    await fetchData(criteria.value.page);
  }
  serviceMap.value = await GlobalService.getServiceMap();
});

const clearIbSearch = () => {
  selectedIbAccountsChain.value = null;
  initCriteria();
  handleRepSearch.value = (q: string) =>
    RepService.fuzzySearchAccount({
      ...criteria.value,
      keywords: q,
    });
  fetchData(1);
};

const IbSearch = (_ibAccount) => {
  if (selectedIbAccountsChain.value === null)
    selectedIbAccountsChain.value = Array<any>();
  selectedIbAccountsChain.value.push(_ibAccount);

  criteria.value.childParentAccountUid = _ibAccount.uid;
  criteria.value.role = 300;
  handleRepSearch.value = (q: string) =>
    RepService.fuzzySearchAccount({
      ...criteria.value,
      agentUid: _ibAccount.uid,
      keywords: q,
    });
  fetchData(1);
};

const isSearch = ref(false);

const handleRepSearch = ref((q: string) =>
  RepService.fuzzySearchAccount({
    ...criteria.value,
    keywords: q,
  })
);

const initCriteria = () => {
  criteria.value = {
    page: 1,
    size: 10,
    role: AccountRoleTypes.Sales,
  };
  if (selectedIbAccountsChain.value !== null) {
    criteria.value.childParentAccountUid =
      selectedIbAccountsChain.value[
        selectedIbAccountsChain.value.length - 1
      ].uid;
  }
};

const handleSearchResults = async (results) => {
  isSearch.value = true;
  if (results.ids.length === 0) {
    isSearch.value = false;
    initCriteria();
    await fetchData(1);
    return;
  }

  criteria.value = {
    ...results.criteria,
  };
  isLoading.value = true;

  const responseBody2 = await RepService.queryAccounts({
    uids: results.data.map((x) => x.accountUid),
  });
  accountsUnderRep.value = responseBody2.data;
  isLoading.value = false;
};
</script>

<style scoped>
.card-body,
.sub-menu {
  width: 100%;
  white-space: nowrap;
}

.return-btn {
  padding: 2px 5px !important;
}

@media (max-width: 768px) {
  .table {
    font-size: 12px !important;
  }
}

.level-tool-tip {
  position: absolute;
  top: -12px;
  right: -18px;
  font-size: 12px;
  height: 17px;
  border-radius: 10px;
  padding: 0 5px;
  background-color: #ffc730;
}
</style>
