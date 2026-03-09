<template>
  <IBLayout activeMenuItem="customers">
    <IBAccountsSelector v-if="!agentAccount" />
    <div v-if="agentAccount" class="h-full">
      <div v-if="accountUid === -1" class="card card-flush h-full">
        <div class="card-header">
          <div
            v-if="!isMobile"
            class="card-title-noicon d-flex justify-content-between align-items-center w-100 w-lg-auto"
          >
            <div>
              <el-select
                v-model="currentRole"
                :disabled="isLoading"
                class="w-125px"
              >
                <el-option
                  v-for="item in accountTypesSelection"
                  :key="item.value"
                  :label="item.label"
                  :value="item.value"
                ></el-option>
              </el-select>
            </div>

            <div class="ms-5 d-flex align-items-center justify-content-center">
              <div
                v-if="currentRole == AccountRoleTypes.Client"
                class="d-flex align-items-center justify-content-center me-3 gap-3"
              >
                <el-select
                  v-model="isDeposit"
                  class="w-125px"
                  :disabled="isLoading"
                  @change="depositFetch(1)"
                >
                  <el-option
                    v-for="item in isDepositOptions"
                    :key="item.value"
                    :label="item.label"
                    :value="item.value"
                  ></el-option>
                </el-select>
                <el-date-picker
                  v-model="startDate"
                  type="date"
                  :placeholder="t('fields.startDate')"
                  class="w-150px"
                  clearable
                  :disabled="isLoading"
                  value-format="YYYY-MM-DD"
                />
                <el-date-picker
                  v-model="endDate"
                  type="date"
                  class="w-150px"
                  :placeholder="t('fields.endDate')"
                  clearable
                  :disabled="isLoading"
                  value-format="YYYY-MM-DD"
                />
              </div>
              <el-input
                :disabled="isLoading"
                v-model="criteria.searchText"
                clearable
                :placeholder="$t('tip.searchKeyWords')"
              >
              </el-input>
            </div>
            <div class="d-flex ms-5 align-items-center w-150px">
              <label
                class="filter-label me-2"
                style="width: max-content; text-wrap: nowrap"
              >
                {{ $t("fields.pageSize") }}
              </label>
              <el-select
                :disabled="isLoading"
                v-model="criteria.size"
                placeholder="Page Size"
                @change="fetchData(1)"
              >
                <el-option
                  v-for="item in pageSizes"
                  :key="item"
                  :label="item"
                  :value="item"
                ></el-option>
              </el-select>
            </div>
            <div class="d-flex">
              <el-button
                size="large"
                class="ms-4"
                @click="search"
                :disabled="isLoading"
              >
                {{ $t("action.search") }}
              </el-button>
              <el-button
                size="large"
                class="ms-4"
                @click="reset"
                :disabled="isLoading"
              >
                {{ $t("action.reset") }}
              </el-button>
            </div>
          </div>
          <div v-if="isMobile">
            <div class="card-title d-flex gap-5">
              <div class="w-100px">
                <el-select v-model="currentRole" :disabled="isLoading">
                  <el-option
                    v-for="item in accountTypesSelection"
                    :key="item.value"
                    :label="item.label"
                    :value="item.value"
                  ></el-option>
                </el-select>
              </div>
              <div>
                <el-input
                  :disabled="isLoading"
                  v-model="criteria.searchText"
                  clearable
                  :placeholder="$t('tip.searchKeyWords')"
                >
                </el-input>
              </div>
            </div>

            <div v-if="currentRole == AccountRoleTypes.Client">
              <el-select
                v-model="isDeposit"
                class="w-125px"
                :disabled="isLoading"
                @change="depositFetch(1)"
              >
                <el-option
                  v-for="item in isDepositOptions"
                  :key="item.value"
                  :label="item.label"
                  :value="item.value"
                ></el-option>
              </el-select>
            </div>

            <div>
              <div
                class="d-flex gap-3 mt-3"
                v-if="currentRole == AccountRoleTypes.Client"
              >
                <el-date-picker
                  v-model="startDate"
                  type="date"
                  :placeholder="t('fields.startDate')"
                  class="w-150px"
                  clearable
                  :disabled="isLoading"
                  value-format="YYYY-MM-DD"
                />
                <el-date-picker
                  v-model="endDate"
                  type="date"
                  class="w-150px"
                  :placeholder="t('fields.endDate')"
                  clearable
                  :disabled="isLoading"
                  value-format="YYYY-MM-DD"
                />
              </div>
            </div>

            <div class="card-toolbar">
              <div class="mt-4 d-flex gap-1">
                <button
                  class="btn btn-primary btn-sm"
                  @click="search"
                  :disabled="isLoading"
                >
                  {{ $t("action.search") }}
                </button>
                <el-button size="large" @click="reset" :disabled="isLoading">
                  {{ $t("action.reset") }}
                </el-button>
              </div>
            </div>
          </div>
        </div>
        <div v-if="isMobile">
          <div
            class="card-header mb-3"
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
                      {{ getUserName(acc) }}
                    </div>
                    <div class="level-tool-tip">Lv{{ idx + 2 }}</div>
                  </div>
                  <span class="fw-bold fs-2 text-gray-600">/</span>
                </div>
              </div>
              <button
                @click="clearIbSearch"
                class="ms-4 btn btn-xs btn-primary return-btn text-nowrap"
              >
                {{ $t("action.clear") }}
              </button>
            </div>
          </div>

          <CustomerMobile />
          <TableFooter @page-change="pageChange" :criteria="criteria" />
        </div>

        <div v-else>
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
                      {{ getUserName(acc) }}
                    </div>
                    <div class="level-tool-tip">Lv{{ idx + 2 }}</div>
                  </div>
                  <span class="fw-bold fs-2 text-gray-600">/</span>
                </div>
              </div>

              <button
                @click="clearIbSearch"
                class="ms-4 btn btn-sm btn-primary btn-radius btn-bordered return-btn"
              >
                {{ $t("action.clear") }}
              </button>
            </div>
          </div>
          <div class="card-body pt-0 overflow-auto" style="white-space: nowrap">
            <table class="table align-middle table-row-bordered">
              <thead>
                <tr class="gs-0 gy-1">
                  <th class="text-start" width="*">
                    {{ $t("fields.customer") }}
                  </th>
                  <th
                    v-if="
                      currentRole != AccountRoleTypes.IB &&
                      currentRole != AccountRoleTypes.Client
                    "
                  >
                    {{ $t("fields.role") }}
                  </th>
                  <template v-if="currentRole == AccountRoleTypes.IB">
                    <th class="text-start">
                      {{ $t("fields.accountUid") }}
                    </th>
                  </template>

                  <th
                    class="text-start"
                    v-else-if="currentRole == AccountRoleTypes.Client"
                  >
                    {{ $t("fields.accountNo") }}
                  </th>

                  <th v-else>
                    {{ $t("fields.accountUid") }} /
                    {{ $t("fields.accountNo") }}
                  </th>

                  <th class="text-start">
                    {{ $t("fields.group") }}
                  </th>
                  <th class="text-start">
                    {{ $t("fields.code") }}
                  </th>
                  <th class="text-start">{{ $t("fields.type") }}</th>
                  <th
                    v-if="currentRole != AccountRoleTypes.IB"
                    class="text-start"
                  >
                    {{ $t("fields.balance") }}
                  </th>
                  <th class="text-start">{{ $t("fields.createdOn") }}</th>
                  <th class="text-start">
                    {{ $t("fields.actions") }}
                  </th>
                </tr>
              </thead>

              <tbody v-if="isLoading">
                <LoadingRing />
              </tbody>

              <tbody v-else-if="!isLoading && customerAccounts.length === 0">
                <NoDataBox />
              </tbody>

              <tbody v-else>
                <tr
                  v-for="(item, index) in customerAccounts"
                  :key="index"
                  class="gs-0 gy-1"
                >
                  <td>
                    <div class="d-md-flex align-items-center">
                      <UserAvatar
                        :avatar="item.user.avatar"
                        :name="getUserName(item)"
                        size="40px"
                        class="me-3"
                        side="client"
                        rounded
                      />
                      <span>
                        {{ getUserName(item) }}<br />
                        <span
                          class="cursor-pointer text-hover-primary"
                          @click="
                            showUnlockEmailAddress(item.uid, item.user?.email)
                          "
                          >{{ item.user?.email }}</span
                        >
                      </span>
                    </div>
                  </td>

                  <td
                    v-if="
                      currentRole != AccountRoleTypes.IB &&
                      currentRole != AccountRoleTypes.Client
                    "
                    class="text-start"
                  >
                    <span
                      class="text-start badge"
                      :class="
                        item.role == AccountRoleTypes.Client
                          ? 'text-bg-info btn btn-radius btn-xm text-active'
                          : 'text-bg-success btn btn-radius btn-xm'
                      "
                    >
                      {{
                        item.role == AccountRoleTypes.Client
                          ? $t("title.client")
                          : $t("title.ib")
                      }}
                    </span>
                  </td>

                  <template v-if="item.role == AccountRoleTypes.IB">
                    <!--- 原来的显示逻辑
                      :class="currentRole == null ? 'text-center' : 'text-start'
                      :class="currentRole == null ? 'text-center' : 'text-start'"
                      "-->
                    <td>
                      {{ item.uid }}
                    </td>
                  </template>

                  <td v-else-if="item.role == AccountRoleTypes.Client">
                    {{
                      item.tradeAccount?.accountNumber ?? $t("tip.noTrdAccount")
                    }}
                  </td>
                  <td class="text-start">
                    <span>
                      {{ item.group }}
                    </span>
                  </td>

                  <td class="text-start">
                    <span>
                      {{ item.code }}
                    </span>
                  </td>
                  <td>{{ $t(`type.account.${item.type}`) }}</td>

                  <td
                    v-if="currentRole != AccountRoleTypes.IB"
                    class="text-start"
                  >
                    <BalanceShow
                      v-if="item.role != AccountRoleTypes.IB"
                      :balance="item.tradeAccount?.balanceInCents"
                      :currency-id="item.tradeAccount?.currencyId"
                    />
                  </td>
                  <td class="text-start">
                    <TimeShow
                      :date-iso-string="item.createdOn"
                      type="oneLiner"
                    />
                  </td>

                  <td v-if="item.role == AccountRoleTypes.Client">
                    <router-link
                      :to="'/ib/customers/' + item.uid"
                      class=""
                      style="color: #0a46aa"
                    >
                      {{ $t("action.viewDetails") }}
                    </router-link>
                  </td>

                  <td v-else-if="item.role == AccountRoleTypes.IB">
                    <el-dropdown trigger="click">
                      <el-button>
                        {{ $t("action.action")
                        }}<el-icon class="el-icon--right"
                          ><arrow-down
                        /></el-icon>
                      </el-button>
                      <template #dropdown>
                        <el-dropdown-menu>
                          <el-dropdown-item @click="IbSearch(item)">
                            {{ $t("action.viewAccounts") }}
                          </el-dropdown-item>

                          <el-dropdown-item @click="showRebateStat(item)">
                            {{ $t("title.viewRebateStatistics") }}
                          </el-dropdown-item>

                          <el-dropdown-item
                            v-if="
                              projectConfig?.rebateEnabled &&
                              selectedIbAccountsChain?.length == 0
                            "
                            @click="showEditSchema(item)"
                          >
                            {{ $t("action.editSchema") }}
                          </el-dropdown-item>
                        </el-dropdown-menu>
                      </template>
                    </el-dropdown>
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

      <IBRebateDetail
        ref="IBRebateDetailRef"
        :productCategory="productCategory"
      />
    </div>
    <ViewRebateStat ref="ViewRebateStatRef" />
    <RebateRuleEditModal
      ref="RebateRuleEditRef"
      :productCategory="productCategory"
    />
    <UnlockEmailAddress ref="UnlockEmailAddressRef" />
  </IBLayout>
</template>

<script lang="ts" setup>
import i18n from "@/core/plugins/i18n";
import IbService from "../services/IbService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import NoDataBox from "@/components/NoDataBox.vue";
import IBCustomerDetail from "./IBCustomerDetail.vue";
import TableFooter from "@/components/TableFooter.vue";
import BalanceShow from "@/components/BalanceShow.vue";
import LoadingRing from "@/components/LoadingRing.vue";
import IBRebateDetail from "../components/IBRebateDetail.vue";
import ViewRebateStat from "../components/modal/ViewRebateStat.vue";
import IBAccountsSelector from "../components/IBAccountsSelector.vue";
import GlobalService from "@/projects/client/services/ClientGlobalService";
import RebateRuleEditModal from "@/projects/client/components/modals/RebateRuleEditModal.vue";
import UnlockEmailAddress from "../components/UnlockEmailAddress.vue";
import IBLayout from "../components/IBLayout.vue";
//import headerMenu from "../components/menu/headerMenu.vue";
import { useStore } from "@/store";
import { RouterLink, useRoute } from "vue-router";
import { ref, watch, onMounted, computed, provide } from "vue";
import { isMobile } from "@/core/config/WindowConfig";
import { PublicSetting } from "@/core/types/ConfigTypes";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import { AccountCriteria } from "@/core/models/AccountCriteria";
import { ArrowDown } from "@element-plus/icons-vue";
import moment from "moment";
import CustomerMobile from "../components/customers/CustomerMobile.vue";

const t = i18n.global.t;
const serviceMap = ref();
const store = useStore();
const route = useRoute();
const isLoading = ref(true);
const currentRole = ref(null);
const customerAccounts = ref();
const productCategory = ref(Array<any>());
const selectedIbAccountsChain = ref<any>([]);
const pageSizes = [10, 15, 20, 25, 30, 50, 100];
const agentAccount = ref(store.state.AgentModule.agentAccount);
const projectConfig: PublicSetting = store.state.AuthModule.config;
const ViewRebateStatRef = ref<InstanceType<typeof ViewRebateStat>>();
const IBRebateDetailRef = ref<InstanceType<typeof IBRebateDetail>>();
const RebateRuleEditRef = ref<InstanceType<typeof RebateRuleEditModal>>();
const UnlockEmailAddressRef = ref<InstanceType<typeof UnlockEmailAddress>>();
const tempSearchText = ref("");

const accountTypesSelection = ref([
  { label: t("title.all"), value: null },
  {
    label: t(`title.ib`),
    value: AccountRoleTypes.IB,
  },
  {
    label: t(`fields.client`),
    value: AccountRoleTypes.Client,
  },
]);

const startDate = ref(null);
const endDate = ref(null);

const isDeposit = ref(null);

const isDepositOptions = ref([
  { label: t("fields.all"), value: null },
  { label: t("fields.hasDeposit"), value: true },
  { label: t("fields.noDeposit"), value: false },
]);

const depositFetch = async (_page: number) => {
  criteria.value.isActive = isDeposit.value;
  await fetchData(_page);
  criteria.value.isActive = null;
};

const criteria = ref<any>({
  page: 1,
  size: 15,
  role: currentRole.value,
  sortField: "createdOn",
  sortFlag: true,
  searchText: null,
  relativeLevel: 1,
} as any);

const accountUid = computed(
  () => parseInt(route.params.accountId as string) || -1
);

const showEditSchema = async (_item: any) => {
  // _service, _parentRole, _parentUid, _editUid
  RebateRuleEditRef.value?.show(
    IbService,
    AccountRoleTypes.IB,
    _item.agentUid,
    _item.uid
  );
};

const showUnlockEmailAddress = (uid: any, email: any) => {
  UnlockEmailAddressRef.value?.show(uid, email);
};

watch(currentRole, (newValue) => {
  criteria.value.role = newValue;
  fetchData(1);
});

const reset = () => {
  criteria.value.searchText = null;
  criteria.value.from = null;
  criteria.value.to = null;
  startDate.value = null;
  endDate.value = null;
  isDeposit.value = null;
  fetchData(1);
};

const search = async () => {
  if (
    (criteria.value.searchText === "" || criteria.value.searchText === null) &&
    (startDate.value === null || endDate.value === null)
  ) {
    return;
  }
  if (startDate.value) {
    criteria.value.from = moment(startDate.value).startOf("day").toISOString();
  }
  if (endDate.value) {
    criteria.value.to = moment(endDate.value).endOf("day").toISOString();
  }
  const tempLevel = criteria.value.relativeLevel;
  criteria.value.relativeLevel = null;
  await fetchData(1);
  criteria.value.relativeLevel = tempLevel;
  criteria.value.from = null;
  criteria.value.to = null;
};

const clearIbSearch = () => {
  selectedIbAccountsChain.value = [];
  criteria.value = {
    page: 1,
    size: 10,
    role: currentRole.value,
    sortField: "createdOn",
    sortFlag: true,
    searchText: null,
    relativeLevel: 1,
  } as AccountCriteria;
  startDate.value = null;
  endDate.value = null;
  fetchData(1);
};

const fetchDataIgnoreSearchText = async () => {
  tempSearchText.value = criteria.value.searchText;
  criteria.value.searchText = null;
  await fetchData(1);
  criteria.value.searchText = tempSearchText.value;
};

const IbSearch = (_ibAccount) => {
  if (selectedIbAccountsChain.value === null)
    selectedIbAccountsChain.value = Array<any>();

  selectedIbAccountsChain.value.push(_ibAccount);
  criteria.value.childParentAccountUid = _ibAccount.uid;
  if (criteria.value.relativeLevel != null)
    criteria.value.relativeLevel = selectedIbAccountsChain.value.length + 1;

  fetchDataIgnoreSearchText();
};

const goToIbLevel = (idx) => {
  if (idx === selectedIbAccountsChain.value.length - 1) return;

  selectedIbAccountsChain.value.splice(idx + 1);
  criteria.value.childParentAccountUid = selectedIbAccountsChain.value[idx].uid;
  if (criteria.value.relativeLevel != null)
    criteria.value.relativeLevel = idx + 2;
  fetchDataIgnoreSearchText();
};

const fetchData = async (selectedPage: number) => {
  isLoading.value = true;
  criteria.value.page = selectedPage;
  try {
    const res = await IbService.queryAgentClientAccountsByAgent(criteria.value);
    criteria.value = res.criteria;
    customerAccounts.value = res.data;
    delete criteria.value.level;
  } catch (error: any) {
    if (error.isAxiosError && error.response.status === 404) {
      customerAccounts.value = [];
    }
    MsgPrompt.error(error.message);
  }
  isLoading.value = false;
};
const getUserName = (item: any) => {
  if (
    !item.user?.nativeName ||
    item.user?.nativeName === "" ||
    item.user?.nativeName === " "
  ) {
    if (
      !item.user?.displayName ||
      item.user?.displayName === "" ||
      item.user?.displayName === " "
    ) {
      if (
        !item.user?.firstName ||
        !item.user?.lastName ||
        item.user?.firstName === "" ||
        item.user?.lastName === "" ||
        item.user?.firstName === " " ||
        item.user?.lastName === " "
      ) {
        return "No Name";
      } else {
        return item.user?.firstName + " " + item.user?.lastName;
      }
    } else {
      return item.user?.displayName;
    }
  } else {
    return item.user?.nativeName;
  }
};
const pageChange = (page: number) =>
  page !== criteria.value.page && fetchData(page);
const showRebateStat = (item: any) => {
  ViewRebateStatRef.value?.show(item);
};

provide("customerAccounts", customerAccounts);
provide("isLoading", isLoading);
provide("currentRole", currentRole);
provide("getUserName", getUserName);
provide("IbSearch", IbSearch);
provide("showRebateStat", showRebateStat);
provide("showEditSchema", showEditSchema);
provide("showUnlockEmailAddress", showUnlockEmailAddress);
provide("selectedIbAccountsChain", selectedIbAccountsChain);

onMounted(async () => {
  if (projectConfig?.rebateEnabled) {
    productCategory.value = await IbService.getCategory();
  }
  console.log("agentAccount", agentAccount);
  if (agentAccount.value) {
    await fetchData(criteria.value.page);
  }
  serviceMap.value = await GlobalService.getServiceMap();
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
.filter-label {
  font-size: 14px;
  color: #b1b1b1;
}
td {
  padding: 0.5rem 0.25rem !important;
}
</style>
