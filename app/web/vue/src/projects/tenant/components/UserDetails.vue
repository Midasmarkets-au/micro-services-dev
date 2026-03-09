<template>
  <SiderDetail
    :title="$t('title.userDetails') + ' - ' + partyId"
    width="70%"
    direction="ltr"
    ref="userDetailShowRef"
    @close="onClose"
  >
    <div class="" v-if="!isLoading">
      <UserDetailsHeader />
      <div>
        <div>
          <UserPersonalInfos v-if="tab === UserDetailsType.infos" />
          <UserDocuments
            v-if="tab === UserDetailsType.documents"
            :verificationDetails="verificationDetails"
            :verifyOperation="false"
            :partyId="partyId"
            @update-verification-details="getUserVerificationDetails"
          />

          <UserApplications
            v-if="tab === UserDetailsType.applications"
            :partyId="partyId"
            @application-approved="tab = UserDetailsType.accounts"
          />

          <UserAccounts
            v-if="tab === UserDetailsType.accounts"
            :partyId="partyId"
          />

          <UserWallets
            v-if="tab === UserDetailsType.wallet"
            :partyId="partyId"
          />

          <PaymentInfo
            v-if="tab === UserDetailsType.paymentInfo"
            :partyId="partyId"
          />

          <UserContactInfo
            v-if="tab === UserDetailsType.socialMedia"
            :partyId="partyId"
          />

          <UserConfig
            v-if="tab === UserDetailsType.config"
            :rowId="partyId"
            :category="ConfigCategory.party"
            :siteId="userInfos.siteId"
          />

          <UserRole v-if="tab === UserDetailsType.roles" />

          <UserVerifyCheck
            v-if="tab === UserDetailsType.verifyCheck"
            :verification-details="verificationDetails"
          />

          <UserApiLog
            :partyId="partyId"
            v-if="tab === UserDetailsType.apiLog && $can('TenantAdmin')"
          />
        </div>
      </div>
    </div>
  </SiderDetail>
</template>

<script setup lang="ts">
import { ref, provide } from "vue";
import SiderDetail from "@/components/SiderDetail2.vue";
import UserDocuments from "../modules/users/components/UserDocuments.vue";
import UserAccounts from "../modules/users/components/UserAccounts.vue";
import UserService from "../modules/users/services/UserService";
import { VerificationTypes } from "@/core/types/VerificationInfos";
import UserApplications from "../modules/users/components/UserApplications.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import UserWallets from "@/projects/tenant/modules/users/components/UserWallets.vue";
import UserPersonalInfos from "@/projects/tenant/modules/users/components/UserPersonalInfos.vue";
import UserRole from "@/projects/tenant/modules/users/components/userRole/UserRole.vue";
import UserContactInfo from "@/projects/tenant/modules/users/components/UserContactInfo.vue";
import UserConfig from "@/projects/tenant/modules/system/components/config/ConfigList.vue";
import PaymentInfo from "@/projects/tenant/modules/Payment/views/PaymentInformation.vue";
import UserDetailsHeader from "./UserDetails/UserDetailsHeader.vue";
import { ConfigCategory } from "@/core/types/ConfigCategory";
import UserApiLog from "@/projects/tenant/modules/users/components/UserApiLog.vue";
import UserVerifyCheck from "@/projects/tenant/modules/users/components/UserVerifyCheck.vue";

enum UserDetailsType {
  infos = "infos",
  financial = "financial",
  documents = "documents",
  register = "register",
  accounts = "accounts",
  applications = "applications",
  paymentService = "paymentService",
  wallet = "wallet",
  paymentInfo = "paymentInfo",
  socialMedia = "socialMedia",
  config = "config",
  roles = "roles",
  apiLog = "apiLog",
  verifyCheck = "verifyCheck",
}

const isLoading = ref(true);
const submitted = ref(true);

const userDetailShowRef = ref<InstanceType<typeof SiderDetail>>();

// default tab is documents
const tab = ref<UserDetailsType | string>(UserDetailsType.infos);
const partyId = ref(-1);
const userInfos = ref({} as any);
provide("userInfos", userInfos);
provide("partyId", partyId);
provide("tab", tab);
provide("UserDetailsType", UserDetailsType);

const verificationDetails = ref<any>(null);

const show = async (_partyId: number, defaultTab?: string) => {
  userDetailShowRef.value?.show();
  partyId.value = _partyId;
  await getUserInfos();
  await getUserVerificationDetails();
  tab.value = defaultTab ?? UserDetailsType.infos;
  submitted.value = false;
};

const onClose = () => {
  tab.value = UserDetailsType.infos;
};

const getUserInfos = async () => {
  try {
    isLoading.value = true;
    userInfos.value = await UserService.getUserInfoByPartyId(partyId.value);
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

const getUserVerificationDetails = async () => {
  // 2. get user's verificationId
  isLoading.value = true;
  const res = await UserService.queryVerifications({
    type: VerificationTypes.Verification,
    partyId: partyId.value,
  });
  if (res.data.length === 0) {
    verificationDetails.value = null;
    isLoading.value = false;
    submitted.value = false;
    return;
  }
  const verificationId = res.data[0].id;

  // 3. get user's verification details by verificationId
  try {
    verificationDetails.value = await UserService.getVerificationById(
      verificationId
    );
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

/**
 * fetch user account infos,
 * if there are pending applications,
 * we need to show the application dropdown list to fill the application form
 * else we need to show the account list with a button to add a new account
 *
 */

// once the user is approved, we need to show the account tab
// const onUserApproved = () => {
//   verificationDetails.value.status = VerificationStatusTypes.Approved;
//   tab.value = UserDetailsType.applications;
// };

defineExpose({ show });
</script>
