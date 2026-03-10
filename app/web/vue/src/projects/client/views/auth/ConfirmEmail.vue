<template>
  <div
    class="authentication-login-group auth-page-wrapper relative w-full h-screen overflow-hidden bg-[radial-gradient(circle,rgba(22,132,252,0.4),rgba(186,184,253,0))]"
  >
    <UiRipple
      circle-class="border-white bg-[#B5CFFB]/25 shadow-[inset_0_0_20px_10px_rgba(255,255,255,0.6)] rounded-full"
    />
    <div class="form-container relative">
      <!-- <div>
        <img class="mobile-logo" src="/images/auth/login-logo.png" alt="" />
      </div> -->

      <!-- <div class="text-dark auth-language-dropdown">
        <LanguageDropdown />
      </div> -->
      <div class="auth-box">
        <div class="auth-box-header">
          <span class="text-3xl font-bold">{{
            $t("title.emailConfirmation")
          }}</span>
        </div>
        <div class="auth-form">
          <Form class="login-form" @submit="onConfirmEmail">
            <!-- <div class="title">{{ $t("title.emailConfirmation") }}</div> -->

            <div class="mb-6">
              <label class="label required">{{ $t("fields.email") }}</label>

              <Field
                tabindex="1"
                class="mt-1 mb-4 form-control form-control-lg form-control-solid"
                type="text"
                name="email"
                autocomplete="off"
                :value="props.email"
                disabled
              />
            </div>

            <div>
              <LoadingButton
                tabindex="3"
                type="submit"
                class="btn btn-lg w-100 loginBtn"
                :is-loading="isLoading"
                :save-title="$t('action.confirm')"
              />

              <div class="msg_1 mb-4">
                {{ $t("action.backTo") + " " }}
                <router-link to="/sign-in">{{
                  $t("action.login")
                }}</router-link>
              </div>
            </div>
          </Form>
        </div>
      </div>
      <!--end::form-->
    </div>
    <div class="absolute top-3" :style="{ left: isMobile ? '20px' : '110px' }">
      <img class="h-12 w-12" alt="Logo" :src="getTenantLogo['src']" />
    </div>
    <div
      class="absolute top-10 flex"
      :style="{ right: isMobile ? '20px' : '110px' }"
    >
      <LanguageDropdown />
    </div>
  </div>
</template>

<script lang="ts" setup>
import { useStore } from "@/store";
import { useI18n } from "vue-i18n";

import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import { Field, Form } from "vee-validate";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { Actions } from "@/store/enums/StoreEnums";
import LoadingButton from "@/components/buttons/LoadingButton.vue";
import LanguageDropdown from "../../components/LanguageDropdown.vue";
import ClientGlobalService from "@/projects/client/services/ClientGlobalService";
import { isMobile } from "@/core/config/WindowConfig";
import { getTenantLogo, getTenancy, tenancies } from "@/core/types/TenantTypes";
import UiRipple from "../../components/ripple/UiRipple.vue";

const { t } = useI18n();
const store = useStore();
const isLoading = ref(false);
const router = useRouter();

const props = defineProps({
  code: { type: String, required: true },
  email: { type: String, required: true },
});

onMounted(() => {
  store.dispatch(Actions.REMOVE_BODY_CLASSNAME, "page-loading");
});

//form submit function
const onConfirmEmail = async () => {
  await store.dispatch(Actions.LOGOUT);

  try {
    isLoading.value = true;
    await ClientGlobalService.confirmEmail({
      ...props,
    });

    MsgPrompt.success(t("tip.pleaseLoginToVerify"), t("tip.confirmSuccess"))
      .then(() => store.dispatch(Actions.LOGOUT))
      .then(() => router.push({ name: "sign-in" }));
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};
</script>

<style lang="scss" scoped>
#email-confirm-form {
  position: relative;
  width: 100vw;

  display: flex;
}

// ----------------------------------------- Side
.side {
  display: flex;
  flex-direction: column;
  justify-content: space-between;

  height: 100vh;
  width: 30%;
  max-width: 483px;

  background-image: url("/images/auth/login-side-bg.png");
  background-size: cover;
  background-repeat: no-repeat;

  padding-top: 100px;
  padding-bottom: 100px;

  @media (max-width: 768px) {
    display: none;
  }
}

.side-text-wrap {
  margin: 0 auto;
  width: 280px;
}

.welcome-text {
  color: white;
  font-weight: 400;
  font-size: 16px;
  line-height: 22px;
  font-family: "Lato", sans-serif;
}

.logo {
  width: 280px;
}
@media (max-width: 1200px) {
  .logo {
    width: 200px;
  }
  .side-text-wrap {
    margin-left: 20px;
  }
}
.mobile-logo {
  display: none;

  @media (max-width: 768px) {
    display: block;
    position: absolute;
    width: 120px;
    top: 30px;
    left: 30px;
  }
}
// ----------------------------------------- Form

.form-container {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: center;

  @media (max-width: 768px) {
    background-image: url("/images/auth/login-side-bg.png");
    background-size: cover;
    background-repeat: no-repeat;
    padding-top: 70px;
    color: white;
  }
}

.email-confirm-form {
  width: 472px;
  height: calc(100vh - 70px);
  display: flex;
  flex-direction: column;
  justify-content: center;
  font-family: "Lato", sans-serif;

  @media (max-width: 768px) {
    width: 80%;
  }
}

.title {
  font-family: "Lato", sans-serif;
  font-weight: 600;
  font-size: 32px;
  color: #393d48;
  text-align: center;
  margin-bottom: 32px;

  @media (max-width: 768px) {
    color: white;
    font-size: 24px;
  }
}

.label {
  font-family: "Lato", sans-serif;
  font-weight: 400;
  font-size: 14px;
  color: #666666;
  @media (max-width: 768px) {
    color: white !important;
  }
}

// .loginBtn {
//   order: 1;
//   box-sizing: border-box;
//   color: white;

//   width: 472px;
//   height: 64px;
//   margin-top: 25px;
//   background: #393d48;
//   border: 1px solid #393d48;
//   border-radius: 8px;

//   @media (max-width: 768px) {
//     order: 2;
//     background-color: rgba(57, 61, 72, 0.5);
//     border: 1px solid white !important;
//     font-size: 16px;
//   }
// }

.msg_1 {
  font-family: "Lato", sans-serif;
  font-weight: 400;
  font-size: 12px;
  text-align: center;

  color: #111111;
  margin-top: 24px;

  @media (max-width: 768px) {
    order: 1;
    color: white;
    margin-bottom: 24px;
  }
}
</style>
