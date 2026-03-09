<template>
  <!--begin::Navbar-->
  <div class="app-navbar flex-shrink-0">
    <!--begin::Websockets Notity-->
    <BPWSNotify />
    <!--end::Websockets Notity-->
    <!--begin::User menu-->
    <div class="app-navbar-item ms-1 ms-lg-3" id="kt_header_user_menu_toggle">
      <!--begin::Menu wrapper-->
      <div
        class="cursor-pointer symbol symbol-35px symbol-md-40px"
        data-kt-menu-trigger="click"
        data-kt-menu-attach="parent"
        data-kt-menu-placement="bottom-end"
      >
        <AuthImage :imageGuid="user.avatar" :alt="user.name" />
      </div>
      <BPUserMenu />
      <!--end::Menu wrapper-->
    </div>
    <!--end::User menu-->
    <!--begin::Header menu toggle-->
    <div
      class="app-navbar-item d-lg-none ms-2 me-n3"
      title="Show header menu"
      v-if="headerMenuDisplay"
    >
      <div
        class="btn btn-icon btn-active-color-primary w-35px h-35px"
        id="kt_app_header_menu_toggle"
      >
        <span class="svg-icon svg-icon-1">
          <inline-svg src="/images/icons/text/txt001.svg" />
        </span>
      </div>
    </div>
    <!--end::Header menu toggle-->
  </div>
  <!--end::Navbar-->
</template>

<script lang="ts">
import { defineComponent } from "vue";
import BPUserMenu from "@/layouts/backend-layout/menus/UserAccountMenu.vue";
import BPWSNotify from "@/layouts/backend-layout/notify/WSNotify.vue";
import AuthImage from "@/components/AuthImage.vue";
import { useStore } from "@/store";
import { headerMenuDisplay } from "../config/config";

export default defineComponent({
  name: "header-navbar",
  components: {
    BPUserMenu,
    BPWSNotify,
    AuthImage,
  },
  setup() {
    const store = useStore();
    const user = store.state.AuthModule.user;
    return {
      user,
      headerMenuDisplay,
    };
  },
});
</script>
