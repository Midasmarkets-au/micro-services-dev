<template>
  <el-dialog
    v-model="dialogRef"
    width="761"
    align-center
    class="rounded-3 overflow-auto"
    :style="containerStyle"
    :show-close="false"
    :before-close="beforeClose"
  >
    <template #header>
      <div class="text-center size-lg" style="color: #800020">
        <span>{{ $t("title.twoFactorAuthenticationSetupReminder") }}</span>
      </div>
    </template>

    <div class="overflow-scroll mx-4">
      <div class="mb-5 fs-4">
        <p>{{ $t("fields.twoFaDesc") }}</p>
        <li>
          <span>{{ $t("fields.enableTwoFa") }}</span>
        </li>
        <li>
          <span>{{ $t("fields.disableTwoFa") }}</span>
        </li>
      </div>
      <div class="mt-5">
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
          </div>
          <p class="text-danger mt-3">
            {{ $t("fields.pleaseEnterTwoFactorCode") }}
          </p>
        </div>
      </div>
    </div>
    <template #footer>
      <div class="d-flex justify-content-center align-items-center gap-5">
        <el-button
          class="btn btn-primary btn-radius btn-sm d-flex align-items-center"
          @click="showCodeInput = false"
          :disabled="isSubmitting || !showCodeInput"
          type="info"
          >{{ $t("fields.previous") }}</el-button
        >
        <el-button
          class="btn btn-primary btn-radius btn-sm d-flex align-items-center"
          v-if="showCodeInput == false"
          type="warning"
          :loading="isSubmitting"
          @click="getCode"
        >
          {{ $t("action.submit") }}
        </el-button>
        <el-button
          v-if="showCodeInput == true"
          type="primary"
          class="d-flex align-items-center"
          :loading="isSubmitting"
          :disabled="code.trim() == ''"
          @click="getCode"
        >
          {{ $t("fields.verify") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref, computed } from "vue";
import ClientGlobalService from "../../services/ClientGlobalService";
import { useStore } from "@/store";
import { PublicSetting } from "@/core/types/ConfigTypes";
import notification from "@/core/plugins/notification";
import { isMobile } from "@/core/config/WindowConfig";
import { Actions } from "@/store/enums/StoreEnums";

const store = useStore();
const projectConfig: PublicSetting = store.state.AuthModule.config;
const isSubmitting = ref(false);
const dialogRef = ref(false);
const choice = ref(true);
const showCodeInput = ref(false);
const code = ref<any>(null);

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
      beforeClose();
    } else {
      code.value = "";
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

const emit = defineEmits(["close"]);

const beforeClose = () => {
  dialogRef.value = false;
  emit("close");
};

// const noShowCheck = () => {
//   savetoLocalStorage();
//   dialogRef.value = true;
//   return true;
// };

const noShowCheck = () => {
  if (projectConfig?.twoFactorAuth === null) {
    dialogRef.value = true;
    return true;
  }

  dialogRef.value = false;
  return false;
};
const containerStyle = computed(() => ({
  maxHeight: isMobile.value ? "90vh" : "60vh",
  //borderTop: "8px solid #f4b83d !important",
}));
defineExpose({
  dialogRef,
  noShowCheck,
});

// onMounted(async () => {
//   noShowCheck();
// });
</script>
<style scoped>
.el-dialog:deep .el-dialog__header {
  border-bottom: 1px solid #ecede4 !important;
  padding-bottom: 0px !important;
}
.border-top {
  border-top: 1px solid #d4d4d2 !important;
}
.border-bottom {
  border-bottom: 1px solid #d4d4d2 !important;
}
.size-llg {
  font-size: 28px;
}
.size-lg {
  font-size: 20px;
}
</style>
