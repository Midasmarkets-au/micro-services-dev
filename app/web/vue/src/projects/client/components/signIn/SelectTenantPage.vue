<template>
  <div>
    <div
      class="d-flex justify-content-center flex-wrap"
      :class="isMobile ? 'gap-4' : 'gap-12'"
      v-loading="isSubmitting"
    >
      <el-card
        class="w-150px cursor"
        :class="isMobile ? 'my-3' : ''"
        :body-style="{ padding: '40px' }"
        v-for="(item, index) in tenantsOptions"
        :key="index"
        @click="loginWithTenantId(item)"
      >
        <img :src="getFlagSrc(item)" alt="" class="img-rounded" />
        <div class="text-center pt-3 mt-3">
          <span>{{ $t("title.tenants." + item) }}</span>
        </div>
      </el-card>
      <el-card
        class="w-150px cursor"
        @click="showPage = 'LoginPage'"
        :body-style="{ padding: '40px' }"
      >
        <img src="/images/back-1.svg" />
        <div class="text-center pt-3 mt-3">
          <span class="">{{ $t("action.back") }}</span>
        </div>
      </el-card>
    </div>

    <div class="mb-6 mt-10" v-if="twoFaRequired">
      <div class="d-flex justify-content-center text-center">
        <div>
          <div class="d-flex justify-content-center align-items-center gap-4">
            <label class="label required mb-1">{{
              $t("fields.twoFactorAuthentication")
            }}</label>

            <Field
              tabindex="2"
              v-model="twoFaCode"
              class="form-control form-control-lg form-control-solid w-200px"
              type="text"
              name="twoFaCode"
              autocomplete="off"
            >
              <el-input
                v-model="twoFaCode"
                class="w-200px"
                name="twoFaCode"
                :placeholder="$t('tip.pleaseInput')"
                size="large"
                tabindex="2"
                type="text"
              />
            </Field>
          </div>
          <div class="fv-plugins-message-container mt-2">
            <div class="fv-help-block">
              {{ $t("fields.pleaseEnterTwoFactorCode") }}
            </div>
          </div>
        </div>
      </div>

      <div>
        <div class="d-flex justify-content-center mt-6">
          <LoadingButton
            :is-loading="isSubmitting"
            :save-title="$t('action.login')"
            class="btn btn-lg w-200px loginBtn"
            @click.prevent="onSubmitLogin"
          />
        </div>
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
import { ref, inject } from "vue";
import { isMobile } from "@/core/config/WindowConfig";
import LoadingButton from "@/components/buttons/LoadingButton.vue";
import { Field } from "vee-validate";
const showPage = inject<any>("showPage");
const isSubmitting = inject<any>("isSubmitting");
const tenantsOptions = inject<any>("tenantsOptions");
const selectedTenant = inject<any>("selectedTenant");
const twoFaRequired = inject<any>("twoFaRequired");
const twoFaCode = inject<any>("twoFaCode");

const onSubmitLogin = inject<any>("onSubmitLogin");

const flagList = ref({
  1: "/images/flags/au.svg",
  10000: "/images/flags/earth.svg",
  10002: "/images/flags/mn.svg",
  10004: "/images/flags/vn.svg",
  10005: "/images/flags/jp.svg",
});

const getFlagSrc = (id) => {
  return flagList.value[id] || "/images/flags/default.svg"; // Provide a default flag if id not found
};

const loginWithTenantId = async (tenantId: number) => {
  selectedTenant.value = tenantId;
  await onSubmitLogin();
};
</script>
<style scoped>
.cursor {
  cursor: pointer;
}
.img-rounded {
  border-radius: 50%;
}
</style>
