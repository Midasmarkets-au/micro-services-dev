<template>
  <SimpleForm
    ref="openProcedureFormRef"
    :title="$t('title.accountOpeningProcedure')"
    :is-loading="isLoading"
    :width="1000"
    disable-footer
    :before-close="props.beforeClose"
    style=""
  >
    <div class="">
      <div class="w-100" style="margin: 0 auto">
        <!--     v-if="accountRole === AccountRole.Client"   -->
        <div
          class="mb-5"
          v-if="clientOpeningStep > 0 || ibSalesOpeningStep > 0"
        >
          <!--          => {{ accountDetails }}dddd-->
          <el-steps
            :active="clientOpeningStep - 1"
            align-center
            v-if="accountRole === AccountRoleTypes.Client"
          >
            <!--            @click="mountIntoClientStep(1)"-->
            <el-step
              class="cursor-pointer"
              :title="$t('title.accountApplications')"
            />

            <!--            @click="mountIntoClientStep(2)"-->
            <el-step class="cursor-pointer" :title="$t('action.openMt')" />

            <!--            @click="mountIntoClientStep(3)"-->
            <el-step class="cursor-pointer" :title="$t('tip.readOnlyNotice')" />

            <!--            @click="mountIntoClientStep(4)"-->
            <el-step class="cursor-pointer" :title="$t('tip.kycForm')" />

            <!--            @click="mountIntoClientStep(5)"-->
            <el-step
              class="cursor-pointer"
              :title="$t('title.paymentMethod')"
            />

            <!--            @click="mountIntoClientStep(6)"-->
            <el-step class="cursor-pointer" :title="$t('title.rebate')" />
          </el-steps>

          <el-steps :active="ibSalesOpeningStep - 1" align-center v-else>
            <!--            @click="mountIntoClientStep(1)"-->
            <el-step
              class="cursor-pointer"
              :title="$t('title.accountApplications')"
            />

            <!--            @click="mountIntoClientStep(4)"-->
            <!--            <el-step class="cursor-pointer" :title="$t('tip.kycForm')" />-->

            <!--            @click="mountIntoClientStep(5)"-->
            <el-step
              class="cursor-pointer"
              :title="$t('title.paymentMethod')"
            />
          </el-steps>
          <el-divider />
        </div>

        <table v-if="isLoading" class="table align-middle fs-6 gy-5 h-100">
          <tbody>
            <LoadingRing />
          </tbody>
        </table>

        <div v-if="!isLoading" class="w-100 px-10">
          <div>
            <UserDocuments
              v-if="clientOpeningStep === 0"
              :verification-details="verificationDetails"
              :verifyOperation="true"
              @user-approved="hide"
            />

            <div v-if="clientOpeningStep === 1 || ibSalesOpeningStep === 1">
              <OpenAccountForm
                @application-submitted="applicationApproved"
                @role-changed="roleChanged"
                :siteId="applicationDetails.siteId"
                :application-details="applicationDetails"
              />
              <!--              <AccountApplicationForm-->
              <!--                :pendingApplication="applicationDetails"-->
              <!--                @application-submitted="applicationApproved"-->
              <!--                @role-changed="roleChanged"-->
              <!--              />-->
            </div>

            <div v-if="clientOpeningStep === 2">
              <AccountCreate
                :application-details="applicationDetails"
                :accountDetails="accountDetails"
                @account-submitted="tradeAccountCreated"
              />
            </div>

            <!--            <WelcomeLetterForm-->
            <!--              v-if="currentStep === 3"-->
            <!--              @submit="currentStep += 1"-->
            <!--            />-->

            <ReadOnlyNotice
              v-if="clientOpeningStep === 3"
              @submit="clientOpeningStep += 1"
            />

            <KycForm
              v-if="clientOpeningStep === 4 || ibSalesOpeningStep === 2"
              @submit="kycFormSubmitted"
            />

            <PaymentMethodDetailForm
              v-if="clientOpeningStep === 5 || ibSalesOpeningStep === 3"
              @submit="paymentMethodSubmitted"
              :openForm="true"
            />

            <div v-if="clientOpeningStep === 6" class="h-500px">
              <div
                class="w-100 h-100 d-flex justify-content-center align-items-center"
              >
                <table>
                  <tbody>
                    <tr class="text-center">
                      <span
                        href="#"
                        class="btn btn-xl btn-light-danger fw-bold"
                        @click="
                          showAccountDetail(
                            applicationDetails.referenceId,
                            'rebate'
                          ),
                            (showRebateCompleteBtn = true)
                        "
                      >
                        Please Edit Rebate Schema
                      </span>
                    </tr>
                    <tr class="text-center" v-if="showRebateCompleteBtn">
                      <span
                        href="#"
                        class="mt-5 btn btn-xl btn-light-success fw-bold"
                        @click="hide()"
                      >
                        Complete
                      </span>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </SimpleForm>

  <AccountDetail ref="accountDetailRef" />
</template>

<script setup lang="ts">
import KycForm from "./KycForm.vue";
import AccountDetail from "./AccountDetail.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import ReadOnlyNotice from "./ReadOnlyNotice.vue";
import SimpleForm from "@/components/SimpleForm.vue";
import { computed, nextTick, provide, ref } from "vue";
import UserService from "../../users/services/UserService";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import PaymentMethodDetailForm from "./PaymentMethodDetailForm.vue";
import AccountCreate from "../../users/components/AccountCreate.vue";
import UserDocuments from "../../users/components/UserDocuments.vue";
import { ApplicationStatusType } from "@/core/types/ApplicationInfos";
import AccountService, { AccountWizardType } from "../services/AccountService";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
import OpenAccountForm from "@/projects/tenant/modules/accounts/components/form/OpenAccountForm.vue";

const props = defineProps<{
  beforeClose?: () => void;
}>();

const criteria = ref({
  size: 100,
});

const openProcedureFormRef = ref<InstanceType<typeof SimpleForm>>();
const accountDetailRef = ref<InstanceType<typeof AccountDetail>>();
const accountWizardStatus = ref<AccountWizardType | null>(null);
const verificationId = ref<number | undefined>();
const accountRole = ref(AccountRoleTypes.Client);
const applicationDetails = ref<any>(null);
const showRebateCompleteBtn = ref(false);
const verificationDetails = ref<any>({});
const accountDetails = ref<any>(null);
const ibSalesOpeningStep = ref(-1);
const clientOpeningStep = ref(-1);
const isLoading = ref(true);

const getAccountWizardStatus = async () => {
  if (!accountDetails.value) return;
  if (accountWizardStatus.value === null) {
    accountWizardStatus.value = await AccountService.getAccountWizardStatus(
      accountDetails.value.id
    );
  }
  return accountWizardStatus.value;
};

const readOnlyCode = ref<any>(null);
const getReadOnlyCode = async () => {
  if (readOnlyCode.value === null) {
    const res = await AccountService.getTradeAccountReadOnlyCode(
      accountDetails.value.id
    );
    readOnlyCode.value = res.readOnlyCode;
  }
  return readOnlyCode.value;
};

const emits = defineEmits<{
  (e: "userApproved"): void;
  (e: "stepFinished"): void;
}>();

const showAccountDetail = (id: number, _tab = "infos") => {
  accountDetailRef.value?.show(id, _tab, [] as any);
};

const loadAccountApplications = async () => {
  // 下面（comment 掉的部分） application detail 有值，就不用再去查
  // 這樣在審核 application "過程中", 就無法更新到最新的 application 數據 (ex: referenceId 就在過程中更新了)
  try {
    const { data } = await AccountService.queryApplications({
      referenceId: accountDetails.value?.id,
      id: applicationDetails.value?.id,
    });
    applicationDetails.value = data[0];
  } catch (error) {
    console.log(error);
  }

  // if (!applicationDetails.value) {
  //   if (!accountDetails.value) {
  //     MsgPrompt.error("Account details not found");
  //     return;
  //   }
  //   const { data } = await AccountService.queryApplications({
  //     referenceId: accountDetails.value?.id,
  //     id: applicationDetails.value?.id,
  //   });
  //   if (data.length <= 0) {
  //     MsgPrompt.error("Account application not found");
  //     return;
  //   }
  //   applicationDetails.value = data[0];
  // }
  return true;
};

const loadTradeAccountCreate = async () => {
  await loadServices();
  await loadAccountApplications();
  return true;
};

const loadReadOnlyNotice = async () => {
  const wzdStatus = await getAccountWizardStatus();
  if (wzdStatus?.noticeEmailSent) {
    return false;
  }
  return true;
};

const loadKycForm = async () => {
  const wzdStatus = await getAccountWizardStatus();
  if (wzdStatus?.kycFormCompleted) {
    return false;
  }
  return true;
};

const loadPaymentMethod = async () => {
  const wzdStatus = await getAccountWizardStatus();
  if (wzdStatus?.paymentAccessGranted) {
    return false;
  }
};

const partyId = computed<number>(
  () => applicationDetails.value?.partyId ?? accountDetails.value?.partyId
);

const userInfos = ref<any>(null);
const getUserInfos = async () => {
  if (partyId.value && !userInfos.value) {
    userInfos.value = await UserService.getUserInfoByPartyId(partyId.value);
  }
  return userInfos.value;
};

const services = ref<any>(null);

provide(AccountInjectionKeys.APPLICATION_DETAILS, applicationDetails);
provide(AccountInjectionKeys.ACCOUNT_DETAILS, accountDetails);

provide(AccountInjectionKeys.GET_ACCOUNT_WIZARD_STATUS, getAccountWizardStatus);
provide(AccountInjectionKeys.GET_READ_ONLY_CODE, getReadOnlyCode);

provide(AccountInjectionKeys.GET_USER_INFOS, getUserInfos);
provide(AccountInjectionKeys.PLATFORM_SERVICES, services);

// func for application role changed
const roleChanged = (role: AccountRoleTypes) => {
  accountRole.value = role;
  if (
    [
      AccountRoleTypes.IB,
      AccountRoleTypes.Broker,
      AccountRoleTypes.Sales,
    ].includes(role)
  ) {
    ibSalesOpeningStep.value = 1;
  } else {
    clientOpeningStep.value = 1;
  }
};

const loadServices = async () => {
  if (!services.value) {
    services.value = await UserService.getServices();
  }
};

const mountIntoClientStep = async (_step: number) => {
  isLoading.value = true;
  ibSalesOpeningStep.value = -1;

  /**
   * {
   *     "kycFormCompleted": false,
   *     "paymentAccessGranted": false,
   *     "welcomeEmailSent": false,
   *     "noticeEmailSent": false
   * }
   */

  const handleMount =
    {
      1: loadAccountApplications,
      2: loadTradeAccountCreate,
      3: loadReadOnlyNotice,
      4: loadKycForm,
      5: loadPaymentMethod,
      6: loadAccountApplications,
    }[_step] ?? (() => MsgPrompt.error("Mount Error"));

  // await loadUserInfos();
  const wzdStatusOfStep = await handleMount();

  /**
   *  if mounted into step 3 or more, then we need to load user infos & accountDetails
   *  if they are both not loaded, we do not mount into the step
   */
  if (_step >= 3 && !userInfos.value && !accountDetails.value) {
    isLoading.value = false;
    return;
  }

  clientOpeningStep.value = _step;
  await nextTick();

  isLoading.value = false;
};

const mountIntoIbSalesStep = async (_step: number) => {
  isLoading.value = true;
  clientOpeningStep.value = -1;
  const handleMount =
    {
      1: loadAccountApplications,
      3: loadPaymentMethod,
      // 2: loadKycForm,
      4: hide,
    }[_step] ?? (() => MsgPrompt.error("Mount Error"));

  // await loadUserInfos();
  await handleMount();
  ibSalesOpeningStep.value = _step;
  await nextTick();
  isLoading.value = false;
};

const applicationApproved = async (createdAccount: any) => {
  accountDetails.value = createdAccount;
  applicationDetails.value.status = ApplicationStatusType.Approved;
  emits("stepFinished");

  [
    AccountRoleTypes.IB,
    AccountRoleTypes.Broker,
    AccountRoleTypes.Sales,
  ].includes(accountRole.value)
    ? await mountIntoIbSalesStep(3)
    : await mountIntoClientStep(2);
};

const kycFormSubmitted = async () => {
  [
    AccountRoleTypes.IB,
    AccountRoleTypes.Broker,
    AccountRoleTypes.Sales,
  ].includes(accountRole.value)
    ? await mountIntoIbSalesStep(3)
    : await mountIntoClientStep(5);
};

const tradeAccountCreated = async (createdTradeAccount: any) => {
  const res = await AccountService.queryAccounts({
    id: createdTradeAccount.id,
  });
  if (res.data.length <= 0) {
    MsgPrompt.error("No trade account found");
    accountDetails.value.tradeAccount = createdTradeAccount;
  } else {
    accountDetails.value = res.data[0];
  }
  await mountIntoClientStep(3);
};

const showAccountProcedureForm = async (
  mountStep: number,
  mountOptions: {
    selectedVerificationId?: number;
    applicationDetails?: any;
    accountDetails?: any;
  }
) => {
  openProcedureFormRef.value?.show();
  userInfos.value = null;
  showRebateCompleteBtn.value = false;
  accountRole.value =
    mountOptions.accountDetails?.role ??
    mountOptions.applicationDetails?.supplement.role ??
    AccountRoleTypes.Client;

  verificationId.value = mountOptions.selectedVerificationId;
  applicationDetails.value = mountOptions.applicationDetails;
  accountDetails.value = mountOptions.accountDetails;

  if (mountStep === -1) {
    const wizardStatus = await getAccountWizardStatus();
    mountStep =
      [
        { condition: wizardStatus?.welcomeEmailSent, step: 3 },
        { condition: wizardStatus?.noticeEmailSent, step: 4 },
        { condition: wizardStatus?.kycFormCompleted, step: 5 },
        { condition: wizardStatus?.paymentAccessGranted, step: 6 },
      ].find((x) => !x.condition)?.step ?? 3;
  }

  [
    AccountRoleTypes.IB,
    AccountRoleTypes.Broker,
    AccountRoleTypes.Sales,
  ].includes(accountRole.value)
    ? await mountIntoIbSalesStep(mountStep)
    : await mountIntoClientStep(mountStep);
};

const paymentMethodSubmitted = async () => {
  if (
    [
      AccountRoleTypes.IB,
      AccountRoleTypes.Broker,
      AccountRoleTypes.Sales,
    ].includes(accountRole.value)
  ) {
    await mountIntoIbSalesStep(4);
  } else {
    await mountIntoClientStep(6);
  }
};

const hide = () => {
  emits("stepFinished");
  openProcedureFormRef.value?.hide();
};

defineExpose({
  hide,
  show: showAccountProcedureForm,
});
</script>

<style scoped lang="scss"></style>
