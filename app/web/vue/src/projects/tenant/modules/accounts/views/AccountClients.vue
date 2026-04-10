<template>
  <div class="card mb-xl-8">
    <div class="card-header d-block pb-2">
      <div class="d-flex flex-wrap align-items-center my-3 gap-3">
        <p class="m-0 fs-5 flex-shrink-0">{{ $t("action.search") }}:</p>

        <!--        <SearchFilter-->
        <!--          :custom-search-handler="handleSearchAccount"-->
        <!--          @get-results-ids="handleSearchResults"-->
        <!--          :defaultCriteria="criteria"-->
        <!--          :search-trigger="searchTrigger"-->
        <!--          place-holder="Fuzzy Search"-->
        <!--          multiple-selection-->
        <!--        />-->
        <el-input
          style="max-width: 400px; min-width: 200px; flex: 1"
          v-model="criteria.searchText"
          :clearable="true"
          v-on:keyup.enter="fetchData(1)"
        >
          <template #append>
            <el-button :icon="Search" @click="fetchData(1)" />
          </template>
        </el-input>
        <SearchHistory
          category="account"
          ref="SearchHistoryRef"
          @reSearch="handleResearch"
        />
      </div>
      <div class="d-flex flex-wrap align-items-center gap-3">
        <div class="d-flex flex-wrap align-items-center gap-2">
          <p class="m-0 fs-5 flex-shrink-0">{{ $t("action.filter") }}:</p>
          <el-checkbox
            v-model="criteria.includeClosed"
            :label="t('status.showClosedAccount')"
          />
          <el-select
            class="w-100px"
            v-model="criteria.serviceId"
            placeholder="MT"
          >
            <el-option
              v-for="item in ServiceList"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
          <el-select
            class="w-100px"
            v-model="criteria.status"
            :placeholder="$t('fields.status')"
          >
            <el-option label="-- All --" value="" />
            <el-option label="Activated" :value="AccountStatusTypes.Activate" />
            <el-option label="Paused" :value="AccountStatusTypes.Pause" />
            <el-option
              label="Inactivated"
              :value="AccountStatusTypes.Inactivated"
            />
          </el-select>
        </div>

        <div class="d-flex flex-wrap align-items-center gap-2">
          <el-select
            class="w-100px"
            v-model="criteria.currencyId"
            :placeholder="$t('fields.currency')"
          >
            <el-option
              v-for="item in currencyOptions"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
          <el-select
            class="w-75px"
            v-model="criteria.siteId"
            :placeholder="$t('fields.site')"
          >
            <el-option label="-- All --" value="" />
            <el-option
              :label="$t(`type.siteType.${SiteTypes.Default}`)"
              :value="SiteTypes.Default"
            />
            <el-option
              :label="$t(`type.siteType.${SiteTypes.BritishVirginIslands}`)"
              :value="SiteTypes.BritishVirginIslands"
            />
            <el-option
              :label="$t(`type.siteType.${SiteTypes.Australia}`)"
              :value="SiteTypes.Australia"
            />
            <el-option
              :label="$t(`type.siteType.${SiteTypes.China}`)"
              :value="SiteTypes.China"
            />
            <el-option
              :label="$t(`type.siteType.${SiteTypes.Taiwan}`)"
              :value="SiteTypes.Taiwan"
            />
            <el-option
              :label="$t(`type.siteType.${SiteTypes.Vietnam}`)"
              :value="SiteTypes.Vietnam"
            />
            <el-option
              :label="$t(`type.siteType.${SiteTypes.Japan}`)"
              :value="SiteTypes.Japan"
            />
            <el-option
              :label="$t(`type.siteType.${SiteTypes.Mongolia}`)"
              :value="SiteTypes.Mongolia"
            />
          </el-select>
          <el-select
            class="w-100px"
            v-model="criteria.role"
            :placeholder="$t('fields.role')"
          >
            <el-option label="-- All --" value="" />
            <el-option label="Client" :value="AccountRoleTypes.Client" />
            <el-option label="IB" :value="AccountRoleTypes.IB" />
            <el-option label="Sales" :value="AccountRoleTypes.Sales" />
          </el-select>
          <!-- <el-input
            class="w-125px"
            v-model="criteria.parentAccountUid"
            placeholder="Child Accounts"
            clearable
            @keyup.enter="fetchData(1)"
          /> -->
          <!-- <el-input
            class="w-125px"
            v-model="criteria.salesUid"
            placeholder="Sales Child Only"
            clearable
            @keyup.enter="fetchData(1)"
          /> -->
          <el-autocomplete
            clearable
            @keyup.enter="fetchData(1)"
            class="w-125px"
            v-model="criteria.group"
            :fetch-suggestions="queryAgentGroupAsync"
            :placeholder="$t('fields.ibGroup')"
          />
          <el-autocomplete
            @keyup.enter="fetchData(1)"
            clearable
            class="w-125px"
            v-model="criteria.codeUid"
            :fetch-suggestions="querySalesGroupAsync"
            :placeholder="$t('fields.salesCode')"
          />
        </div>

        <div class="d-flex flex-wrap gap-2">
          <el-button type="primary" class="" @click="fetchData(1)">
            {{ $t("action.search") }}
          </el-button>
          <el-button
            plain
            type="primary"
            class=""
            @click="clearSearchFilterCriteria"
          >
            {{ $t("action.clear") }}
          </el-button>
          <el-button type="success" @click="showExportPanel" plain>
            {{ $t("action.export") }}
          </el-button>
        </div>
      </div>
    </div>
    <div class="card-body">
      <div
        class="text-end w-100 mb-4 d-flex align-items-center justify-content-end gap-1"
      >
        <div>
          <i class="fa-solid fa-circle-dot text-success"></i>
          {{ $t("status.normal") }} |
        </div>
        <div>
          <i class="fa-solid fa-circle-dot text-danger"></i>
          {{ $t("status.closed") }} |
        </div>
        <div>
          <i class="fa-solid fa-vial text-danger m-1"></i>
          {{ $t("status.testAccount") }} |
        </div>
        <div>
          <i class="fa-regular fa-file text-primary m-1"></i>
          {{ $t("status.dailyReport") }} |
        </div>
        <div>
          <i class="fa-regular fa-paste text-gray"></i>
          {{ $t("action.copy") }} |
        </div>
        <div>
          <i class="fa-regular fa-comment-dots text-primary"></i>
          {{ $t("status.hasComments") }} |
        </div>

        <div class="d-flex align-items-center">
          <el-icon color="#FF0000"><WarningFilled /></el-icon>
          {{ $t("title.cheaterIp") }} |
        </div>
        <div class="d-flex align-items-center">
          <el-icon><Avatar /></el-icon> {{ $t("status.blocked") }} |
        </div>
        <div class="d-flex align-items-center" v-if="tenant != 'jp'">
          &#169; <span> {{ $t("fields.commission") }}</span> |
        </div>
        <div class="d-flex align-items-center" v-if="tenant != 'jp'">
          &#8471; <span> {{ $t("title.pips") }}</span>
        </div>
      </div>
      <div class="table-responsive">
        <table
          class="table align-middle table-row-dashed fs-6 gy-1 table-hover"
          id="table_accounts_requests"
        >
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th class=""></th>
              <th class="">{{ $t("fields.status") }}</th>
              <th class="">{{ $t("fields.id") }}</th>
              <th class="">{{ $t("fields.type") }}</th>
              <th class="" v-if="tenant != 'jp'">
                {{ $t("fields.currency") }}
              </th>
              <th class="" v-if="tenant != 'jp'">
                {{ $t("fields.wallet") + " ID" }}
              </th>
              <th class="" v-if="tenant != 'jp'">MT</th>
              <th class="">{{ $t("fields.tradeAccount") }}</th>
              <th class="">{{ $t("fields.name") }}</th>
              <th class="">{{ $t("fields.email") }}</th>
              <th class="">{{ $t("fields.lastLoginIp") }}</th>
              <th class="">{{ $t("fields.registeredIp") }}</th>
              <th class="" v-if="tenant != 'jp'">{{ $t("fields.site") }}</th>
              <th class="" v-if="projectConfig.rebateEnabled && tenant != 'au'">
                {{ $t("fields.rebate") }}
              </th>
              <th class="">{{ $t("fields.group") }}</th>
              <th class="">{{ $t("fields.role") }}</th>
              <th class="">{{ $t("fields.sales") }}</th>
              <th class="">{{ $t("fields.ib") }}</th>
              <!-- <th class="">{{ $t("fields.accountCode") }}</th> -->
              <!-- <th class="">{{ $t("title.rebate") }}</th> -->
              <th class="">{{ $t("fields.created_at") }}</th>
            </tr>
          </thead>

          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && liveAccounts.length === 0">
            <NoDataBox />
          </tbody>
          <tbody v-else class="fw-semibold">
            <tr
              v-for="(item, index) in liveAccounts"
              :key="index"
              :class="{
                'text-gray-600': item.status === 1,
                'text-gray-400': item.status === 2,
                'account-select': item.id === accountSelected,
              }"
              @click="selectedAccount(item.id)"
            >
              <td>
                <el-dropdown trigger="click">
                  <el-button type="primary" class="btn btn-secondary btn-sm">
                    {{ $t("action.action")
                    }}<el-icon class="el-icon--right"><arrow-down /></el-icon>
                  </el-button>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item
                        @click="handleOpenAccountProcedure(item)"
                        v-if="!item.wizzard?.paymentAccessGranted"
                      >
                        {{ $t("tip.accountOpenProcedure") }}
                      </el-dropdown-item>
                      <template v-else>
                        <el-dropdown-item
                          @click="handleOpenAccountProcedure(item, 3)"
                        >
                          {{ $t("tip.readOnlyNotice") }}
                        </el-dropdown-item>

                        <el-dropdown-item
                          @click="handleOpenAccountProcedure(item, 4)"
                        >
                          {{ $t("tip.kycForm") }}
                        </el-dropdown-item>

                        <el-dropdown-item
                          @click="handleOpenAccountProcedure(item, 5)"
                        >
                          {{ $t("title.paymentMethod") }}
                        </el-dropdown-item>

                        <el-dropdown-item
                          v-if="tenant != 'au'"
                          @click="handleOpenAccountProcedure(item, 6)"
                        >
                          {{ $t("title.rebate") }}
                        </el-dropdown-item>
                      </template>

                      <el-dropdown-item
                        divided
                        @click="handleOpenUserDetail(item.user?.partyId)"
                      >
                        {{ $t("title.userDetails") }}
                      </el-dropdown-item>

                      <el-dropdown-item
                        divided
                        @click="
                          showAccountDetail(item.id, 'infos', item.user.email)
                        "
                      >
                        {{ $t("title.accountDetails") }}
                      </el-dropdown-item>

                      <el-dropdown-item
                        @click="showAccountDetail(item.id, 'trade')"
                      >
                        {{ $t("title.tradeHistory") }}
                      </el-dropdown-item>

                      <el-dropdown-item
                        @click="showAccountDetail(item.id, 'transaction')"
                      >
                        {{ $t("title.transferHistory") }}
                      </el-dropdown-item>

                      <el-dropdown-item
                        @click="showAccountDetail(item.id, 'deposit')"
                      >
                        {{ $t("title.deposit") }}
                      </el-dropdown-item>

                      <el-dropdown-item
                        @click="showAccountDetail(item.id, 'withdraw')"
                      >
                        {{ $t("title.withdrawal") }}
                      </el-dropdown-item>

                      <el-dropdown-item
                        v-if="
                          (item.role == 400 || item.role == 300) &&
                          tenant != 'jp' &&
                          tenant != 'au'
                        "
                        @click="showAccountDetail(item.id, 'rebate')"
                      >
                        {{ $t("title.rebate") }}
                      </el-dropdown-item>

                      <!-- <el-dropdown-item
                      @click="showAccountDetail(item.id, 'payment')"
                    >
                      {{ $t("title.paymentMethod") }}
                    </el-dropdown-item> -->
                      <el-dropdown-item
                        @click="showAccountDetail(item.id, 'paymentMethod')"
                      >
                        {{ $t("title.paymentMethod") }}
                      </el-dropdown-item>
                      <el-dropdown-item
                        v-if="item.role == 400"
                        divided
                        @click="openChangeCreditPanel(item.accountNumber)"
                      >
                        {{ $t("action.changeCredit") }}
                      </el-dropdown-item>
                      <el-dropdown-item
                        v-if="item.role == 400"
                        @click="openChangeAdjustPanel(item.accountNumber)"
                      >
                        {{ $t("action.changeAdjust") }}
                      </el-dropdown-item>
                      <el-dropdown-item
                        v-if="item.role == 400 && tenant != 'jp'"
                        @click="createKYCFromOpenAccountProcedure(item)"
                      >
                        {{ $t("action.generateKyc") }}
                      </el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
              </td>
              <td @click="handleGodModeClick(item.partyId)">
                <AccountStatusBadge :status="item.status" :showDot="true" />
                <i
                  v-if="item.accountTags.includes('Test')"
                  class="fa-solid fa-vial text-danger m-1"
                ></i>
                <i
                  v-if="item.accountTags.includes('DailyConfirmEmail')"
                  class="fa-regular fa-file text-primary m-1"
                ></i>
              </td>

              <td :title="`${item.id} (${item.partyId})`">
                {{ item.uid
                }}<TinyCopyBox
                  class="p-1"
                  :val="item.uid.toString()"
                ></TinyCopyBox>
              </td>
              <td>{{ $t(`type.shortAccount.${item.type}`) }}</td>
              <td v-if="tenant != 'jp'">
                {{ t(`type.currency.${item.currencyId}`) }}
              </td>
              <td v-if="tenant != 'jp'">
                {{ item.walletId }}
              </td>
              <td v-if="tenant != 'jp'">
                {{ serviceMap[item.serviceId]?.serverName }}
              </td>
              <td>
                <span>
                  {{ item.accountNumber }}
                </span>
                <TinyCopyBox
                  v-if="item.accountNumber"
                  :val="item.accountNumber.toString()"
                ></TinyCopyBox>
                <span
                  v-if="$can('Admin')"
                  class="ms-2 cursor-pointer"
                  role="button"
                  @click="
                    viewComments(
                      CommentType.Account,
                      item.id,
                      item.accountNumber
                    )
                  "
                >
                  <i
                    v-if="item.hasComment"
                    class="fa-regular fa-comment-dots text-primary"
                  ></i>
                  <i
                    v-else
                    class="fa-regular fa-comment-dots text-secondary"
                  ></i>
                </span>
              </td>
              <td>
                <span
                  role="button"
                  @click="
                    viewComments(
                      CommentType.User,
                      item.user.partyId,
                      item.user.firstName + ' ' + item.user.lastName
                    )
                  "
                >
                  <div class="d-flex align-items-center gap-1">
                    {{ item.user.firstName }} {{ item.user.lastName }}
                    <span
                      v-if="$can('Admin')"
                      class="ms-2 cursor-pointer"
                      role="button"
                      @click="
                        viewComments(
                          CommentType.User,
                          item.user?.partyId,
                          item.user?.firstName + ' ' + item.user?.lastName
                        )
                      "
                    >
                      <i
                        v-if="item.user?.hasComment"
                        class="fa-regular fa-comment-dots text-primary"
                      ></i>
                      <i
                        v-else
                        class="fa-regular fa-comment-dots text-secondary"
                      ></i>
                    </span>
                    <i
                      v-if="item.user.status == 2"
                      class="fa-solid fa-circle-dot text-danger"
                    ></i>
                    <el-icon v-if="item.isInIpBlackList" color="#FF0000"
                      ><WarningFilled
                    /></el-icon>
                    <el-icon v-if="item.isInUserBlackList"><Avatar /></el-icon>
                  </div>
                </span>
              </td>
              <td>
                {{ item.user.email
                }}<TinyCopyBox class="p-1" :val="item.user.email"></TinyCopyBox>

                <span
                  v-if="$can('Admin')"
                  class="ms-2 cursor-pointer"
                  role="button"
                  @click="
                    viewComments(
                      CommentType.User,
                      item.user?.partyId,
                      item.user?.firstName + ' ' + item.user?.lastName
                    )
                  "
                >
                  <i
                    v-if="item.user?.hasComment"
                    class="fa-regular fa-comment-dots text-primary"
                  ></i>
                  <i
                    v-else
                    class="fa-regular fa-comment-dots text-secondary"
                  ></i>
                </span>
              </td>
              <td>{{ item.user.lastLoginIp }}</td>
              <td>{{ item.user.registeredIp }}</td>
              <td v-if="tenant != 'jp'">
                {{ $t(`type.siteType.${item.siteId}`) }}
              </td>
              <td v-if="projectConfig.rebateEnabled">
                <div class="d-flex">
                  <span
                    class="text-uppercase me-1"
                    :class="{ 'text-danger': !item.hasRebateRule }"
                    @click="showAccountDetail(item.id, 'rebate')"
                    >{{ item.hasRebateRule }}</span
                  >
                  <span
                    v-html="
                      item.accountTags.includes('AddCommission') == true
                        ? ' \&\#169'
                        : ''
                    "
                  >
                  </span>
                  <span
                    v-html="
                      item.accountTags.includes('AddPips') == true
                        ? ' \&\#8471'
                        : ''
                    "
                  >
                  </span>
                </div>
              </td>
              <td>
                <div
                  class="d-flex justify-content-start align-items-center gap-2"
                >
                  <ViewReferPath :item="item" />
                  <div>{{ item.group }}</div>
                </div>
              </td>
              <td>
                {{ $t(`type.accountRole.${item.role}`) }}
              </td>

              <td>
                <template v-if="item.role === AccountRoleTypes.Sales">
                  <span class="me-1"> {{ item.code }}</span>
                  <TinyCopyBox :val="item.code.toString()"></TinyCopyBox>
                </template>
                <span v-else>
                  <span
                    role="button"
                    @click="
                      viewComments(
                        CommentType.Account,
                        item.salesAccount.id,
                        item.salesAccount.user.firstName +
                          ' ' +
                          item.salesAccount.user.lastName
                      )
                    "
                  >
                    [{{ item.salesAccount.code }}]
                    {{ item.salesAccount.uid }}

                    <i
                      v-if="item.salesAccount.hasComment"
                      class="fa-regular fa-comment-dots text-primary"
                    ></i>
                  </span>
                  <TinyCopyBox
                    :val="item.salesAccount.uid.toString()"
                  ></TinyCopyBox>
                </span>
              </td>
              <td>
                <span v-if="item.agentAccount.id == 0">No IB</span>
                <span v-else>
                  <span
                    role="button"
                    @click="
                      viewComments(
                        CommentType.Account,
                        item.agentAccount.id,
                        item.agentAccount.user.firstName +
                          ' ' +
                          item.agentAccount.user.lastName
                      )
                    "
                    >[{{ item.agentAccount.group }}]
                    {{ item.agentAccount.uid }}
                    <i
                      v-if="item.agentAccount.hasComment"
                      class="fa-regular fa-comment-dots text-primary"
                    ></i
                  ></span>
                  <TinyCopyBox
                    :val="item.agentAccount.uid.toString()"
                  ></TinyCopyBox>
                </span>
              </td>
              <!-- <td>{{ item.code }}</td> -->
              <!-- <td>{{ item.agentRebateRule }}</td> -->
              <td><TimeShow :date-iso-string="item.createdOn" /></td>
            </tr>
          </tbody>
        </table>
      </div>

      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
    <AccountDetail ref="accountDetailRef" />

    <OpenProcedureForm ref="openProcedureFormRef" />
    <CommentsView ref="commentsRef" type="" id="0" />
    <ChangeAdjustForm ref="changeAdjustFormRef" />
    <ChangeCreditForm ref="changeCreditFormRef" />
    <ExportAccounts ref="exportAccountsRef" />
  </div>
</template>

<script lang="ts" setup>
import { useStore } from "@/store";
import {
  computed,
  inject,
  nextTick,
  onMounted,
  provide,
  ref,
  watch,
} from "vue";
import OpenProcedureForm from "../components/OpenProcedureForm.vue";
import i18n from "@/core/plugins/i18n";
import TableFooter from "@/components/TableFooter.vue";
import AccountDetail from "../components/AccountDetail.vue";
import GlobalService from "@/projects/client/services/ClientGlobalService";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import { ServiceMapType } from "@/core/types/ServiceTypes";
import {
  ArrowDown,
  Search,
  Avatar,
  WarningFilled,
} from "@element-plus/icons-vue";
import CommentsView from "@/projects/tenant/components/CommentView.vue";
import { CommentType } from "@/core/types/CommentType";
import TimeShow from "@/components/TimeShow.vue";
import InjectionKeys from "@/core/types/TenantGlobalInjectionKeys";
import { PublicSetting } from "@/core/types/ConfigTypes";
import { useRoute, useRouter } from "vue-router";
import {
  AccountRoleTypes,
  AccountStatusTypes,
} from "@/core/types/AccountInfos";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
import { SiteTypes } from "@/core/types/SiteTypes";
import AccountService, {
  generateAutoCompleteHandler,
} from "@/projects/tenant/modules/accounts/services/AccountService";
import { AccountGroupTypes } from "@/core/types/AccountGroupTypes";
import UserService from "@/projects/tenant/modules/users/services/UserService";
import TinyCopyBox from "@/components/TinyCopyBox.vue";
import ViewReferPath from "@/projects/tenant/modules/accounts/components/modal/ViewReferPath.vue";
import ChangeCreditForm from "@/projects/tenant/modules/Payment/components/ChangeCreditForm.vue";
import ChangeAdjustForm from "@/projects/tenant/modules/Payment/components/ChangeAdjustForm.vue";
import SearchHistory from "@/projects/tenant/components/SearchHistory.vue";
import ExportAccounts from "../components/modal/ExportAccounts.vue";

const { t } = i18n.global;
const route = useRoute();
const router = useRouter();
const store = useStore();
const tenant = ref(store.state.AuthModule.user?.tenancy);
const queryAgentGroupAsync = generateAutoCompleteHandler(
  async (keywords: string) => {
    const res = await AccountService.getFullAccountGroupNames(
      AccountGroupTypes.Agent,
      keywords
    );
    return res.map((item) => ({ value: item, label: item }));
  }
);

const querySalesGroupAsync = generateAutoCompleteHandler(
  async (keywords: string) => {
    const res = await AccountService.getFullAccountGroupNames(
      AccountGroupTypes.Sales,
      keywords
    );
    return res.map((item) => ({ value: item, label: item }));
  }
);

const projectConfig = computed<PublicSetting>(
  () => store.state.AuthModule.config
);
const ServiceList = ref<any>([
  { label: "-- All --", value: "" },
  { label: "No Server", value: 0 },
  { label: "MDM-Real", value: 10 },
  { label: "MDM-Real", value: 20 },
  { label: "MDM-MT5 Real", value: 30 },
  { label: "MDM-MT5 Demo", value: 31 },
]);
const currencyOptions = ref([
  { label: "-- All --", value: "" },
  { value: 840, label: "USD" },
  { value: 841, label: "USC" },
]);

const getInitialCriteria = () => ({
  page: 1,
  size: 20,
  ServiceId:
    {
      mt4: 20,
      mt5: 30,
    }[route.params.type as string] ?? undefined,
  includeClosed: false,
  sortField: "CreatedOn",
});

const criteria = ref<any>(getInitialCriteria());
const accountDetailRef = ref<InstanceType<typeof AccountDetail>>();
const changeCreditFormRef = ref<InstanceType<typeof ChangeCreditForm>>();
const changeAdjustFormRef = ref<InstanceType<typeof ChangeAdjustForm>>();
const commentsRef = ref<any>(null);
const exportAccountsRef = ref<any>(null);
const SearchHistoryRef = ref<InstanceType<typeof SearchHistory>>();

const openProcedureFormRef = ref<InstanceType<typeof OpenProcedureForm>>();

const isLoading = ref(true);
const liveAccounts = ref(Array<any>());
const serviceMap = ref<ServiceMapType>({} as ServiceMapType);

const isSearch = ref(false);
const searchTrigger = ref(false);

const accountSelected = ref(0);

const handleOpenUserDetail =
  inject(InjectionKeys.OPEN_USER_DETAILS) ?? ((any) => null);

provide(AccountInjectionKeys.HANDLE_REFRESH_ACCOUNT_CLIENT_TABLE, () =>
  fetchData(criteria.value.page)
);

const handleOpenAccountProcedure = (item, step?) => {
  /**
   * {
   *   "welcomeEmailSent": false,
   *   "noticeEmailSent": false
   *   "kycFormCompleted": false,
   *   "paymentAccessGranted": false,
   * }
   */
  openProcedureFormRef.value?.show(step ?? -1, { accountDetails: item });
};

const createKYCFromOpenAccountProcedure = (item) => {
  openProcedureFormRef.value?.show(4, { accountDetails: item });
};

const handleResearch = (text) => {
  console.log("reSearch", text);
  criteria.value.searchText = text;
  fetchData(1);
};

const paymentServiceCriteria = ref({
  size: 100,
});

const clearSearchFilterCriteria = async () => {
  criteria.value = getInitialCriteria();
  await fetchData(1);
};

const fetchData = async (_page: number) => {
  criteria.value.page = _page;
  await nextTick();
  if (isSearch.value) {
    searchTrigger.value = !searchTrigger.value;
    await nextTick();
    return;
  }
  if (criteria.value.searchText) {
    SearchHistoryRef.value?.updateSearchHistory(criteria.value.searchText);
  }
  try {
    isLoading.value = true;
    const responseBody = await AccountService.queryAccounts(criteria.value);
    criteria.value = responseBody.criteria;
    liveAccounts.value = responseBody.data;
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
  }
};

const selectedAccount = (id: number) => {
  accountSelected.value = id;
};

watch(
  () => route.params.type,
  () => {
    criteria.value = getInitialCriteria();
    fetchData(1);
  }
);

onMounted(async () => {
  serviceMap.value = await GlobalService.getServiceMap();
  criteria.value = getInitialCriteria();
  await fetchData(1);
  processActionByRoute();
});

const processActionByRoute = () => {
  if (!route.query) return;

  const action = route.query.action as string;
  const _handler = {
    checkRebate: () => {
      const _id = Number(route.query.accountId);
      showAccountDetail(_id, "rebate");
    },
  }[action];

  if (_handler) {
    _handler();
    setTimeout(() => router.push({ name: "accountClients" }), 5000);
  }
};

const viewComments = (type: CommentType, id: number, title: string) => {
  commentsRef.value?.show(type, id, title);
};
const showAccountDetail = (id: number, _tab = "infos", email?: string) => {
  accountDetailRef.value?.show(id, _tab, [] as any, undefined, email);
};

const openChangeCreditPanel = (accountNumber: number) => {
  changeCreditFormRef.value?.show(accountNumber);
};
const openChangeAdjustPanel = (accountNumber: number) => {
  changeAdjustFormRef.value?.show(accountNumber);
};

const showExportPanel = () => {
  exportAccountsRef.value?.show(criteria.value);
};

const handleGodModeClick = UserService.generateGodModeHandler();
</script>
<style>
.account-select {
  background-color: rgba(254, 215, 215, 0.5) !important;
}
</style>
