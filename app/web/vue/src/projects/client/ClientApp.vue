<template>
  <el-config-provider :locale="elementLocale">
    <router-view />
    <UserFileDocModal ref="userFileDocModalRef" />

    <ConfirmBox ref="confirmBoxRef" />
    <RejectReasonBox ref="rejectReasonBoxRef" />
    <AddCommentModal ref="addCommentModalRef" />
  </el-config-provider>
</template>

<script setup lang="ts">
import { computed, nextTick, onMounted, onUnmounted, provide, ref } from "vue";
import { useStore } from "@/store";
import { Mutations } from "@/store/enums/StoreEnums";
import { initializeComponents } from "@/core/plugins/plugins";
import { updateWidth } from "@/core/config/WindowConfig";
import zhCn from "element-plus/dist/locale/zh-cn.mjs";
import en from "element-plus/dist/locale/en.mjs";
import zhTw from "element-plus/dist/locale/zh-tw.mjs";
import viVN from "element-plus/dist/locale/vi.mjs";
import thTH from "element-plus/dist/locale/th.mjs";
import jpJP from "element-plus/dist/locale/ja.mjs";
import mnMN from "element-plus/dist/locale/mn.mjs";
import idID from "element-plus/dist/locale/id.mjs";
import msMY from "@vee-validate/i18n/dist/locale/ms_MY.json";
import UserFileDocModal from "@/components/ClientUserFileDocModal.vue"; // Todo: change pdfjs lib
import ClientGlobalInjectionKeys from "@/core/types/ClientGlobalInjectionKeys";
import ImageViewer from "./components/ImageViewer.vue";
import RejectReasonBox from "@/components/RejectReasonBox.vue";
import ConfirmBox from "@/components/ConfirmBox.vue";
import AddCommentModal from "@/components/modal/AddCommentModal.vue";
import { getLanguage } from "@/core/types/LanguageTypes";
const store = useStore();
const token = ref<any>("");
const langMap = {
  ["en-us"]: en,
  ["zh-cn"]: zhCn,
  ["zh-tw"]: zhTw,
  ["vi-vn"]: viVN,
  ["th-th"]: thTH,
  ["jp-jp"]: jpJP,
  // ["mn-mn"]: mnMN,
  ["id-id"]: idID,
  // ["ms-my"]: msMY,
};
// const language = computed(
//   () => store?.state?.authModule?.user?.language ?? "en-us"
// );
const elementLocale = computed(() => langMap[getLanguage.value] ?? en);
const userFileDocModalRef = ref<InstanceType<typeof UserFileDocModal>>();
const confirmBoxRef = ref<InstanceType<typeof ConfirmBox>>();
const rejectReasonBoxRef = ref<InstanceType<typeof RejectReasonBox>>();
const addCommentModalRef = ref<InstanceType<typeof AddCommentModal>>();
onMounted(async () => {
  provide(
    ClientGlobalInjectionKeys.OPEN_FILE_MODAL,
    userFileDocModalRef.value?.show
  );
  provide(
    ClientGlobalInjectionKeys.OPEN_CONFIRM_MODAL,
    confirmBoxRef.value?.show
  );
  provide(
    ClientGlobalInjectionKeys.OPEN_REJECT_REASON_MODAL,
    rejectReasonBoxRef.value?.show
  );
  provide(
    ClientGlobalInjectionKeys.OPEN_ADD_COMMENT_MODAL,
    addCommentModalRef.value?.show
  );
  provide(ClientGlobalInjectionKeys.IMG_SHOW_REF, userFileDocModalRef);
  /**
   * Overrides the layout config using saved data from localStorage
   * remove this to use static config (@/core/config/DefaultLayoutConfig.ts)
   */
  store.commit(Mutations.OVERRIDE_LAYOUT_CONFIG);
  window.addEventListener("resize", updateWidth);
  /** tbd
   *  Sets a mode from configuration
   *
   */
  nextTick(() => {
    initializeComponents();
  });
});

onUnmounted(() => {
  window.removeEventListener("resize", updateWidth);
});
</script>

<style lang="scss">
@import "~bootstrap-icons/font/bootstrap-icons.css";
@import "~apexcharts/dist/apexcharts.css";
@import "~quill/dist/quill.snow.css";
@import "~animate.css";
@import "~sweetalert2/dist/sweetalert2.css";
@import "~nouislider/distribute/nouislider.css";
@import "~@fortawesome/fontawesome-free/css/all.min.css";
@import "~socicon/css/socicon.css";
@import "~line-awesome/dist/line-awesome/css/line-awesome.css";
@import "~dropzone/dist/dropzone.css";
@import "~@vueform/multiselect/themes/default.css";
@import "~prism-themes/themes/prism-shades-of-purple.css";
@import "~element-plus/dist/index.css";

// Main demo style scss
@import "@/assets/sass/plugins";
@import "@/assets/sass/style";

//RTL version styles
//@import "assets/css/style.rtl.css";

#app {
  display: contents;
}
</style>
