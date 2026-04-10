<template>
  <!--begin::Header-->
  <div
    id="kt_app_header"
    v-if="headerDisplay"
    class="app-header"
    :style="'background-color:' + siteColor"
    :class="{
      'bg-none': env == 'Development',
      'bg-success': env == 'staging',
    }"
  >
    <!--begin::Header container-->
    <div
      class="app-container d-flex align-items-stretch justify-content-between"
      :class="{
        'container-fluid': headerWidthFluid,
        'container-xxl': !headerWidthFluid,
      }"
    >
      <div
        v-if="layout === 'light-header' || layout === 'dark-header'"
        class="d-flex align-items-center flex-grow-1 flex-lg-grow-0 me-lg-15"
      >
        <router-link to="/">
          <img
            v-if="themeMode === 'light' && layout === 'light-header'"
            alt="Logo"
            src="/images/logos/logo@2x.png"
            class="h-20px h-lg-30px app-sidebar-logo-default"
          />
          <img
            v-if="
              layout === 'dark-header' ||
              (themeMode === 'dark' && layout === 'light-header')
            "
            alt="Logo"
            src="/images/logos/logo@2x.png"
            class="h-20px h-lg-30px app-sidebar-logo-default"
          />
        </router-link>
      </div>
      <!--begin::sidebar mobile toggle-->
      <div
        class="d-flex align-items-center d-lg-none ms-n2 me-2"
        title="Show sidebar menu"
      >
        <div
          class="btn btn-icon btn-active-color-primary w-35px h-35px"
          id="kt_app_sidebar_mobile_toggle"
        >
          <span class="svg-icon svg-icon-1">
            <inline-svg src="/images/icons/abstract/abs015.svg" />
          </span>
        </div>
      </div>
      <!--end::sidebar mobile toggle-->
      <!--begin::Mobile logo-->
      <div class="d-flex align-items-center flex-grow-1 flex-lg-grow-0">
        <router-link to="/" class="d-lg-none">
          <img
            alt="Logo"
            src="/images/logos/default-small.svg"
            class="h-30px"
          />
        </router-link>
      </div>
      <!--end::Mobile logo-->
      <!--begin::Header wrapper-->
      <div
        class="d-flex align-items-center justify-content-between flex-lg-grow-1"
        id="kt_app_header_wrapper"
      >
        <div class="p-3 fs-2" style="width: 225px; text-transform: uppercase">
          Backend
        </div>
        <div class="p-3 fs-2">
          <TimerDisplay v-if="!roles.includes('DemoAuAdmin')" />
        </div>
        <div class="p-3">
          <SessionTimer />
        </div>
        <div>
          <TenantSwitch />
        </div>
        <KTHeaderMenu />
        <KTHeaderNavbar />
      </div>
      <!--end::Header wrapper-->
    </div>
    <!--end::Header container-->
  </div>
  <!--end::Header-->
</template>

<script>
import { defineComponent } from "vue";
import KTHeaderMenu from "@/layouts/backend-layout/header/menu/Menu.vue";
import KTHeaderNavbar from "@/layouts/backend-layout/header/Navbar.vue";
import {
  layout,
  headerWidthFluid,
  themeMode,
  headerDisplay,
} from "../config/config";
import TimerDisplay from "@/components/TimerDisplay.vue";
import TenantSwitch from "@/components/TenantSwitch.vue";
import SessionTimer from "@/components/SessionTimer.vue";
import { useStore } from "@/store";
export default defineComponent({
  name: "layout-header",
  components: {
    KTHeaderMenu,
    KTHeaderNavbar,
    TimerDisplay,
    TenantSwitch,
    SessionTimer,
  },
  setup() {
    const env = process.env.VUE_APP_ENV;
    const store = useStore();
    const user = store.state.AuthModule.user;
    const roles = store.state.AuthModule.user.roles;
    var siteName = user.tenancy;
    var siteColors = {
      au: "#7a9e7a",
      bvi: "#fa6b6c",
      sea: "#349beb",
      mn: "#b497b4",
      jp: "#f6c23e",
    };
    const siteColor = siteColors[user.tenancy];
    window.document.title = " Backend" || "MDM";
    return {
      layout,
      headerWidthFluid,
      headerDisplay,
      themeMode,
      env,
      siteName,
      siteColor,
      roles,
    };
  },
});
</script>
