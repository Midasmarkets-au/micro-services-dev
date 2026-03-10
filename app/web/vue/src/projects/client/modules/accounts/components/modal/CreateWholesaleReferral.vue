<!-- Hank Refactor on 02/01/2024 -->
<template>
  <div class="modal fade" ref="wholesaleFriendRef">
    <div class="modal-dialog modal-dialog-centered mw-450px">
      <div class="modal-content">
        <div class="form fv-plugins-bootstrap5 fv-plugins-framework">
          <!-- ============================================================================= -->
          <!------------------------------------------------------------------- Modal Header -->
          <!-- ============================================================================= -->
          <div class="modal-header">
            <h2
              class="fs-2"
              :class="{
                'fs-1': isMobile,
              }"
            >
              {{ $t("title.bcrProReferral") }}
            </h2>
            <div data-bs-dismiss="modal">
              <span class="svg-icon svg-icon-1">
                <inline-svg src="/images/icons/arrows/arr061.svg" />
              </span>
            </div>
          </div>

          <div>
            <form v-if="!isLoading" class="form-style" style="height: 400px">
              <!-- ===================================================================================== -->
              <div
                class="d-flex align-items-center justify-content-center ps-3 pe-3 pt-2 pb-2 mb-5"
                style="background-color: cornsilk; border-radius: 10px"
              >
                {{ $t("tip.enterFriendInfo") }}
              </div>
              <div class="row info-part">
                <div>
                  <!-- ---------------------------------------------------------------->
                  <div class="fv-row mb-5">
                    <label
                      class="required fs-7 fw-semobold mb-2 createAccountTitle"
                    >
                      {{ $t("tip.emailAddress") }}
                    </label>
                    <div class="d-flex align-items-center">
                      <Field
                        v-model="formData.email"
                        name="email"
                        style="border: 1px solid #dcdcdc"
                      >
                        <el-input v-model="formData.email" name="email" />
                      </Field>
                      <i
                        v-if="availableAccount"
                        class="fa-regular fa-xl fa-circle-check ms-3"
                        style="color: #14a44d"
                      ></i>
                      <i
                        v-else
                        class="fa-regular fa-xl fa-circle-check ms-3"
                        style="color: lightgray"
                      ></i>
                    </div>

                    <ErrorMessage
                      as="div"
                      class="fv-plugins-message-container invalid-feedback"
                      name="email"
                    />
                  </div>
                  <!-- ---------------------------------------------------------------->
                  <!-- ---------------------------------------------------------------->
                  <div class="fv-row mb-2">
                    <label
                      class="required fs-7 fw-semobold mb-2 createAccountTitle"
                    >
                      {{ $t("fields.accountNumber") }}
                    </label>
                    <div class="d-flex align-items-center">
                      <Field
                        v-model="formData.accountNumber"
                        name="accountNumber"
                        style="border: 1px solid #dcdcdc"
                      >
                        <el-input
                          v-model="formData.accountNumber"
                          name="accountNumber"
                        />
                      </Field>
                      <i
                        v-if="availableAccount"
                        class="fa-regular fa-xl fa-circle-check ms-3"
                        style="color: #14a44d"
                      ></i>
                      <i
                        v-else
                        class="fa-regular fa-xl fa-circle-check ms-3"
                        style="color: lightgray"
                      ></i>
                    </div>

                    <ErrorMessage
                      as="div"
                      class="fv-plugins-message-container invalid-feedback"
                      name="accountNumber"
                    />
                  </div>
                </div>
              </div>

              <div
                class="d-flex flex-column align-items-center justify-content-center mt-4"
              >
                <label class="checkboxContainer fs-7">
                  {{ $t("tip.byClickingYouAgree") }}
                  <a
                    :href="'https://bvi.midasmkts.com/terms-and-conditions'"
                    target="_blank"
                    >{{ $t("title.bcrProTermandCondition") }}</a
                  >
                  <input type="checkbox" v-model="checkedAgree" />
                  <span class="checkmark"></span>
                </label>
                <!-- <div v-if="availableAccount">
                  {{ $t("title.referralFriend") }}: {{ friendName }}
                </div> -->
              </div>

              <div class="d-flex justify-content-center mt-7">
                <button
                  class="ms-5 btn btn-sm bg-hover-light"
                  style="background: #f5f7fa"
                  @click="checkAvailability"
                >
                  <span v-if="isChecking">
                    {{ $t("action.waiting") }}
                    <span
                      class="spinner-border h-15px w-15px align-middle text-gray-400"
                    ></span>
                  </span>
                  <span v-else>{{ $t("action.checkAvailability") }}</span>
                </button>
              </div>
              <!-- ===================================================================================== -->
            </form>

            <table
              v-else
              class="table align-middle table-row-bordered"
              id="kt_permissions_table"
            >
              <tbody v-if="isLoading">
                <LoadingRing />
              </tbody>
            </table>
          </div>

          <!-- ============================================================================== -->
          <!-------------------------------------------------------------------------- Footer -->
          <!-- ============================================================================== -->
          <div class="modal-footer">
            <div class="d-flex flex-stack">
              <div>
                <button
                  class="btn btn-primary text-uppercase"
                  @click="submitCreateAccount()"
                  :disabled="
                    isSubmitting ||
                    isLoading ||
                    !availableAccount ||
                    !checkedAgree
                  "
                >
                  <span v-if="isLoading || isSubmitting">
                    {{ $t("action.waiting") }}
                    <span
                      class="spinner-border h-15px w-15px align-middle text-gray-400"
                    ></span>
                  </span>
                  <span v-else>{{ $t("action.submit") }} </span>
                </button>
              </div>
            </div>
          </div>
          <!-- ================================================================================== -->
          <!-- ================================================================================== -->
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import * as Yup from "yup";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import AccountService from "../../services/AccountService";

import { useI18n } from "vue-i18n";
import { useStore } from "@/store";
import { SiteTypes } from "@/core/types/SiteTypes";
import { ref, computed, nextTick, watch, onUnmounted, provide } from "vue";
import { isMobile } from "@/core/config/WindowConfig";
import { showModal, hideModal, removeModalBackdrop } from "@/core/helpers/dom";
import { Field, ErrorMessage, useForm } from "vee-validate";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";

import {
  CurrencyTypes,
  ConfigCurrencySelections,
} from "@/core/types/CurrencyTypes";
import { ConfigLeverageSelections } from "@/core/types/LeverageTypes";
import { ConfigRealServiceSelections } from "@/core/types/ServiceTypes";

import {
  AccountTypes,
  ConfigAccountTypeSelections,
} from "@/core/types/AccountInfos";

const { t } = useI18n();
const formData = ref<any>({});
const checkedAgree = ref(false);
const isLoading = ref(false);
const isValidForm = ref(false);
const isSubmitting = ref(false);
const isChecking = ref(false);
const availableAccount = ref(false);
const friendName = ref("");
const store = useStore();
const siteId = computed(() => store.state.AuthModule.config.siteId);
const user = computed(() => store.state.AuthModule.user);
const tenancy = store.state.AuthModule.user.tenancy;
const wholesaleFriendRef = ref<HTMLElement | null>(null);
const createTradeAccountSchema = Yup.object().shape({
  email: Yup.string().required(t("error.fieldIsRequired")),
  accountNumber: Yup.string().required(t("error.fieldIsRequired")),
});

const { handleSubmit, resetForm } = useForm({
  validationSchema: createTradeAccountSchema,
});

// ======================================================================================
onUnmounted(() => {
  removeModalBackdrop();
});

const show = async () => {
  showModal(wholesaleFriendRef.value);

  isLoading.value = true;
  isSubmitting.value = false;
  formData.value = {};

  resetForm({ values: {} });

  isLoading.value = false;
};

// Functions ============================================================================
// ======================================================================================
const checkAvailability = handleSubmit(async () => {
  availableAccount.value = false;
  isChecking.value = true;
  friendName.value = "";

  try {
    const res = await AccountService.checkAccountExist({
      email: formData.value.email,
      accountNumber: formData.value.accountNumber,
    });

    availableAccount.value = true;
    friendName.value = res;
  } catch (error) {
    friendName.value = "";
    MsgPrompt.error(error);
  }

  isChecking.value = false;
});

const submitCreateAccount = handleSubmit(async () => {
  isSubmitting.value = true;

  try {
    await AccountService.submitWholesaleReferralRequest({
      userUid: store.state.AuthModule.user.uid,
      email: formData.value.email,
      accountNumber: formData.value.accountNumber,
    });
    MsgPrompt.success(t("tip.submitSuccess")).then(() => {
      hideModal(wholesaleFriendRef.value);
    });
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isSubmitting.value = false;
  }
});

defineExpose({
  show,
});
</script>

<style scoped lang="scss">
.info-part {
  @media (max-width: 768px) {
    label {
      font-size: 16px;
    }
  }
}

.form-style {
  width: 100%;
  height: 300px;
  overflow-y: auto;
  border-left: 1px solid #e4e6ef;
  padding: 30px 30px;
  @media (max-width: 768px) {
    padding: 30px 20px;
  }
}
.fields-title {
  color: #0053ad;
  background-color: #f5f7fa;
  border-right: 1px solid #e4e6ef;
}
.stepper-icon-round {
  border-radius: 100% !important;
}

.createAccountTitle {
  color: #717171;
}

.alpha-tag {
  background: rgba(123, 97, 255, 0.1);
  color: #7b61ff;
  border-radius: 8px;
  padding: 0 8px;
}

.standard-tag {
  background-color: rgba(255, 205, 147, 0.3);
  color: #ff8a00;
  border-radius: 8px;
  padding: 0 8px;
}

.statusBadgeCompleted {
  background-color: rgba(97, 200, 166, 0.3);
  color: #009262;
  border-radius: 5px;
  padding: 5px 10px;
  font-weight: 700;
}

.account-review {
  width: 70%;
  border-radius: 30px;
  border: 1px solid #e4e6ef;
  overflow: hidden;
  margin: auto;

  @media (max-width: 768px) {
    width: 100%;
  }
}
</style>
