<template>
  <div class="row">
    <div class="col">
      <UserInfo url="#" :user="userInfos" class="me-2" />
    </div>
    <div class="col text-end">
      <el-button
        plain
        type="primary"
        class="me-3"
        @click="showSendEmail(partyId)"
      >
        {{ $t("action.sendEmail") }}
      </el-button>
      <el-button
        class="me-3"
        plain
        type="danger"
        @click="requestUserToken"
        v-if="$cans(['TenantAdmin', 'WebGodMode'])"
      >
        {{ $t("fields.requestVisit") }}
      </el-button>

      <el-button
        class="me-3"
        plain
        type="warning"
        @click="lockUser"
        v-if="userInfos.lockoutEnd == null"
      >
        {{ $t("fields.lockUser") }}
      </el-button>
      <el-button v-else class="me-3" plain type="success" @click="unlockUser">
        {{ $t("fields.unlockUser") }}
      </el-button>

      <el-button
        v-if="quizLock == true"
        plain
        type="primary"
        @click="switchQuizLock()"
      >
        {{ $t("fields.lockQuiz") }}
      </el-button>
      <el-button v-else plain type="success" @click="switchQuizLock()">
        {{ $t("fields.unlockQuiz") }}
      </el-button>
    </div>
  </div>
  <hr />
  <div class="fv-row mb-7">
    <ul
      class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold"
    >
      <li class="nav-item">
        <a
          class="nav-link text-active-primary pb-4"
          :class="{ active: tab === UserDetailsType.infos }"
          data-bs-toggle="tab"
          href="#"
          @click="tab = UserDetailsType.infos"
          >{{ $t("title.info") }}</a
        >
      </li>

      <li class="nav-item">
        <a
          class="nav-link text-active-primary pb-4"
          :class="{ active: tab === UserDetailsType.documents }"
          data-bs-toggle="tab"
          href="#"
          @click="tab = UserDetailsType.documents"
          >{{ $t("title.documents") }}</a
        >
      </li>

      <li class="nav-item">
        <a
          class="nav-link text-active-primary pb-4"
          :class="{ active: tab === UserDetailsType.applications }"
          data-bs-toggle="tab"
          href="#"
          @click="tab = UserDetailsType.applications"
          >{{ $t("title.applications") }}</a
        >
      </li>

      <li class="nav-item">
        <a
          class="nav-link text-active-primary pb-4"
          :class="{ active: tab === UserDetailsType.accounts }"
          data-bs-toggle="tab"
          href="#"
          @click="tab = UserDetailsType.accounts"
          >{{ $t("title.account") }}</a
        >
      </li>

      <li class="nav-item">
        <a
          class="nav-link text-active-primary pb-4"
          :class="{ active: tab === UserDetailsType.wallet }"
          data-bs-toggle="tab"
          href="#"
          @click="tab = UserDetailsType.wallet"
          >{{ $t("title.wallet") }}</a
        >
      </li>

      <li class="nav-item">
        <a
          class="nav-link text-active-primary pb-4"
          :class="{ active: tab === UserDetailsType.paymentInfo }"
          data-bs-toggle="tab"
          href="#"
          @click="tab = UserDetailsType.paymentInfo"
          >{{ $t("title.paymentInfos") }}</a
        >
      </li>

      <li class="nav-item">
        <a
          class="nav-link text-active-primary pb-4"
          :class="{ active: tab === UserDetailsType.socialMedia }"
          data-bs-toggle="tab"
          href="#"
          @click="tab = UserDetailsType.socialMedia"
          >{{ $t("title.socialMedia") }}</a
        >
      </li>

      <li class="nav-item">
        <a
          class="nav-link text-active-primary pb-4"
          :class="{ active: tab === UserDetailsType.config }"
          data-bs-toggle="tab"
          href="#"
          @click="tab = UserDetailsType.config"
          >{{ $t("fields.userConfig") }}</a
        >
      </li>

      <li class="nav-item">
        <a
          class="nav-link text-active-primary pb-4"
          :class="{ active: tab === UserDetailsType.roles }"
          data-bs-toggle="tab"
          href="#"
          @click="tab = UserDetailsType.roles"
          >{{ $t("fields.userRoles") }}</a
        >
      </li>

      <li class="nav-item">
        <a
          class="nav-link text-active-primary pb-4"
          :class="{ active: tab === UserDetailsType.verifyCheck }"
          data-bs-toggle="tab"
          href="#"
          @click="tab = UserDetailsType.verifyCheck"
          >{{ $t("title.verification") }}</a
        >
      </li>

      <li class="nav-item">
        <a
          class="nav-link text-active-primary pb-4"
          :class="{ active: tab === UserDetailsType.apiLog }"
          data-bs-toggle="tab"
          href="#"
          @click="tab = UserDetailsType.apiLog"
          >ApiLog</a
        >
      </li>
    </ul>
  </div>
  <SendEmail ref="sendEmailRef" />
</template>
<script setup lang="ts">
import { ref, inject } from "vue";
import UserService from "../../modules/users/services/UserService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import Clipboard from "clipboard";
import SendEmail from "./SendEmail.vue";

const userInfos = inject<any>("userInfos");
const partyId = inject<any>("partyId");
const UserDetailsType = inject<any>("UserDetailsType");
const tab = inject<any>("tab");
const sendEmailRef = ref<any>(null);

const submitted = ref(true);
const requestUserTokenSubmitted = ref(false);
const quizLock = ref(false);

const showSendEmail = (partyId: string) => {
  sendEmailRef.value.show(partyId);
};

const requestUserToken = async () => {
  requestUserTokenSubmitted.value = true;
  try {
    const res = await UserService.requestUserToken(partyId.value);
    MsgPrompt.success(res.message).then(() => {
      Clipboard.copy(
        (process.env.VUE_APP_BASE_CDN_URL +
          "/set-token?token=" +
          res.token) as string
      );
      window.open(
        process.env.VUE_APP_BASE_CDN_URL + "/set-token?token=" + res.token,
        "_blank"
      );
    });
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    submitted.value = false;
  }
};

const lockUser = async () => {
  submitted.value = true;
  try {
    const res = await UserService.requestLockUser(partyId.value);
    MsgPrompt.success(res.message);
    userInfos.value.lockoutEnd = new Date();
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    submitted.value = false;
  }
};

const unlockUser = async () => {
  submitted.value = true;
  try {
    const res = await UserService.requestUnlockUser(partyId.value);
    MsgPrompt.success(res.message);
    userInfos.value.lockoutEnd = null;
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    submitted.value = false;
  }
};

const switchQuizLock = async () => {
  submitted.value = true;
  try {
    const res = await UserService.switchQuizLock(partyId.value);
    MsgPrompt.success(res.message);
    quizLock.value = !quizLock.value;
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    submitted.value = false;
  }
};
</script>
