<template>
  <div class="verify-email-wrap">
    <div class="verify-email-notice">
      <div class="title">{{ $t("tip.verifyYourEmail") }}</div>
      <div class="final-wrap mt-9">
        <div>
          <div class="finalMessage">
            {{ $t("tip.thankSignUpAndConfirm") }}
            <a href="#">{{ formData.email }}</a>
            {{ $t("tip.toActivateYourAccount") }}
          </div>
          <br />
          <div class="finalMessage">
            {{ $t("tip.linkExpireContact") }}
          </div>
          <br />
          <div class="finalMessage">
            {{ $t("tip.notReceiveEmail") }}<br />
            <a href="#" @click="resendConfirmation">
              {{ $t("action.resendEmail") }}</a
            >
          </div>
        </div>
        <img src="/images/auth/signup-final-icon.png" alt="" />
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, inject } from "vue";
import i18n from "@/core/plugins/i18n";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import GlobalService from "@/projects/client/services/ClientGlobalService";

const { t } = i18n.global;
const formData = inject<any>("formData");

const resendConfirmation = async () => {
  var confirmUrl =
    window.location.protocol + "//" + window.location.host + "/confirm-email";
  if (window.location.href.includes("portal")) {
    confirmUrl =
      window.location.protocol +
      "//" +
      window.location.host +
      "/portal/confirm-email";
  }
  try {
    await GlobalService.resendConfirmationEmail(
      formData.value.email,
      confirmUrl
    );

    MsgPrompt.success(t("tip.confirmationEmailResend"));
  } catch (error) {
    MsgPrompt.error(error);
  }
};
</script>
