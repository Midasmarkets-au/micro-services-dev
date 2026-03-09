<template>
  <div class="w-100 card verify-card" style="max-width: 880px; margin: auto">
    <div class="mb-8">
      <h2>{{ $t("fields.accountOpeningApplicationCompleted") }}</h2>
      <p>{{ $t("fields.thankYouForApply") }}</p>
    </div>

    <div>
      <h2>{{ $t("fields.nextStep") }}</h2>
    </div>
    <div>
      <h4>{{ $t("fields.accountOpeningApplicationRequest") }}</h4>
    </div>
    <ul class="mb-6 li-style">
      <li>{{ $t("fields.jpAccountRequest1") }}</li>
      <li>{{ $t("fields.jpAccountRequest2") }}</li>
    </ul>
    <div>
      <h4>{{ $t("fields.returnRequiredDocuments") }}</h4>
    </div>
    <ul class="li-style">
      <li>{{ $t("fields.jpReturnRequiredDocuments1") }}</li>
      <li>{{ $t("fields.jpReturnRequiredDocuments2") }}</li>
    </ul>
    <div class="d-flex flex-column">
      <el-input
        v-model="verificationCode"
        class="mt-4"
        style="width: 240px"
        :disabled="isLoading"
        :placeholder="$t('fields.authenticationNumber')"
        clearable
      />
      <el-button
        type="success"
        class="mt-4"
        style="width: 240px"
        @click="submit"
        :disabled="isLoading"
      >
        {{ $t("action.submit") }}
      </el-button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, inject } from "vue";
import VerificationService from "@/projects/client/modules/verification/services/VerificationService";
import { VerificationStatusTypes } from "@/core/types/VerificationInfos";
import notification from "@/core/plugins/notification";
const verificationCode = ref("");
const isLoading = ref(false);
const items = inject<any>("items");
const submit = async () => {
  isLoading.value = true;
  try {
    await VerificationService.codeVerify({
      code: verificationCode.value,
      hashId: items.value.data.hashId,
    });
    items.value.data.status = VerificationStatusTypes.CodeVerified;
    notification.success();
  } catch (error) {
    console.error(error);
    notification.danger();
  } finally {
    isLoading.value = false;
  }
};
</script>
<style scoped lang="scss">
.verify-card {
  --primary-color: #8a7633;
  --primary-light: #a08f4d;
  --primary-dark: #695a27;
  --primary-bg: #f7f5ed;

  .li-style {
    list-style: none;
    padding-left: 1.5em;
    margin-bottom: 2rem;
  }

  .li-style li {
    position: relative;
    color: #161c20;
    padding: 0.5rem 0;
    line-height: 1.5;
  }

  .li-style li::before {
    content: "※";
    position: absolute;
    left: -1.5em;
    width: 1.5em;
    text-align: center;
    color: var(--primary-color);
  }

  .li-style li:hover {
    background-color: #f9fafb;
    transition: background-color 0.2s ease;
  }

  h2 {
    color: var(--primary-color);
    font-size: 1.5rem;
    font-weight: 600;
    margin-bottom: 1rem;
    border-bottom: 2px solid var(--primary-light);
    padding-bottom: 0.5rem;
  }

  h4 {
    color: var(--primary-dark);
    font-size: 1.1rem;
    font-weight: 500;
    margin-top: 1.5rem;
    margin-bottom: 0.75rem;
  }

  /* Paragraph styles */
  p {
    color: #161c20;
    line-height: 1.6;
    margin-bottom: 1.5rem;
  }
}
</style>
