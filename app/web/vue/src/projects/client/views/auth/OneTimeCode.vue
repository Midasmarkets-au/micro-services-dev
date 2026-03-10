<template>
  <div
    class="authentication-login-group auth-page-wrapper relative w-full h-screen overflow-hidden bg-[radial-gradient(circle,rgba(22,132,252,0.4),rgba(186,184,253,0))]"
  >
    <UiRipple
      circle-class="border-white bg-[#B5CFFB]/25 shadow-[inset_0_0_20px_10px_rgba(255,255,255,0.6)] rounded-full"
    />
    <div class="form-container relative">
      <component :is="getComponent()" />
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
import { ref, onMounted, provide } from "vue";
import { Actions } from "@/store/enums/StoreEnums";
import EmailForm from "./2FA/EmailForm.vue";
import CodeForm from "./2FA/CodeForm.vue";
import PasswordForm from "./2FA/PasswordForm.vue";
import LanguageDropdown from "../../components/LanguageDropdown.vue";
import { isMobile } from "@/core/config/WindowConfig";
import { getTenantLogo, getTenancy, tenancies } from "@/core/types/TenantTypes";
import UiRipple from "../../components/ripple/UiRipple.vue";
const store = useStore();
const step = ref(0);

const components = {
  EmailForm,
  CodeForm,
  PasswordForm,
};

const data = ref({
  email: "",
  code: "",
});

provide("data", data);
provide("step", step);

const getComponent = () => {
  return components[Object.keys(components)[step.value]];
};
onMounted(() => {
  store.dispatch(Actions.REMOVE_BODY_CLASSNAME, "page-loading");
});
</script>
