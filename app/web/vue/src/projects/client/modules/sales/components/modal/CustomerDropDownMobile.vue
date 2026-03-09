<template>
  <el-dropdown trigger="click" class="w-100">
    <el-button type="primary" class="btn btn-secondary btn-sm" v-if="!isMobile">
      {{ $t("action.action")
      }}<el-icon class="el-icon--right"><arrow-down /></el-icon>
    </el-button>
    <div v-else class="w-100 text-end">
      <el-icon><Setting /></el-icon>
    </div>
    <template #dropdown>
      <el-dropdown-menu>
        <template
          v-for="(dropdownItem, index) in getDropdownItems(props.item)"
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

  <NewIbReferralLinkModal
    ref="NewIbReferralLinkRef"
    :productCategory="productCategory"
  />
  <IbLinks ref="IbLinksRef" :productCategory="productCategory" />
  <ViewRebateStat ref="ViewRebateStatRef" />
  <RebateRuleEditModal
    ref="RebateRuleEditRef"
    :productCategory="productCategory"
  />
  <OpenTradeAccount ref="OpenTradeAccountRef" />
  <AccountRebateRelation ref="AccountRebateRelationRef" />
  <ShowDashboard ref="showDashboardRef" />
</template>
<script setup lang="ts">
import i18n from "@/core/plugins/i18n";
import { ref, inject } from "vue";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import NewIbReferralLinkModal from "./NewIbReferralLink.vue";
import RebateRuleEditModal from "@/projects/client/components/modals/RebateRuleEditModal.vue";
import AccountRebateRelation from "@/projects/client/modules/sales/components/modal/AccountRebateRelation.vue";
import ShowDashboard from "./ShowDashboard.vue";
import IbLinks from "./IbLinks.vue";
import ViewRebateStat from "./ViewRebateStat.vue";
import OpenTradeAccount from "../OpenTradeAccount.vue";
import SalesService from "../../services/SalesService";
import { useStore } from "@/store";
import { PublicSetting } from "@/core/types/ConfigTypes";
import { ArrowDown, Setting } from "@element-plus/icons-vue";
import { isMobile } from "@/core/config/WindowConfig";

const props = defineProps<{
  item: any;
}>();

const t = i18n.global.t;
const IbSearch = inject<any>("IbSearch");
const productCategory = inject("productCategory");
const store = useStore();
const projectConfig: PublicSetting = store.state.AuthModule.config;
const IbLinksRef = ref<InstanceType<typeof IbLinks>>();
const ViewRebateStatRef = ref<InstanceType<typeof ViewRebateStat>>();
const RebateRuleEditRef = ref<InstanceType<typeof RebateRuleEditModal>>();
const NewIbReferralLinkRef = ref<InstanceType<typeof NewIbReferralLinkModal>>();
const OpenTradeAccountRef = ref<InstanceType<typeof OpenTradeAccount>>();
const showDashboardRef = ref<InstanceType<typeof ShowDashboard>>();
const AccountRebateRelationRef =
  ref<InstanceType<typeof AccountRebateRelation>>();

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
      style: "color: #7c8fa2;width:100%",
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
  ];
};
</script>
