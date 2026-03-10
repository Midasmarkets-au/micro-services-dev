<template>
  <el-dialog
    v-model="dialogRef"
    :title="
      $t('title.verifications') + ' - ' + userName + ' - ' + verificationId
    "
    width="1100"
    align-center
    :before-close="hide"
  >
    <div style="height: 80vh; overflow: auto">
      <div>
        <ModalMenu />
      </div>
      <div
        v-if="isLoading"
        class="d-flex align-items-center justify-content-center h-600px"
      >
        <scale-loader :color="'#ffc730'"></scale-loader>
      </div>
      <div v-else style="position: relative">
        <el-form
          ref="ruleFormRef"
          class="pb-10"
          :model="formData"
          :rules="rules"
          :hide-required-asterisk="true"
          label-width="auto"
          label-position="top"
          style="z-index: 9"
        >
          <JpInfo v-if="currentTab === VerificationTab.Info" />
          <JpFinancial v-if="currentTab === VerificationTab.Financial" />
          <JpAgreement v-if="currentTab === VerificationTab.Agreement" />
          <JpReview v-if="currentTab === VerificationTab.Review" />
          <JpPdf v-if="currentTab === VerificationTab.PDF" />
          <JpVerificationCode
            v-if="currentTab === VerificationTab.VerificationCode"
          />
        </el-form>
      </div>
    </div>
    <template #footer>
      <div class="d-flex justify-content-between">
        <div>
          <el-button
            type="warning"
            @click="showMoveModal()"
            v-if="$can('SuperAdmin')"
          >
            {{ $t("action.moveUser") }}
          </el-button>

          <el-button
            type="primary"
            @click="convertToPdf()"
            :disabled="isLoading"
            :loading="isPrinting"
          >
            {{ $t("action.downloadPdf") }}
          </el-button>
          <el-button
            type="info"
            @click="downloadCode()"
            :disabled="isLoading"
            :loading="isPrinting"
            v-if="code && code !== '〇〇〇〇'"
          >
            {{ $t("action.downloadCode") }}
          </el-button>
        </div>

        <div class="d-flex justify-content-end">
          <!-- <el-button
            v-if="
              verificationDetails.status ===
              VerificationStatusTypes.AwaitingCodeVerify
            "
            type="info"
            @click="downloadCode()"
            :disabled="isLoading"
            :loading="isPrinting"
          >
            {{ $t("action.downloadCode") }}
          </el-button> -->
          <div v-for="(button, index) in buttonConfigs" :key="index">
            <el-button
              v-if="
                button.show.includes(verificationDetails.status) ||
                button.show.length === 0
              "
              :type="button.type"
              class="ms-5"
              @click="button.action"
            >
              {{ button.label }}
            </el-button>
          </div>
        </div>
      </div>
    </template>
  </el-dialog>

  <ShowMoveModal ref="moveModalRef" @user-moved="emits('user-moved')" />
</template>
<script lang="ts" setup>
import {
  ref,
  provide,
  reactive,
  nextTick,
  inject,
  computed,
  onMounted,
} from "vue";
import ModalMenu from "./jpVerifyModal/ModalMenu.vue";
import JpInfo from "./jpVerifyModal/JpInfo.vue";
import JpFinancial from "./jpVerifyModal/JpFinancial.vue";
import JpAgreement from "./jpVerifyModal/JpAgreement.vue";
import JpReview from "./jpVerifyModal/JpReview.vue";
import JpPdf from "./jpVerifyModal/JpPdf.vue";
import JpVerificationCode from "./jpVerifyModal/JpVerificationCode.vue";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import type { FormInstance } from "element-plus";
import html2pdf from "html2pdf.js";
import notification from "@/core/plugins/notification";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";
import { VerificationStatusTypes } from "@/core/types/VerificationInfos";
import { ElNotification } from "element-plus";
import i18n from "@/core/plugins/i18n";
import ShowMoveModal from "@/projects/tenant/modules/accounts/components/modal/ShowMoveModal.vue";
import { useStore } from "@/store";
import UserService from "@/projects/tenant/modules/users/services/UserService";
import { getJpAccountType } from "@/core/types/jp/verificationFinancial";

const t = i18n.global.t;
const VerificationTab = {
  Info: "Info",
  Financial: "Financial",
  Agreement: "Agreement",
  Review: "Review",
  PDF: "PDF",
  VerificationCode: "VerificationCode",
};

const currentTab = ref(VerificationTab.PDF);
const verificationDetails = ref<any>({});
const isLoading = ref(false);
const disabled = ref(false);
const isPrinting = ref(false);
const formData = ref<any>({});
const store = useStore();
const user = store.state.AuthModule.user;
const userName = ref("");
const verificationId = ref(0);
const partyId = ref(0);
const code = ref("〇〇〇〇");

provide("code", code);
provide("disabled", disabled);
provide("isLoading", isLoading);
provide("currentTab", currentTab);
provide("VerificationTab", VerificationTab);
provide("verificationDetails", verificationDetails);
provide("formData", formData);

const dialogRef = ref(false);
const ruleFormRef = ref<FormInstance>();
const rules = reactive<any>({});

const emits = defineEmits<{
  (e: "fetch-data"): void;
  (e: "user-moved"): void;
}>();

const convertToPdf = async () => {
  currentTab.value = VerificationTab.PDF;
  await nextTick();
  isPrinting.value = true;
  try {
    const element = document.getElementById("print") as HTMLElement;
    const opt = {
      margin: [1, 0.2],
      filename: `Verification ${userName.value}.pdf`,
      image: { type: "jpeg", quality: 0.98 },
      html2canvas: { scale: 2, useCORS: true, logging: true },
      jsPDF: { unit: "in", format: "a3", orientation: "portrait" },
    };
    await html2pdf().from(element).set(opt).save();
    notification.success();
  } catch (error) {
    console.log("error", error);
    notification.danger();
  } finally {
    isPrinting.value = false;
  }
};

const downloadCode = async () => {
  currentTab.value = VerificationTab.VerificationCode;
  await nextTick();
  isPrinting.value = true;
  try {
    const element = document.getElementById("print") as HTMLElement;
    const opt = {
      margin: [0.5, 0.2],
      filename: `Code ${userName.value}.pdf`,
      image: { type: "jpeg", quality: 0.98 },
      html2canvas: { scale: 2, useCORS: true, logging: true },
      jsPDF: { unit: "in", format: "a4", orientation: "portrait" },
    };
    await html2pdf().from(element).set(opt).save();
    notification.success();
  } catch (error) {
    console.log("error", error);
    notification.danger();
  } finally {
    isPrinting.value = false;
  }
};

const generateCode = async () => {
  isLoading.value = true;

  try {
    await UserService.putUserToAwaitingCodeVerify(verificationId.value);
    await UserService.generateMailCode(verificationDetails.value.id);
    await nextTick();
    currentTab.value = VerificationTab.VerificationCode;
  } catch (error) {
    console.log("error", error);
    notification.danger();
  } finally {
    isLoading.value = false;
  }
};

const loadUserVerifications = async (id) => {
  try {
    isLoading.value = true;
    verificationDetails.value = await UserService.getVerificationById(id);
    var info = verificationDetails.value.info;
    userName.value = info.firstName + " " + info.lastName;
    verificationId.value = verificationDetails.value.id;
    partyId.value = verificationDetails.value.partyId;
    changeCurrentTab();
  } catch (error) {
    console.log("error", error);
    notification.danger();
  } finally {
    isLoading.value = false;
  }
};

const changeCurrentTab = () => {
  if (
    verificationDetails.value.status ===
    VerificationStatusTypes.AwaitingCodeVerify
  ) {
    currentTab.value = VerificationTab.VerificationCode;
  }
};

const openConfirmModal = inject<any>(
  TenantGlobalInjectionKeys.OPEN_CONFIRM_MODAL
);

const moveModalRef = ref<InstanceType<typeof ShowMoveModal>>();
const showMoveModal = () => {
  moveModalRef.value?.show(verificationDetails.value.partyId, user);
};

const openConfirmPanel = (_action: VerificationStatusTypes) => {
  openConfirmModal?.(() => {
    return {
      [VerificationStatusTypes.AwaitingAddressVerify]: () =>
        UserService.putUserToAwaitingVerify(verificationId.value),

      [VerificationStatusTypes.AwaitingCodeVerify]: () => generateCode(),

      [VerificationStatusTypes.Approved]: () =>
        UserService.putUserToApproved(verificationId.value),

      [VerificationStatusTypes.Rejected]: () => rejectVerification(),
    }
      [_action]()
      .then(() => {
        verificationDetails.value.status = _action;
        ElNotification({
          title: t("status.success"),
          message: t("status.success"),
          type: "success",
        });
        emits("fetch-data");
      });
  });
};

const buttonConfigs = computed(() => [
  {
    label: t("action.sendIdVerification"),
    type: "primary",
    show: [VerificationStatusTypes.AwaitingReview],
    action: () =>
      openConfirmPanel(VerificationStatusTypes.AwaitingAddressVerify),
  },

  {
    label: t("action.generateCode"),
    type: "warning",
    show: [VerificationStatusTypes.AwaitingAddressVerify],
    action: () => openConfirmPanel(VerificationStatusTypes.AwaitingCodeVerify),
  },
  {
    label: t("action.approve"),
    type: "success",
    show: [],
    action: () => approveAndCreateApplications(),
  },
  {
    label: t("action.reject"),
    type: "danger",
    show: [
      VerificationStatusTypes.AwaitingReview,
      VerificationStatusTypes.AwaitingAddressVerify,
    ],
    action: () => openConfirmPanel(VerificationStatusTypes.Rejected),
  },
]);

const approveAndCreateApplications = async () => {
  isLoading.value = true;
  try {
    await UserService.putUserToApproved(verificationId.value);
    await createApplications();
    // emits("fetch-data");
  } catch (error) {
    console.log("error", error);
    notification.danger();
  } finally {
    isLoading.value = false;
  }
};

const createApplications = async () => {
  try {
    var accountTypes = verificationDetails.value.financial.accountTypes;
    var accountRole = verificationDetails.value.financial.accountRole;
    console.log("account Types : ", accountTypes);
    console.log("account Role : ", accountRole);
    await accountTypes.forEach((accountType) => {
      var jpAccountType = getJpAccountType(accountRole, accountType);
      console.log("jpAccountType : ", jpAccountType);
      var applicationData = {
        accountType: jpAccountType,
        currencyId: 392,
        leverage: 20,
        serviceId: 30,
        platform: 30,
      };
      UserService.openTradeAccount(
        verificationDetails.value.partyId,
        applicationData
      );
    });
    notification.success();
    emits("fetch-data");
  } catch (error) {
    console.log("error", error);
    notification.danger();
  } finally {
    isLoading.value = false;
  }
};

const rejectVerification = async () => {
  isLoading.value = true;
  try {
    await UserService.putUserToRejected(verificationId.value);
    await UserService.sendRejectDocumentEmail(verificationId.value);
    ElNotification.success(t("tip.emailSent"));
    emits("fetch-data");
  } catch (error) {
    console.log("error", error);
    notification.danger();
  } finally {
    isLoading.value = false;
  }
};
const show = async (id: number) => {
  dialogRef.value = true;
  await loadUserVerifications(id);
};

const hide = () => {
  dialogRef.value = false;
  currentTab.value = VerificationTab.PDF;
};

defineExpose({
  show,
  hide,
});
</script>
<style scoped>
.dialog-footer {
  border-top: 1px solid #929499;
  padding-top: 10px;
}
</style>
