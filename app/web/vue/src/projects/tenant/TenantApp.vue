<template>
  <router-view />
  <UserDetails ref="globalUserShowRef" />
  <CommentView ref="globalCommentViewRef" />
  <UserFileDocModal ref="userFileDocModalRef" />
  <ConfirmBox ref="confirmBoxRef" />
  <RejectReasonBox ref="rejectReasonBoxRef" />
  <AddCommentModal ref="addCommentModalRef" />
</template>

<script setup lang="ts">
import { nextTick, onMounted, provide, ref } from "vue";
import { useStore } from "@/store";
import { Mutations, Actions } from "@/store/enums/StoreEnums";
import { themeMode } from "@/layouts/backend-layout/config/config";
import { initializeComponents } from "@/core/plugins/plugins";
import { useRouter } from "vue-router";
import UserDetails from "@/projects/tenant/components/UserDetails.vue";
import CommentView from "@/projects/tenant/components/CommentView.vue";
import UserFileDocModal from "@/components/UserFileDocModal.vue";
import InjectKeys from "@/core/types/TenantGlobalInjectionKeys";
import ConfirmBox from "@/components/ConfirmBox.vue";
import RejectReasonBox from "@/components/RejectReasonBox.vue";
import AddCommentModal from "@/components/modal/AddCommentModal.vue";

const addCommentModalRef = ref<InstanceType<typeof AddCommentModal>>();
const router = useRouter();
const globalUserShowRef = ref<InstanceType<typeof UserDetails>>();
const globalCommentViewRef = ref<InstanceType<typeof CommentView>>();
const userFileDocModalRef = ref<InstanceType<typeof UserFileDocModal>>();
const confirmBoxRef = ref<InstanceType<typeof ConfirmBox>>();
const rejectReasonBoxRef = ref<InstanceType<typeof RejectReasonBox>>();

onMounted(async () => {
  // check if current user is authenticated
  // console.log(globalUserShowRef.value);
  // console.log(userFileDocModalRef.value);
  const store = useStore();
  provide(InjectKeys.OPEN_FILE_MODAL, userFileDocModalRef.value?.show);
  provide(InjectKeys.OPEN_USER_DETAILS, globalUserShowRef.value?.show);
  provide(InjectKeys.OPEN_COMMENT_VIEW, globalCommentViewRef.value?.show);
  provide(InjectKeys.OPEN_CONFIRM_MODAL, confirmBoxRef.value?.show);
  provide(InjectKeys.OPEN_REJECT_REASON_MODAL, rejectReasonBoxRef.value?.show);
  provide(InjectKeys.FILE_SHOW_REF, userFileDocModalRef);
  provide(InjectKeys.OPEN_ADD_COMMENT_MODAL, addCommentModalRef.value?.show);
  // Authentication redirect is handled by the router guard (VERIFY_AUTH).
  // Do NOT check isAuthenticated here — it is always false on page refresh
  // until the async VERIFY_AUTH call completes, causing premature redirects.
  /**
   * Overrides the layout config using saved data from localStorage
   * remove this to use static config (@/core/config/DefaultLayoutConfig.ts)
   */
  store.commit(Mutations.OVERRIDE_LAYOUT_CONFIG);
  /**
   *  Sets a mode from configuration
   */
  if (themeMode.value === undefined) {
    await store.dispatch(Actions.SET_THEME_MODE_ACTION, "light");
  } else {
    await store.dispatch(Actions.SET_THEME_MODE_ACTION, themeMode.value);
  }

  nextTick(() => {
    initializeComponents();
  });
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
@import "@/assets-backend/sass/plugins";
@import "@/assets-backend/sass/style";

//RTL version styles
//@import "assets/css/style.rtl.css";

#app {
  display: contents;
}
</style>
