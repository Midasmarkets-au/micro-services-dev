<template>
  <div v-if="isLoadingMain" style="max-width: 880px; margin: auto">
    <div
      class="d-flex align-items-center justify-content-center"
      style="min-height: 300px"
    >
      <scale-loader :color="'#ffc730'"></scale-loader>
    </div>
  </div>
  <div v-else>
    <h1 class="text-3xl font-bold text-center text-gray-900 mb-12">
      {{ $t("action.openTradeAccount") }}
    </h1>

    <div class="d-flex gap-8 align-items-center justify-content-center">
      <div
        v-for="item in data"
        :key="item"
        class="card account-card h-100 text-center"
      >
        <p class="text-center title">{{ item.name }}</p>
        <div v-if="item.status == VerificationStatusTypes.Approved">
          <el-button type="info" :loading="isLoading" class="w-75 custom-btn">{{
            $t("status.active")
          }}</el-button>
        </div>

        <div v-else-if="item.status !== null">
          <el-button
            type="warning"
            :loading="isLoading"
            class="w-75 custom-btn"
            @click="openAccount(item)"
            >{{ $t("status.pending") }}</el-button
          >
        </div>

        <div v-else>
          <input
            type="checkbox"
            style="width: 40px; height: 40px"
            :loading="isLoading"
            @click="addToSelectedAccountArray(item)"
          />
        </div>
        <!-- <div v-else>
        <el-checkbox
          type="warning"
          class="w-250px"
          :loading="isLoading"
          @click="addToSelectedAccountArray(item)"
        ></el-checkbox>
      </div> -->
      </div>
    </div>

    <div class="d-flex align-items-center justify-content-center mt-10">
      <el-button
        v-if="typeData.length < 3"
        type="success"
        class="w-25 custom-btn"
        :loading="isLoading"
        :disabled="selectedAccountTypes.length === 0"
        @click="openAccount(null)"
        >{{ $t("status.open") }}</el-button
      >
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, inject, nextTick, provide } from "vue";
import AccountService from "../../accounts/services/AccountService";
import { AccountTypesJp } from "@/core/types/AccountInfos";
import {
  ApplicationStatusType,
  ApplicationType,
} from "@/core/types/ApplicationInfos";
import {
  AccountRoleTypes,
  AccountStatusTypes,
} from "@/core/types/AccountInfos";
import { accountTypes } from "@/core/types/jp/verificationFinancial";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import VerificationService from "../../verification/services/VerificationService";
import { VerificationStatusTypes } from "@/core/types/VerificationInfos";
import i18n from "@/core/plugins/i18n";

const t = i18n.global.t;
const isLoading = ref(false);
const isLoadingMain = ref(false);
const pendingVerifications = ref<any>([]);
const data = ref<any>([
  {
    name: t("fields.marginTrading"),
    accountTypes: [AccountTypesJp.JpALPFX, AccountTypesJp.JpSTDFX],
    verificationAccType: accountTypes.margin,
    status: null,
  },
  {
    name: t("fields.cfdTrading"),
    accountTypes: [AccountTypesJp.JpALPCFD, AccountTypesJp.JpSTDCFD],
    verificationAccType: accountTypes.cfdTrading,
    status: null,
  },
  {
    name: t("fields.productTrading"),
    accountTypes: [AccountTypesJp.JpALPIND, AccountTypesJp.JpSTDIND],
    verificationAccType: accountTypes.productTrading,
    status: null,
  },
]);
const typeData = ref<any>([]);
const mainStep = inject<any>("mainStep");
const selectedAccountTypes = inject<any>("selectedAccountTypes");
const selectedVerification = inject<any>("selectedVerification");
const verificationData = inject<any>("verificationData");

const openAccount = async (item: any) => {
  selectedVerification.value = item;
  await nextTick();
  mainStep.value = 1;
};

const addToSelectedAccountArray = (item) => {
  if (selectedAccountTypes.value.includes(item.verificationAccType)) {
    selectedAccountTypes.value = selectedAccountTypes.value.filter(
      (type) => type !== item.verificationAccType
    );
  } else {
    selectedAccountTypes.value.push(item.verificationAccType);
  }
};

const fetchOpenedAccountData = async () => {
  try {
    const pendingData = await AccountService.queryApplications({
      statuses: [
        ApplicationStatusType.AwaitingApproval,
        ApplicationStatusType.Approved,
      ],
      type: ApplicationType.TradeAccount,
    });

    const liveData = await AccountService.queryAccounts({
      hasTradeAccount: true,
      status: AccountStatusTypes.Activate,
      roles: [
        AccountRoleTypes.Client,
        AccountRoleTypes.SuperAdmin,
        AccountRoleTypes.TenantAdmin,
        AccountRoleTypes.Wholesale,
        AccountRoleTypes.Guest,
      ],
    });

    const liveTypesData = liveData.data.map((item) => item.type);
    const pendingTypesData = pendingData.data.map(
      (item) => item.supplement.accountType
    );
    typeData.value = [...liveTypesData, ...pendingTypesData];
    data.value.forEach((item) => {
      if (item.accountTypes.some((item) => typeData.value.includes(item))) {
        item.status = VerificationStatusTypes.Approved;
      }
    });
  } catch (error) {
    console.log(error);
  }
};

const fetchPendingVerification = async () => {
  try {
    const verificationRes =
      await VerificationService.getExistingVerifications();

    verificationData.value = verificationRes;

    pendingVerifications.value = verificationRes.filter(
      (item) => item.status !== VerificationStatusTypes.Approved
    );

    data.value.forEach((item) => {
      pendingVerifications.value.forEach((pendingItem) => {
        if (
          pendingItem.financial.accountTypes.includes(item.verificationAccType)
        ) {
          item.status = VerificationStatusTypes.Incomplete;
          typeData.value.push(item.verificationAccType);
        }
      });
    });
    console.log("data", data.value);
  } catch (error) {
    console.log(error);
  }
};

onMounted(async () => {
  isLoadingMain.value = true;
  await fetchOpenedAccountData();
  await fetchPendingVerification();
  isLoadingMain.value = false;
});
</script>

<style scoped>
.account-card {
  transition: transform 0.3s ease, box-shadow 0.3s ease;
  border: 1px solid #dee2e6;
  padding: 1.5rem;
  width: 300px;
  box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15);
}

.account-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15);
}

.title {
  font-size: 1.5rem;
  font-weight: 600;
}

.custom-btn {
  height: 48px !important;
  font-weight: 500;
  font-size: 1rem;
}
</style>
