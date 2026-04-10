<template>
  <KTLoader v-if="loaderEnabled" :logo="loaderLogo" />

  <!-- begin:: Body -->
  <!---   style="background-image: url('/images/dashboard_bg.png')"-->
  <div
    class="page d-flex flex-row flex-column-fluid min-h-screen relative bg-fixed bg-no-repeat"
  >
    <div
      class="absolute inset-0 overflow-hidden bg-[linear-gradient(to_bottom,rgba(255,255,255,0)_0px,rgba(255,255,255,0.7)_70px,rgba(193,214,250,0.2)_160px,rgba(193,214,250,0.8)_100%)]"
    >
      <UiRipple
        circle-class="border-white shadow-[inset_0_0_10px_10px_rgba(255,255,255,0.7)] rounded-full w-full"
      />
    </div>
    <div
      id="kt_wrapper"
      class="wrapper relative d-flex flex-column flex-row-fluid"
    >
      <KTHeader />
      <!-- begin:: Content Head -->
      <KTToolbar ref="toolBarAction" />
      <!-- end:: Content Head -->
      <!-- begin:: Content -->
      <div
        id="kt_content"
        class="d-flex flex-column-fluid align-items-start"
        :class="{
          'container-fluid': contentWidthFluid,
          'container-xxl': !contentWidthFluid,
        }"
      >
        <!-- begin:: Aside Left -->
        <KTAside
          v-if="asideEnabled"
          :lightLogo="themeLightLogo"
          :darkLogo="themeDarkLogo"
        />
        <!-- end:: Aside Left -->
        <!-- begin:: Content Body -->
        <div class="content flex-row-fluid webtraderContent pt-3">
          <router-view />
          <!-- <div id="zendesk" class="live-chat" v-if="canLiveChat">
            <a href="javascript:void(0)">
              <inline-svg src="/images/icons/other/livechat_new.svg" />
            </a>
          </div> -->
        </div>
        <!-- end:: Content Body -->
      </div>
      <!-- end:: Content -->
      <KTFooter />
    </div>
  </div>
  <!-- end:: Body -->
</template>

<script lang="ts" setup>
import { onMounted, watch, nextTick, ref } from "vue";
import { useStore } from "@/store";
import { useRoute, useRouter } from "vue-router";
import KTAside from "@/layouts/client-layout/aside/Aside.vue";
import KTHeader from "@/layouts/client-layout/header/Header.vue";
import KTFooter from "@/layouts/client-layout/footer/Footer.vue";
import HtmlClass from "@/core/services/LayoutService";
import KTToolbar from "@/layouts/client-layout/toolbar/Toolbar.vue";
import KTLoader from "@/components/Loader.vue";
import { Actions } from "@/store/enums/StoreEnums";
import { MenuComponent } from "@/assets/ts/components";
import { reinitializeComponents } from "@/core/plugins/plugins";
import { removeModalBackdrop } from "@/core/helpers/dom";
import UiRipple from "../../projects/client/components/ripple/UiRipple.vue";
import {
  loaderEnabled,
  contentWidthFluid,
  loaderLogo,
  asideEnabled,
  themeLightLogo,
  themeDarkLogo,
} from "@/core/helpers/config";
// import { canLiveChat } from "@/core/types/TenantTypes";

const store = useStore();
const route = useRoute();
const router = useRouter();
const toolBarAction = ref<InstanceType<typeof KTToolbar>>();
// const script = document.createElement("script");
// script.src = "/js/zenChat.js?ver=1.0.1";

store.dispatch(Actions.ADD_BODY_CLASSNAME, "page-loading");

onMounted(() => {
  //check if current user is authenticated
  if (!store.getters.isUserAuthenticated) {
    router.push({
      name: "sign-in",
      query: { redirect: router.currentRoute.value.path },
    });
  }

  nextTick(() => {
    reinitializeComponents();
  });

  // initialize html element classes
  HtmlClass.init();

  // Simulate the delay page loading
  setTimeout(() => {
    // Remove page loader after some time
    store.dispatch(Actions.REMOVE_BODY_CLASSNAME, "page-loading");
    // document.head.appendChild(script);
  }, 500);
});

watch(
  () => route.path,
  () => {
    MenuComponent.hideDropdowns(undefined);

    // check if current user is authenticated
    if (!store.getters.isUserAuthenticated) {
      router.push({
        name: "sign-in",
        // query: { redirect: router.currentRoute.value.name },
      });
    }

    removeModalBackdrop();
    nextTick(() => {
      reinitializeComponents();
    });
  }
);
</script>
<style>
.live-chat {
  position: fixed;
  bottom: 30px;
  /* width: 80px; */
  right: calc(50% - (36%));
  padding: 5px 12px;
  font-size: 14px;
  width: 44px;
  height: 44px;
  background: #002768;
  box-shadow: 0px 2px 4px 0px rgba(21, 63, 133, 0.32);
  border-radius: 28px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  @media (max-width: 768px) {
    bottom: 10px;
    right: 10px;
  }
}
</style>
