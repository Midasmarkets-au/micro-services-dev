<template>
  <div
    class="modal fade"
    id="kt_modal_iblibk_lists"
    tabindex="-1"
    aria-hidden="true"
    ref="IBLinkListsModalRef"
  >
    <IbLinksMobile v-if="isMobile" />
    <div v-else class="modal-dialog modal-dialog-centered mw-1000px">
      <div class="modal-content">
        <div class="modal-header" id="kt_modal_new_address_header">
          <h2 class="fs-2">{{ title }}</h2>
          <div data-bs-dismiss="modal">
            <span class="svg-icon svg-icon-1">
              <inline-svg src="/images/icons/arrows/arr061.svg" />
            </span>
          </div>
        </div>
        <div style="max-height: 80vh; overflow: auto">
          <table class="table align-middle table-row-brodered fs-6">
            <thead>
              <tr class="gs-0">
                <th class="text-center">{{ $t("fields.linkName") }}</th>
                <th class="text-center">{{ $t("fields.referCode") }}</th>
                <th class="text-center">{{ $t("fields.accountType") }}</th>
                <th class="text-center">{{ $t("fields.linkType") }}</th>
                <th class="text-center">{{ $t("fields.language") }}</th>
                <th class="text-center">{{ $t("action.view") }}</th>
                <th class="text-center">{{ $t("status.active") }}</th>
                <th class="text-center">{{ $t("title.autoCreateAccount") }}</th>
                <th class="text-center">{{ $t("fields.actions") }}</th>
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
                <td class="text-center">
                  {{ item.name
                  }}<i
                    class="fa-solid fa-pen-to-square ms-2"
                    style="cursor: pointer"
                    @click="editCode(item)"
                  ></i>
                </td>
                <td class="text-center">
                  {{ item.code }}
                </td>
                <td class="text-center">
                  <div class="d-flex justify-content-around">
                    <span v-if="item.serviceType == ReferralServiceType.Client">
                      <span
                        v-for="(acc, index) in item.displaySummary
                          .allowAccountTypes"
                        class="accountBadge ms-1"
                        :class="['type1', 'type2', 'type3'][index % 3]"
                        :key="'type_' + index"
                        >{{ $t("type.shortAccount." + acc.accountType) }}</span
                      ></span
                    >
                    <span v-if="item.serviceType == ReferralServiceType.Agent">
                      <span
                        v-for="(acc, index) in item.displaySummary.schema"
                        class="accountBadge ms-1"
                        :class="['type1', 'type2', 'type3'][index % 3]"
                        :key="'type_' + index"
                        >{{ $t("type.shortAccount." + acc.accountType) }}</span
                      ></span
                    >
                  </div>
                </td>
                <td class="text-center">
                  <div class="d-flex align-items-center justify-content-center">
                    {{ $t("type.accountRole." + item.serviceType) }}
                    <div
                      v-if="
                        item.serviceType == AccountRoleTypes.Client &&
                        item.isDefault == 1
                      "
                      class="me-1"
                    >
                      <el-tag type="success" size="small">
                        {{ $t("status.default") }}</el-tag
                      >
                    </div>
                  </div>
                </td>
                <td class="text-center">
                  {{
                    $t(
                      "type.language." +
                        (item.displaySummary?.language == undefined
                          ? getLanguage
                          : item.displaySummary?.language)
                    )
                  }}
                </td>
                <td class="text-center">
                  <i
                    class="fa-regular fa-eye"
                    style="cursor: pointer"
                    @click="showDetail(item.code)"
                  ></i>
                </td>

                <td class="text-center">
                  <el-switch
                    v-model="item.status"
                    class="p-4"
                    size="small"
                    style="font-size: 18px"
                    :active-value="0"
                    :inactive-value="1"
                    @change="changeStatus(item)"
                  >
                  </el-switch>
                </td>

                <td class="text-center">
                  {{
                    item.displaySummary?.isAutoCreatePaymentMethod === 1
                      ? $t("action.yes")
                      : $t("action.no")
                  }}
                </td>

                <td class="text-center">
                  <CopyReferralLink
                    :code="item.code"
                    :siteId="salesAccount.siteId"
                    :language="
                      item.displaySummary?.language == undefined
                        ? getLanguage
                        : item.displaySummary?.language
                    "
                  />
                </td>
              </tr>
            </tbody>
          </table>
          <TableFooter @page-change="changePage" :criteria="criteria" />
        </div>
      </div>
    </div>
  </div>

  <IBLinkDetailModal
    ref="IBLInkDetailRef"
    :key="IBLinkDetailComponentKey"
    :defaultLevelSetting="defaultLevelSetting"
    :productCategory="props.productCategory"
    :currentAccountRebateRule="currentAccountRebateRule"
  />

  <EditReferralLink ref="EditReferralLinkRef" @fetchData="fetchData" />
</template>
<script setup lang="ts">
import TableFooter from "@/components/TableFooter.vue";
import CopyReferralLink from "@/components/CopyReferralLink.vue";
import SalesService from "@/projects/client/modules/sales/services/SalesService";
import IBLinkDetailModal from "@/projects/client/modules/ib/components/IBLinkDetail.vue";
import EditReferralLink from "./EditReferralLink.vue";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import { useStore } from "@/store";
import { ref, computed, provide } from "vue";
import { showModal } from "@/core/helpers/dom";
import { getLanguage } from "@/core/types/LanguageTypes";
import { processKeysToCamelCase } from "@/core/services/api.client";
import { ReferralServiceType } from "@/core/types/ReferralServiceType";
import { isMobile } from "@/core/config/WindowConfig";
import IbLinksMobile from "./IbLinksMobile.vue";

const props = defineProps<{
  productCategory: any;
}>();

const criteria = ref({
  page: 1,
  pageSize: 10,
  childAccountUid: 0,
  status: 0,
});

const store = useStore();
const data = ref(<any>[]);
const isLoading = ref(false);
const rebateRuleDetail = ref();
const title = ref("Refer Code List");
const IBLinkDetailComponentKey = ref(0);
const configLevelSetting = ref({} as any);
const defaultLevelSetting = ref({} as any);
const currentAccountRebateRule = ref({} as any);
const IBLInkDetailRef = ref<InstanceType<typeof IBLinkDetailModal>>();
const EditReferralLinkRef = ref<InstanceType<typeof EditReferralLink>>();
const salesAccount = computed(() => store.state.SalesModule.salesAccount);
const IBLinkListsModalRef = ref<null | HTMLElement>(null);

const editCode = (item: any) => {
  EditReferralLinkRef.value?.show(item);
};

const changePage = (page: number) => {
  fetchData(page);
};

const showDetail = (_code: string) => {
  IBLInkDetailRef.value?.show(_code, rebateRuleDetail.value.isRoot);
};

const changeStatus = async (item: any) => {
  try {
    await SalesService.updateIbLink(item.id, {
      status: item.status,
      name: item.name,
    });
  } catch (error) {
    // console.log(error);
  }
};

provide("salesAccount", salesAccount);
provide("data", data);
provide("isLoading", isLoading);
provide("title", title);
provide("editCode", editCode);
provide("showDetail", showDetail);
provide("changeStatus", changeStatus);

const show = async (_item: any) => {
  isLoading.value = true;
  IBLinkDetailComponentKey.value++;

  showModal(IBLinkListsModalRef.value);

  currentAccountRebateRule.value = {};
  criteria.value.childAccountUid = _item.uid;
  title.value = _item.user.displayName + " " + "Refer Code List";

  try {
    rebateRuleDetail.value = await SalesService.getIbRebateRuleDetailFromSales(
      _item.uid
    );

    defaultLevelSetting.value =
      await SalesService.getAccountDefaultLevelSetting(
        rebateRuleDetail.value.agentAccountUid
      );
    defaultLevelSetting.value = processKeysToCamelCase(
      defaultLevelSetting.value
    );

    // Get account configuration LevelSetting - IB rate has OPTIONS!
    // If IB account has options, put in account configuration DefaultRebateLevelSetting
    const ibConfig = await SalesService.getIBAccountConfiguration(_item.uid);
    if (ibConfig.length != 0) {
      const getDefaultLevelSetting = ibConfig.find(
        (x) => x.key == "DefaultRebateLevelSetting"
      );

      if (getDefaultLevelSetting) {
        configLevelSetting.value = JSON.parse(getDefaultLevelSetting.value);
      }
    }

    const _levelSetting =
      rebateRuleDetail.value.calculatedLevelSetting.allowedAccounts;

    _levelSetting.forEach((account: any) => {
      let currentAccount = (currentAccountRebateRule.value[
        account.accountType
      ] = {} as any);

      currentAccount.selected = false;
      currentAccount.optionName = account.optionName;
      currentAccount.accountType = account.accountType;
      currentAccount.percentage = account.percentage;
      currentAccount.allowPips = account.allowPips;
      currentAccount.allowCommissions = account.allowCommissions;
      currentAccount.pips = account.pips;
      currentAccount.commission = account.commission;

      currentAccount.items = {};
      account.items.forEach((item: any) => {
        if (Object.keys(configLevelSetting.value).length == 0) {
          currentAccount.items[item.cid] = item.r;
        } else {
          currentAccount.items[item.cid] =
            configLevelSetting.value[account.accountType][0].Category[item.cid];
        }
      });
    });

    fetchData(1);
  } catch (error) {
    // console.log(error);
  }
};

const fetchData = async (page: number) => {
  isLoading.value = true;
  criteria.value.page = page;

  try {
    const res = await SalesService.getIbLinks(criteria.value);
    data.value = res.data;
    criteria.value = res.criteria;
  } catch (e) {
    // console.log(e);
  }

  isLoading.value = false;
};

defineExpose({
  show,
});
</script>
<style>
.accountBadge {
  width: 43px;
  height: 20px;
  border-radius: 8px;
  padding: 2px 8px;
  font-size: 10px;
  font-weight: 700;
}

.type1 {
  background: rgba(88, 168, 255, 0.1);
  color: #4196f0;
}

.type2 {
  background: rgba(255, 164, 0, 0.15);
  color: #ffa400;
}

.type3 {
  background: rgba(123, 97, 255, 0.1);
  color: #7b61ff;
}
</style>
