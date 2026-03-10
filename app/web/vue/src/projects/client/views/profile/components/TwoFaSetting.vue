<template>
  <div class="card shadow-sm">
    <!--begin::Content-->
    <div class="card-header">
      <div class="card-title">{{ $t("fields.twoFactorAuthentication") }}</div>
    </div>
    <div class="card-body" style="padding: 0px 30px 20px 30px">
      <div>
        <div v-if="showCodeInput == false">
          <el-radio-group v-model="choice" :disabled="isSubmitting">
            <el-radio :label="true" size="large" border>{{
              $t("fields.enable")
            }}</el-radio>
            <el-radio :label="false" size="large" border>{{
              $t("fields.disable")
            }}</el-radio>
          </el-radio-group>
        </div>
        <div v-else>
          <div class="d-flex align-items-center gap-4">
            <el-input
              v-model="code"
              :placeholder="$t('fields.enterYourCode')"
              size="large"
              class="w-150px"
              :disabled="isSubmitting"
            ></el-input>
            <el-button
              type="primary"
              :loading="isSubmitting"
              @click="getCode"
              size="large"
            >
              {{ $t("fields.verify") }}
            </el-button>
          </div>
          <p class="text-danger mt-3">
            {{ $t("fields.pleaseEnterTwoFactorCode") }}
          </p>
        </div>
      </div>
      <div class="d-flex gap-5 flex-end mt-5">
        <button
          class="btn btn-sm btn-light btn-bordered btn-radius"
          type="reset"
          @click="showCodeInput = false"
        >
          {{ $t("action.reset") }}
        </button>
        <button
          class="btn btn-sm btn-primary btn-bordered btn-radius"
          @click="getCode"
          :disabled="oldValue == choice"
        >
          <span
            class="indicator-label"
            v-if="isSubmitting == false"
            :disabled="showCodeInput"
          >
            {{ $t("action.saveChanges") }}
          </span>

          <span class="indicator-progress" v-else :disabled="isSubmitting">
            {{ $t("action.pleaseWait") }}
            <span
              class="spinner-border spinner-border-sm align-middle ms-2"
            ></span>
          </span>
        </button>
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
import { ref, onMounted } from "vue";
import ClientGlobalService from "@/projects/client/services/ClientGlobalService";
import { useStore } from "@/store";
import { PublicSetting } from "@/core/types/ConfigTypes";
import notification from "@/core/plugins/notification";
import { Actions } from "@/store/enums/StoreEnums";
const store = useStore();
const projectConfig: PublicSetting = store.state.AuthModule.config;
const isSubmitting = ref(false);
const choice = ref<any>(null);
const showCodeInput = ref(false);
const code = ref<any>(null);
const oldValue = ref<any>("");

const getCode = async () => {
  isSubmitting.value = true;
  try {
    const res =
      choice.value == true
        ? await ClientGlobalService.twoFaEnableCode({ code: code.value })
        : await ClientGlobalService.twoFaDisableCode({ code: code.value });

    if (res.isSuccess == true) {
      savetoLocalStorage();
      notification.success();
      showCodeInput.value = false;
      oldValue.value = choice.value;
      code.value = null;
    } else {
      showCodeInput.value = true;
    }
  } catch (error) {
    console.error(error);
    notification.danger();
  } finally {
    isSubmitting.value = false;
  }
};

const savetoLocalStorage = () => {
  let config = JSON.parse(localStorage.getItem("config"));

  if (config) {
    config.twoFactorAuth = choice.value;
    store.dispatch(Actions.UPDATE_CONFIG, config);
  } else {
    console.error("Config not found in localStorage");
  }
};

onMounted(() => {
  if (projectConfig?.twoFactorAuth != undefined) {
    choice.value = projectConfig?.twoFactorAuth;
    oldValue.value = projectConfig?.twoFactorAuth;
  }
});
</script>
