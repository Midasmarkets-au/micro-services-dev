<template>
  <div v-if="isLoading">
    <LoadingRing />
  </div>
  <div v-else>
    <AccountRebateInfo :account-details="accountDetails" />
    <hr />
    <!-- ======================================================= -->
    <!-- ======================================================= -->
    <div v-if="isClient">
      <div class="fs-3 text-center mt-9">
        <div v-if="rebateType == RebateDistributionTypes.Direct">
          <span>Direct Rebate Setting ( 直接反佣設置 )</span>
        </div>
        <div v-if="rebateType == RebateDistributionTypes.Allocation">
          <div>Level Rebate Setting ( 層級反佣設置 )</div>
          <div v-if="Object.keys(pcSettings).length != 0">
            <span class="typeBadge">Pips: {{ pcSettings.pips }}</span>
            <span class="typeBadge ms-5"
              >Commission: {{ pcSettings.commission }}</span
            >
          </div>
          <div v-else>
            <span class="typeBadge">No Pips / Commission Setting</span>
          </div>
        </div>
      </div>
      <div class="d-flex justify-content-end align-items center">
        <div class="mt-5">
          <Field
            v-model="rebateType"
            class="form-check-input widget-9-check me-3"
            type="radio"
            name="directRebate"
            value="1"
            @change="updateDistributionType"
          />
          <label class="me-9" for="directRebate">Direct Rebate</label>
          <Field
            v-model="rebateType"
            class="form-check-input widget-9-check me-3"
            type="radio"
            name="allocationRebate"
            value="2"
            @change="updateDistributionType"
          />
          <label class="me-9" for="allocationRebate">Allocation Rebate</label>
          <Field
            v-model="rebateType"
            class="form-check-input widget-9-check me-3"
            type="radio"
            name="levelSet"
            value="3"
            @change="updateDistributionType"
          />
          <label class="me-9" for="levelSet">Level Set</label>
        </div>
        <!-- <el-switch
          v-model="rebateType"
          size="large"
          style="--el-switch-on-color: #0275d8; --el-switch-off-color: #13ce66"
          active-text="Direct Rebate"
          inactive-text="Level Rebate"
          active-value="1"
          inactive-value="2"
          @click="updateDistributionType"
        /> -->
      </div>
    </div>
    <!-- ======================================================= -->
    <!-- ======================================================= -->

    <div v-else class="mt-9">
      <div class="fs-3 mb-3 d-flex justify-content-center align-items-center">
        This is a
        <div
          class="accountBadge d-flex justify-content-center align-items-center ms-3 me-3"
        >
          {{ $t("type.accountRole." + accountDetails.role) }}
        </div>
        Account
      </div>
    </div>

    <div v-if="updatingType"><LoadingRing /></div>
    <div v-else>
      <div class="px-0 d-flex gap-1" style="border-bottom: 3px #ffd400 solid">
        <span
          v-if="rebateType == RebateDistributionTypes.Direct && isClient"
          class="basic-tab"
          :class="{ 'active-tab': activeTab == RebateDistributionTypes.Direct }"
          @click="activeTab = rebateType"
        >
          Direct Rebate
        </span>
        <span
          v-if="rebateType == RebateDistributionTypes.Allocation && isClient"
          class="basic-tab"
          :class="{
            'active-tab': activeTab == RebateDistributionTypes.Allocation,
          }"
          @click="activeTab = rebateType"
        >
          Allocation Rebate
        </span>

        <span
          v-if="rebateType == RebateDistributionTypes.LevelSet && isClient"
          class="basic-tab"
          :class="{
            'active-tab': activeTab == RebateDistributionTypes.LevelSet,
          }"
          @click="activeTab = rebateType"
        >
          Level Set Rebate
        </span>

        <span
          class="basic-tab"
          :class="{ 'active-tab': activeTab === 'relationSheet' }"
          @click="activeTab = 'relationSheet'"
        >
          Relation Sheet
        </span>
      </div>

      <AccountRebateDirect
        v-if="activeTab == RebateDistributionTypes.Direct && isClient"
        :account-details="accountDetails"
      ></AccountRebateDirect>

      <AccountRebateLevel
        v-if="activeTab == RebateDistributionTypes.Allocation && isClient"
        :account-details="accountDetails"
      ></AccountRebateLevel>

      <AccountRebateLevel
        v-if="activeTab == RebateDistributionTypes.LevelSet && isClient"
        :account-details="accountDetails"
      ></AccountRebateLevel>

      <AccountRebateRelation
        v-if="activeTab == 'relationSheet'"
        :accountId="accountDetails.id"
      ></AccountRebateRelation>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import RebateService from "../../rebate/services/RebateService";
import { RebateDistributionTypes } from "@/core/types/RebateDistributionTypes";
import AccountRebateInfo from "./AccountRebateInfo.vue";
import AccountRebateDirect from "./AccountRebateDirect.vue";
import AccountRebateLevel from "./AccountRebateLevel.vue";
import AccountRebateRelation from "./AccountRebateRelation.vue";
import { Field } from "vee-validate";

const isLoading = ref(true);
const updatingType = ref(false);
const accountDetails = ref({} as any);
const pcSettings = ref({} as any);
const isClient = ref(false);

const props = defineProps<{
  accountDetails: any;
}>();
const rebateType = ref();
const activeTab = ref();

onMounted(async () => {
  accountDetails.value = props.accountDetails;

  isClient.value = accountDetails.value.role === AccountRoleTypes.Client;

  if (isClient.value) {
    rebateType.value =
      accountDetails.value.rebateClientRule.distributionType.toString();

    activeTab.value = accountDetails.value.rebateClientRule.distributionType;

    pcSettings.value = JSON.parse(
      accountDetails.value.rebateClientRule?.schema
    );

    if (Object.keys(pcSettings.value).length != 0) {
      pcSettings.value = pcSettings.value.find(function (item) {
        return item.accountType === accountDetails.value.type;
      });
    } else {
      pcSettings.value = {};
    }
  } else {
    activeTab.value = "relationSheet";
  }
  // await fetchData();
  isLoading.value = false;
});

// const fetchData = async () => {
//   if (accountDetails.value.role === AccountRoleTypes.Client) {
//     if (isDirect(accountDetails.value.rebateClientRule.distributionType)) {
//       // 直接返佣，获取返佣规则
//       // await fetchDirectRebateData();
//     } else {
//       // Client层级返佣，获取上级账号信息
//       await fetchLevelRebateData();
//     }
//   } else {
//     // IB层级返佣，获取上级账号信息
//     await fetchLevelRebateData();
//   }
// };

// const fetchDirectRebateData = async () => {
//   const responseBody = await RebateService.getRebateDirectRule(
//     rebateRuleCriteria
//   );
//   rebateRuleCriteria.value = responseBody.criteria;
//   rebateRuleData.value = responseBody.data;

//   console.log("direct", responseBody);
// };

// const fetchLevelRebateData = async () => {
//   // // 如果是Client获取返佣配置
//   // if (accountDetails.value.role === AccountRoleTypes.Client) {
//   //   const _clientSchema = await RebateService.getRebateClientRule(
//   //     accountDetails.value.rebateClientRule.id
//   //   );
//   //   clientSchema.value = _clientSchema;
//   // }
// };

const updateDistributionType = async () => {
  updatingType.value = true;

  try {
    await RebateService.putRebateAllocationRule(
      props.accountDetails.rebateClientRule.id,
      { distributionType: rebateType.value }
    );
    accountDetails.value.rebateClientRule.distributionType = rebateType.value;
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    updatingType.value = false;
    activeTab.value = rebateType.value;
  }
};
</script>

<style scoped>
.accountBadge {
  width: 100px;
  height: 30px;
  background: rgba(88, 168, 255, 0.1);
  border-radius: 8px;
  color: black;
  padding: 2px 8px;
  font-size: 18px;
  font-weight: 700;
}
</style>
