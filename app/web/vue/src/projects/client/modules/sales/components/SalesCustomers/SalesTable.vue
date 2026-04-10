<template>
  <table
    class="table align-middle table-row-bordered gy-2"
    v-if="accountUid === -1"
  >
    <thead class="table-header">
      <tr class="text-start gs-0">
        <th>{{ $t("fields.customer") }}</th>

        <th v-if="currentRole == null">
          {{ $t("fields.role") }}
        </th>

        <th
          class="text-start"
          v-if="
            currentRole == AccountRoleTypes.IB ||
            currentRole == AccountRoleTypes.Sales
          "
        >
          {{ $t("fields.accountUid") }}
        </th>

        <th v-else>
          {{ $t("fields.accountUid") }} / {{ $t("fields.accountNo") }}
        </th>

        <th class="text-start">{{ $t("fields.type") }}</th>
        <th class="text-start">
          {{ $t("fields.code") }} & {{ $t("fields.group") }}
        </th>
        <th class="text-start">{{ $t("fields.createdOn") }}</th>
        <th class="text-start" v-if="currentRole != AccountRoleTypes.IB">
          {{ $t("fields.balance") }}
        </th>
        <th class="text-start">{{ $t("fields.actions") }}</th>
      </tr>
    </thead>
    <tbody v-if="isLoading">
      <LoadingRing />
    </tbody>
    <tbody v-else-if="!isLoading && data.length === 0">
      <NoDataBox />
    </tbody>
    <tbody v-else>
      <tr v-for="(item, index) in data" :key="index">
        <td>
          <div class="d-flex align-items-center">
            <UserAvatar
              :avatar="item.user?.avatar"
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
                @click="showUnlockEmailAddress(item.uid, item.user?.email)"
                >{{ item.user?.email }}</span
              >
            </span>
          </div>
        </td>
        <td v-if="currentRole == null">
          <span :class="getRoleType(item).class">
            {{ getRoleType(item).label }}
          </span>
        </td>
        <td :class="currentRole == null ? 'text-center' : 'text-start'">
          {{
            item.role == AccountRoleTypes.Client
              ? item.accountNumber == 0
                ? t("fields.noTradeAccount")
                : item.accountNumber
              : item.uid
          }}
          <span v-html="item.hasCommission == true ? ' \&\#169' : ''"> </span>
          <span v-html="item.hasPips == true ? ' \&\#8471' : ''"> </span>
        </td>

        <td class="text-start">
          {{ getAccountType(item) }}
        </td>
        <td class="text-start">
          <span v-if="item.code !== ''">{{ item.code }} <br /></span
          >{{ item.group }}
        </td>
        <td class="text-start">
          <TimeShow :date-iso-string="item.createdOn" type="inFields" />
        </td>
        <td class="text-start" v-if="currentRole != AccountRoleTypes.IB">
          <BalanceShow
            v-if="item.role != AccountRoleTypes.IB"
            :balance="item?.tradeAccount?.balanceInCents"
            :currency-id="item?.tradeAccount?.currencyId"
          />
        </td>
        <td>
          <el-dropdown trigger="click">
            <el-button>
              {{ $t("action.action")
              }}<el-icon class="el-icon--right"><arrow-down /></el-icon>
            </el-button>
            <template #dropdown>
              <el-dropdown-menu>
                <template
                  v-for="(dropdownItem, index) in getDropdownItems(item)"
                  :key="index"
                >
                  <el-dropdown-item
                    v-if="dropdownItem.condition"
                    @click="dropdownItem.action && dropdownItem.action()"
                  >
                    <router-link
                      v-if="dropdownItem.isLink"
                      :to="dropdownItem.to"
                      :style="dropdownItem.style"
                    >
                      {{ dropdownItem.label }}
                    </router-link>
                    <span v-else>
                      {{ dropdownItem.label }}
                    </span>
                  </el-dropdown-item>
                </template>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </td>
      </tr>
    </tbody>
  </table>
  <TableFooter
    v-if="accountUid === -1"
    @page-change="fetchData"
    :criteria="criteria"
  />
  <SalesCustomerDetail
    v-if="accountUid !== -1"
    :service-map="serviceMap"
    :customer-accounts="data"
  />

  <NewIbReferralLinkModal
    ref="NewIbReferralLinkRef"
    :productCategory="productCategory"
  />
  <IbLinks ref="IbLinksRef" :productCategory="productCategory" />
  <UnlockEmailAddress ref="UnlockEmailAddressRef" />
  <ViewRebateStat ref="ViewRebateStatRef" />
  <RebateRuleEditModal
    ref="RebateRuleEditRef"
    :productCategory="productCategory"
  />
  <OpenTradeAccount ref="OpenTradeAccountRef" />
  <AccountRebateRelation ref="AccountRebateRelationRef" />
  <ShowDashboard ref="showDashboardRef" />

  <div
    class="modal fade"
    id="kt_modal_sales_link_lists"
    tabindex="-1"
    aria-hidden="true"
    ref="SalesLinkListsModalRef"
  >
    <div class="modal-dialog modal-dialog-centered mw-1000px">
      <div class="modal-content">
        <div class="modal-header" id="kt_modal_new_address_header">
          <h2 class="fs-2">{{ title }}</h2>
          <div data-bs-dismiss="modal">
            <span class="svg-icon svg-icon-1">
              <inline-svg src="/images/icons/arrows/arr061.svg" />
            </span>
          </div>
        </div>
        <div class="modal-body" style="max-height: 80vh; overflow: auto">
          <SaleLinks
            ref="SaleLinksRef"
            :saleId="selectedSalesUid"
            isHideTitle
          />
        </div>
      </div>
    </div>
  </div>

  <div
    class="modal fade"
    id="kt_modal_sales_add_link"
    tabindex="-1"
    aria-hidden="true"
    ref="SalesAddLinkModalRef"
  >
    <div class="modal-dialog modal-dialog-centered mw-1000px">
      <div class="modal-content">
        <div class="modal-header" id="kt_modal_new_sales_link_header">
          <h2 class="fs-2">{{ addSalesLinkTitle }}</h2>
          <div data-bs-dismiss="modal">
            <span class="svg-icon svg-icon-1">
              <inline-svg src="/images/icons/arrows/arr061.svg" />
            </span>
          </div>
        </div>
        <div class="modal-body" style="max-height: 80vh; overflow: auto">
          <SalesAddNewLink :saleId="selectedSalesUid" @refresh="refresh" />
        </div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import i18n from "@/core/plugins/i18n";
import { useStore } from "@/store";
import { ref, inject } from "vue";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import { PublicSetting } from "@/core/types/ConfigTypes";
import NewIbReferralLinkModal from "../modal/NewIbReferralLink.vue";
import RebateRuleEditModal from "@/projects/client/components/modals/RebateRuleEditModal.vue";
import SalesCustomerDetail from "@/projects/client/modules/sales/views/SalesCustomerDetail.vue";
import OpenTradeAccount from "../OpenTradeAccount.vue";
import AccountRebateRelation from "@/projects/client/modules/sales/components/modal/AccountRebateRelation.vue";
import ShowDashboard from "../modal/ShowDashboard.vue";
import IbLinks from "../modal/IbLinks.vue";
import SaleLinks from "@/projects/client/modules/sales/components/SalesManageLink.vue";
import SalesAddNewLink from "@/projects/client/modules/sales/components/SalesAddNewLink.vue";
import UnlockEmailAddress from "../UnlockEmailAddress.vue";
import ViewRebateStat from "../modal/ViewRebateStat.vue";
import { ArrowDown } from "@element-plus/icons-vue";
import SalesService from "../../services/SalesService";
import { hideModal, showModal } from "@/core/helpers/dom";

const data = inject<any>("data");
const isLoading = inject("isLoading");
const currentRole = inject("currentRole");
const productCategory = inject("productCategory");
const serviceMap = inject("serviceMap");
const accountUid = inject("accountUid");
const fetchData = inject("fetchData");
const IbSearch = inject<any>("IbSearch");
const criteria = inject("criteria");
const getUserName = inject<any>("getUserName");
const store = useStore();
const t = i18n.global.t;
const projectConfig: PublicSetting = store.state.AuthModule.config;
const title = ref("Refer Code List");
const addSalesLinkTitle = ref("Add New Link");
const selectedSalesUid = ref<number>();
const IbLinksRef = ref<InstanceType<typeof IbLinks>>();
const UnlockEmailAddressRef = ref<InstanceType<typeof UnlockEmailAddress>>();
const ViewRebateStatRef = ref<InstanceType<typeof ViewRebateStat>>();
const RebateRuleEditRef = ref<InstanceType<typeof RebateRuleEditModal>>();
const NewIbReferralLinkRef = ref<InstanceType<typeof NewIbReferralLinkModal>>();
const OpenTradeAccountRef = ref<InstanceType<typeof OpenTradeAccount>>();
const showDashboardRef = ref<InstanceType<typeof ShowDashboard>>();
const AccountRebateRelationRef =
  ref<InstanceType<typeof AccountRebateRelation>>();
const SaleLinksRef = ref<InstanceType<typeof SaleLinks>>();
const SalesLinkListsModalRef = ref<null | HTMLElement>(null);
const SalesAddLinkModalRef = ref<null | HTMLElement>(null);
const getRoleType = (item: any) => {
  switch (item.role) {
    case AccountRoleTypes.Client:
      return {
        class: "badge text-bg-info btn btn-radius",
        label: t("fields.client"),
      };
    case AccountRoleTypes.IB:
      return {
        class: "badge text-bg-success btn btn-radius",
        label: t("fields.ib"),
      };
    case AccountRoleTypes.Sales:
      return {
        class: "badge text-bg-danger btn btn-radius",
        label: t("fields.sales"),
      };
  }
};

const getAccountType = (item: any) => {
  return t(`type.account.${item.type}`);
};

const showEditSchema = async (_item: any) => {
  // _service, _parentRole, _parentUid, _editUid

  const targetAccount = await SalesService.getAccountDetailByUid(_item.uid);

  RebateRuleEditRef.value?.show(
    SalesService,
    targetAccount.agentAccountUid == 0
      ? AccountRoleTypes.Sales
      : AccountRoleTypes.IB,
    targetAccount.agentAccountUid == 0
      ? targetAccount.salesAccountUid
      : targetAccount.agentAccountUid,
    targetAccount.uid
  );
};

const showAddLinkModal = (item: any) => {
  NewIbReferralLinkRef.value?.show(item);
};
const showLinkList = (item: any) => {
  IbLinksRef.value?.show(item);
};
const showUnlockEmailAddress = (uid: any, email: any) => {
  console.log("uid", uid);
  UnlockEmailAddressRef.value?.show(uid, email);
};
const showRebateStat = (item: any) => {
  ViewRebateStatRef.value?.show(item);
};
const showAccountRebateRelationModal = (item: any) => {
  AccountRebateRelationRef.value?.show(item);
};

const showDashboard = (item: any) => {
  showDashboardRef.value?.show(item);
};

const showCreateAccount = (item: any) => {
  OpenTradeAccountRef.value?.show(item);
};
const showSalesLinkList = (_item: any) => {
  selectedSalesUid.value = _item.uid;
  title.value = _item.user.displayName + " " + "Refer Code List";
  showModal(SalesLinkListsModalRef.value);
  SaleLinksRef.value?.fetchData();
};
const showAddSalesLinkModal = (item: any) => {
  selectedSalesUid.value = item.uid;
  addSalesLinkTitle.value =
    item.user.displayName + " " + t("action.addNewLink");
  showModal(SalesAddLinkModalRef.value);
};
const refresh = () => {
  hideModal(SalesAddLinkModalRef.value);
  SaleLinksRef.value?.fetchData();
};

const getDropdownItems = (item) => {
  return [
    {
      condition:
        item.role == AccountRoleTypes.IB || item.role == AccountRoleTypes.Sales,
      isLink: false,
      action: () => IbSearch(item),
      label: t("action.viewAccounts"),
    },
    {
      condition: item.role == AccountRoleTypes.Client,
      isLink: true,
      to: `/sales/customers/${item.uid}`,
      style: "width:100%",
      label: t("action.viewDetails"),
    },

    {
      condition: true,
      isLink: false,
      action: () => showRebateStat(item),
      label: t("title.viewTradeStatistics"),
    },
    {
      condition: true,
      isLink: false,
      action: () => showCreateAccount(item),
      label: t("action.createTradeAccount"),
    },

    // {
    //   condition: item.role == AccountRoleTypes.IB,
    //   isLink: false,
    //   action: () => showDashboard(item),
    //   label: t("title.dashboard"),
    // },
    {
      condition: item.role == AccountRoleTypes.IB,
      isLink: false,
      action: () => showAccountRebateRelationModal(item),
      label: t("title.viewRebateRelation"),
    },
    {
      condition: item.role == AccountRoleTypes.IB,
      isLink: false,
      action: () => showLinkList(item),
      label: t("title.refferalCodeList"),
    },
    // {
    //   condition: item.role == AccountRoleTypes.Sales,
    //   isLink: false,
    //   action: () => showSalesLinkList(item),
    //   label: t("title.refferalCodeList"),
    // },

    {
      condition:
        item.role == AccountRoleTypes.IB && projectConfig.rebateEnabled,
      isLink: false,
      action: () => showEditSchema(item),
      label: t("action.editSchema"),
    },
    {
      condition:
        item.role == AccountRoleTypes.IB && projectConfig.rebateEnabled,
      isLink: false,
      action: () => showAddLinkModal(item),
      label: t("action.newIBReferraCode"),
    },
    // {
    //   condition:
    //     item.role == AccountRoleTypes.Sales && projectConfig.rebateEnabled,
    //   isLink: false,
    //   action: () => showAddSalesLinkModal(item),
    //   label: t("action.newIBReferraCode"),
    // },
  ];
};
</script>
<style scoped>
td {
  text-align: start;
}
@media (max-width: 768px) {
  .table {
    font-size: 12px !important;
  }
}
</style>
