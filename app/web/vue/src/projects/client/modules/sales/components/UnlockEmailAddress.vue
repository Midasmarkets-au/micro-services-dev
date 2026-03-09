<template>
  <div
    class="modal fade"
    id="kt_modal_unlock_email_address"
    tabindex="-1"
    aria-hidden="true"
    ref="UnlockEmailAddressRef"
  >
    <div class="modal-dialog modal-dialog-centered mw-500px">
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
          <table v-if="isLoading" class="table align-middle table-row-bordered">
            <tbody>
              <LoadingRing />
            </tbody>
          </table>
          <div
            v-else
            class="d-flex flex-column align-items-center justify-content-center pt-7 pb-7"
          >
            <div
              class="mb-2 fs-3 d-flex justify-content-center align-items-center"
            >
              <div
                class="d-flex justify-content-center pt-2 pb-2 ps-3 pe-3"
                style="
                  border: 1px solid #e0e0e0;
                  min-width: 350px;
                  border-radius: 5px;
                "
              >
                {{ emailAddress }}
              </div>

              <!-- <TinyCopyBox
                class="p-1 ms-3"
                :val="emailAddress.toString()"
              ></TinyCopyBox> -->
            </div>

            <div
              class="d-flex justify-content-center p-1 mb-7"
              style="
                border: 1px solid #e0e0e0;
                min-width: 350px;
                border-radius: 100px;
                background-color: cornsilk;
                color: #900000;
              "
            >
              {{ showMessage }}
            </div>

            <el-button
              v-if="!ableToCheck"
              type="warning"
              class="w-85px"
              @click="sendCode"
              :loading="isLoading"
              :disabled="isSubmitting"
            >
              {{ $t("action.sendCode") }}
            </el-button>

            <div v-if="ableToCheck">
              <div class="d-flex">
                <div>
                  <el-input
                    clearable
                    class="w-150px h-40px"
                    :placeholder="$t('fields.oneTimeCode')"
                    v-model="verificationCode"
                    :disabled="isLoading"
                  />
                </div>
                <span>
                  <el-button
                    type="primary"
                    class="w-75px ms-4 me-3 mt-1"
                    @click="submit"
                    :loading="isLoading"
                    :disabled="isSubmitting"
                  >
                    {{ $t("action.confirm") }}
                  </el-button>
                </span>

                <span>
                  <el-button
                    type="warning"
                    class="w-85px mt-1"
                    @click="reSendCode"
                    :loading="isLoading"
                    :disabled="isSubmitting"
                  >
                    {{ $t("action.sendCode") }}
                  </el-button>
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import SalesService from "@/projects/client/modules/sales/services/SalesService";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import { useStore } from "@/store";
import TinyCopyBox from "@/components/TinyCopyBox.vue";
import { ref, computed } from "vue";
import { showModal } from "@/core/helpers/dom";
import { getLanguage } from "@/core/types/LanguageTypes";
import { processKeysToCamelCase } from "@/core/services/api.client";
import { ReferralServiceType } from "@/core/types/ReferralServiceType";
import i18n from "@/core/plugins/i18n";

const t = i18n.global.t;
const store = useStore();
const data = ref(<any>[]);
const emailAddress = ref("");
const uid = ref("");
const showMessage = ref("t('tip.needVerificationCode')");
const isLoading = ref(false);
const isSubmitting = ref(false);
const verificationCode = ref("");
const title = ref(t("tip.emailAddress"));
const salesAccount = computed(() => store.state.SalesModule.salesAccount);
const UnlockEmailAddressRef = ref<null | HTMLElement>(null);
const ableToCheck = ref(false);

const show = async (_uid: any, _email: any) => {
  console.log("show");
  showMessage.value = t("tip.needVerificationCode");
  ableToCheck.value = false;
  emailAddress.value = _email;
  uid.value = _uid;

  showModal(UnlockEmailAddressRef.value);
};

const sendCode = async () => {
  isLoading.value = true;

  try {
    const res = await SalesService.getViewEmailCode(uid.value);
    ableToCheck.value = true;
    showMessage.value = t("tip.codeSentToEmail");
  } catch (e) {
    if (e.response.data == "CODE_ALREADY_SENT") {
      ableToCheck.value = true;
    } else {
      ableToCheck.value = false;
    }
    showMessage.value = t("error." + e.response.data);
  }

  verificationCode.value = "";
  isLoading.value = false;
};

const reSendCode = async () => {
  isSubmitting.value = true;

  try {
    const res = await SalesService.getViewEmailCode(uid.value);
    showMessage.value = t("tip.codeSentToEmail");
  } catch (e) {
    showMessage.value = t("error." + e.response.data);
  }

  verificationCode.value = "";
  isSubmitting.value = false;
};

const submit = async () => {
  isSubmitting.value = true;

  if (!verificationCode.value) {
    showMessage.value = t("tip.verificationCodeRequired");
    isSubmitting.value = false;
    return;
  }

  try {
    const res = await SalesService.getEmailByCode(
      uid.value,
      verificationCode.value
    );
    emailAddress.value = res;
    showMessage.value = t("status.success");
  } catch (e) {
    showMessage.value = t("error." + e.response.data);
  }

  verificationCode.value = "";
  isSubmitting.value = false;
};

defineExpose({
  show,
});
</script>
