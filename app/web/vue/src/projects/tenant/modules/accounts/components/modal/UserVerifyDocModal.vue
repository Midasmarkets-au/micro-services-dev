<template>
  <SimpleForm2
    ref="modalRef"
    :min-height="700"
    :min-width="1300"
    :title="$t('title.verifications')"
    :is-loading="isLoading"
    :disable-submit="
      verificationDetails.status !== VerificationStatusTypes.AwaitingApprove
    "
    :disable-discard="
      verificationDetails.status !== VerificationStatusTypes.AwaitingApprove
    "
    :save-title="$t('action.approve')"
    :submit="openApprovePanel"
    :discard="openRejectPanel"
    discard-color="danger"
    :discard-title="$t('action.reject')"
    append-body
    :delay-review="true"
    :delay-review-handler="openDelayReviewPanel"
  >
    <div class="px-5">
      <div class="d-flex justify-content-between">
        <div style="width: 400px">
          <el-menu
            :default-active="currentTab"
            class="w-100 mb-5"
            mode="horizontal"
            @select="handleSelect"
          >
            <el-menu-item :index="VerificationTab.Document">{{
              $t("title.document")
            }}</el-menu-item>
            <!--            <el-menu-item :index="VerificationTab.PersonalInfos"-->
            <!--              >Personal Infos</el-menu-item-->
            <!--            >-->

            <el-sub-menu index="1111111">
              <template #title
                ><span>{{ $t("fields.verficationInfos") }}</span></template
              >
              <el-menu-item :index="VerificationTab.Started">{{
                $t("fields.started")
              }}</el-menu-item>
              <el-menu-item :index="VerificationTab.Financial">{{
                $t("fields.financial")
              }}</el-menu-item>
              <el-menu-item :index="VerificationTab.Quiz">{{
                $t("fields.quiz")
              }}</el-menu-item>
              <el-menu-item :index="VerificationTab.Agreement">{{
                $t("fields.agreement")
              }}</el-menu-item>
              <el-menu-item :index="VerificationTab.Info">{{
                $t("fields.info")
              }}</el-menu-item>
            </el-sub-menu>
          </el-menu>
        </div>
        <div class="d-flex align-items-center">
          <!-- <a href="#" class="btn btn-primary btn-sm">File Manager</a> -->

          <UserInfo
            v-if="
              verificationDetails.status !==
              VerificationStatusTypes.AwaitingReview
            "
            :user="user"
            :sub="$t('fields.operator')"
            class="me-2"
          />

          <button
            class="btn btn-warning text-black-50 me-5"
            @click="openRejectPanel"
          >
            {{ $t("action.sendDocumentsRejectionEmail") }}
          </button>
          <div
            class="nav bg-light rounded-pill px-2 py-1"
            data-kt-buttons="true"
            data-kt-initialized="1"
          >
            <button
              class="nav-link btn btn-active btn-active-dark fw-bold btn-color-gray-600 py-2 px-5 m-1 rounded-pill"
              :class="{
                active:
                  verificationDetails.status ===
                  VerificationStatusTypes.AwaitingReview,
              }"
              @click="
                changeVerifyingState(VerificationStatusTypes.AwaitingReview)
              "
              style="transition: background-color 0.3s ease-in-out 0s"
            >
              {{ $t("status.pending") }}
            </button>
            <button
              class="nav-link btn btn-active btn-active-dark fw-bold btn-color-gray-600 py-2 px-5 m-1 rounded-pill"
              :class="{
                active:
                  verificationDetails.status ===
                  VerificationStatusTypes.UnderReview,
              }"
              @click="changeVerifyingState(VerificationStatusTypes.UnderReview)"
              style="transition: background-color 0.3s ease-in-out 0s"
            >
              {{ $t("status.reviewing") }}
            </button>
            <button
              class="nav-link btn btn-active btn-active-dark fw-bold btn-color-gray-600 py-2 px-5 m-1 rounded-pill"
              :class="{
                active:
                  verificationDetails.status ===
                  VerificationStatusTypes.AwaitingApprove,
              }"
              @click="
                changeVerifyingState(VerificationStatusTypes.AwaitingApprove)
              "
              style="transition: background-color 0.3s ease-in-out 0s"
            >
              {{ $t("status.reviewed") }}
            </button>
          </div>
        </div>
      </div>
      <div class="d-flex" style="justify-content: flex-end">
        <el-button
          class="me-9"
          type="warning"
          @click="showMoveModal()"
          v-if="$can('TenantAdmin')"
        >
          {{ $t("action.moveUser") }}
        </el-button>
        <el-form-item
          :label="t('fields.site')"
          prop="siteID"
          style="width: 200px"
        >
          <el-input v-model="showSiteID" disabled></el-input>
        </el-form-item>

        <el-button
          class="ms-4"
          type="primary"
          @click="
            changeSiteModalRef?.show(
              verificationDetails.partyId,
              verificationDetails.siteId
            )
          "
          >{{ $t("action.update") }}</el-button
        >
      </div>
      <div
        v-if="
          verificationDetails.started && verificationDetails.started.referral
        "
        class="d-flex"
        style="justify-content: flex-end"
      >
        <div class="me-5">
          <div class="mb-2">
            Referral Code: <b>{{ verificationDetails.started.referral }}</b>
          </div>
          <div>
            Sales Code:
            <b v-if="Object.keys(referInfo).length != 0">{{
              referInfo.salesAccountCode
            }}</b>
          </div>
        </div>
        <div>
          <div class="mb-2">
            Open For:
            <b v-if="Object.keys(referInfo).length != 0">{{
              $t(`type.accountRole.${referInfo.serviceFor}`)
            }}</b>
          </div>
          <div>
            IB Group:
            <b v-if="Object.keys(referInfo).length != 0">{{
              referInfo.accountGroupName
            }}</b>
          </div>
        </div>
      </div>
      <div class="">
        <UserStart
          v-if="!isLoading && VerificationTab.Started === currentTab"
          :verification-details="verificationDetails"
        />

        <UserInfos
          v-if="!isLoading && VerificationTab.Info === currentTab"
          :verification-details="verificationDetails"
        />
        <UserFinancial
          :verification-details="verificationDetails"
          v-if="!isLoading && VerificationTab.Financial === currentTab"
        />

        <UserQuizs
          :verification-details="verificationDetails"
          v-if="!isLoading && VerificationTab.Quiz === currentTab"
        />

        <UserAgreement
          :verification-details="verificationDetails"
          v-if="!isLoading && VerificationTab.Agreement === currentTab"
        />
        <UserDocuments
          v-if="!isLoading && VerificationTab.Document === currentTab"
          :verification-details="verificationDetails"
          :verifyOperation="true"
          @update-verification-details="
            loadUserVerifications(verificationDetails.id)
          "
          :partyId="verificationDetails.partyId"
          @user-verification-state-change="handleUserVerificationStatusChange"
          height="625px"
        />
      </div>
    </div>
    <ConfirmBox
      ref="confirmBoxRef"
      :is-loading="isLoading"
      cancel-color="reject"
    />
    <RejectReasonBox ref="rejectReasonBoxRef" />
  </SimpleForm2>

  <ChangeSiteModal
    ref="changeSiteModalRef"
    @updateSiteID="updateSiteID"
  ></ChangeSiteModal>
  <ShowMoveModal ref="moveModalRef" @user-moved="emits('user-moved')" />
</template>

<script setup lang="ts">
import UserDocuments from "@/projects/tenant/modules/users/components/UserDocuments.vue";
import { nextTick, ref } from "vue";
import UserService from "@/projects/tenant/modules/users/services/UserService";
import SimpleForm from "@/components/SimpleForm.vue";
import UserStart from "@/projects/tenant/modules/users/components/UserStart.vue";
import UserInfos from "@/projects/tenant/modules/users/components/UserInfos.vue";
import UserFinancial from "@/projects/tenant/modules/users/components/UserFinancial.vue";
import UserQuizs from "@/projects/tenant/modules/users/components/UserQuizs.vue";
import UserAgreement from "@/projects/tenant/modules/users/components/UserAgreement.vue";
import SimpleForm2 from "@/components/SimpleForm.vue";
import { VerificationStatusTypes } from "@/core/types/VerificationInfos";
import ConfirmBox from "@/components/ConfirmBox.vue";
import { useStore } from "@/store";
import RejectReasonBox from "@/components/RejectReasonBox.vue";
import { useI18n } from "vue-i18n";
import ChangeSiteModal from "@/projects/tenant/modules/users/components/modal/ChangeSiteModal.vue";
import GlobalService from "@/projects/tenant/services/TenantGlobalService";
import { ReferralServiceTypes } from "@/core/types/ReferralServiceTypes";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import { ElNotification } from "element-plus";
import ShowMoveModal from "./ShowMoveModal.vue";
const { t } = useI18n();
const enum VerificationTab {
  Started = "Started",
  Info = "Info",
  Financial = "Financial",
  Quiz = "Quiz",
  Agreement = "Agreement",
  Document = "Document",
  PersonalInfos = "Personal",
}
const store = useStore();
const user = store.state.AuthModule.user;
const modalRef = ref<InstanceType<typeof SimpleForm>>();
const changeSiteModalRef = ref<InstanceType<typeof ChangeSiteModal>>();
const referInfo = ref<any>({});

const emits = defineEmits<{
  (e: "user-approved"): void;
  (e: "user-moved"): void;
}>();
const isLoading = ref(true);
const currentTab = ref(VerificationTab.Document);
const verificationDetails = ref<any>({});
const showSiteID = ref("");
const handleUserVerificationStatusChange = (
  _status: VerificationStatusTypes
) => {
  verificationDetails.value.status = _status;
  // nextTick(() => loadUserVerifications(verificationDetails.value.id));
};
const moveModalRef = ref<InstanceType<typeof ShowMoveModal>>();
const showMoveModal = () => {
  moveModalRef.value?.show(verificationDetails.value.partyId, user);
};

const loadUserVerifications = async (id) => {
  try {
    isLoading.value = true;
    verificationDetails.value = await UserService.getVerificationById(id);
    showSiteID.value = t("type.siteType." + verificationDetails.value.siteId);
  } catch (error) {
    ElNotification.error(error);
  } finally {
    isLoading.value = false;
  }
};

const confirmBoxRef = ref<InstanceType<typeof ConfirmBox>>();
const rejectReasonBoxRef = ref<InstanceType<typeof RejectReasonBox>>();
const openApprovePanel = () => {
  confirmBoxRef.value?.show(() =>
    UserService.putUserToApproved(verificationDetails.value.id)
      // Promise.resolve()
      .then(() => ElNotification.success("User verification approved!"))
      .then(() => confirmBoxRef.value?.hide())
      .then(() => hide())
      .then(() => emits("user-approved"))
  );
};

const changeVerifyingState = async (_status: VerificationStatusTypes) => {
  if (verificationDetails.value.status === _status) {
    return;
  }

  try {
    isLoading.value = true;
    await {
      [VerificationStatusTypes.AwaitingReview]: () =>
        UserService.putUserToAwaitingReview(verificationDetails.value.id),
      [VerificationStatusTypes.UnderReview]: () =>
        UserService.putUserToUnderReview(verificationDetails.value.id),
      [VerificationStatusTypes.AwaitingApprove]: () =>
        UserService.putUserToAwaitingApprove(verificationDetails.value.id),
      [VerificationStatusTypes.Rejected]: () =>
        UserService.putUserToRejected(verificationDetails.value.id),
    }[_status]();
    verificationDetails.value.status = _status;
  } catch (err) {
    ElNotification.error(err);
  } finally {
    isLoading.value = false;
  }
};

// https://pro.t.api.mybcr.dev/api/v1/tenant/verification/137/reject-notice

const openRejectPanel = () => {
  confirmBoxRef.value?.show(
    () =>
      UserService.sendRejectDocumentEmail(verificationDetails.value.id)
        // Promise.resolve()
        .then(() =>
          ElNotification.success("User verification reject email sent!")
        )
        .then(() => confirmBoxRef.value?.hide())
        .then(() => hide())
        .then(() => emits("user-approved")),
    "Ready to send reject notice email?"
  );
};

const handleSelect = (key: VerificationTab, keyPath: string[]) => {
  currentTab.value = key;
};

const updateSiteID = async (_siteId: number) => {
  showSiteID.value = t("type.siteType." + _siteId);
};

const getReferCodeDetail = async () => {
  if (
    verificationDetails.value.started !== null &&
    verificationDetails.value.started.referral !== ""
  ) {
    referInfo.value = await GlobalService.getReferralCodeAccountInfo(
      verificationDetails.value.started.referral
    );
    referInfo.value.serviceFor = getAccountRoleDesignatedByReferCode(
      referInfo.value.serviceType
    );
  }
};

const getAccountRoleDesignatedByReferCode = (serviceType) =>
  ({
    [ReferralServiceTypes.Broker]: AccountRoleTypes.IB,
    [ReferralServiceTypes.Agent]: AccountRoleTypes.IB,
    [ReferralServiceTypes.Client]: AccountRoleTypes.Client,
    // for legacy data
    [200]: AccountRoleTypes.IB,
    [300]: AccountRoleTypes.IB,
    [400]: AccountRoleTypes.Client,
  }[serviceType] ?? AccountRoleTypes.Client);

const show = async (id: number) => {
  await loadUserVerifications(id);
  try {
    await getReferCodeDetail();
  } catch (error) {
    // MsgPrompt.error(error);
  }

  // verificationDetails change in loadUserVerifications,
  // wait for nextTick to show modal to display the proper verification details
  await nextTick();
  modalRef.value?.show();
};
const openDelayReviewPanel = () => {
  confirmBoxRef.value?.show(() =>
    UserService.putUserToDelayReview(verificationDetails.value.id)
      // Promise.resolve()
      .then(() => ElNotification.success("User verification delay review!"))
      .then(() => confirmBoxRef.value?.hide())
      .then(() => hide())
      .then(() => emits("user-approved"))
  );
};
const hide = () => {
  modalRef.value?.hide();
};

defineExpose({
  show,
  hide,
});
</script>

<style scoped></style>
