<template>
  <!--begin::Header-->
  <div
    id="kt_header"
    class="header align-items-stretch"
    :data-kt-sticky="isHeaderSticky"
    data-kt-sticky-name="header"
    data-kt-sticky-offset="{default: '200px', lg: '300px'}"
  >
    <!--begin::Container-->
    <div
      :class="{
        'container-fluid': headerWidthFluid,
        'container-xxl': !headerWidthFluid,
      }"
      class="d-flex align-items-center"
    >
      <!--begin::Aside toggle-->
      <div
        class="d-flex topbar align-items-center d-lg-none ms-n2 me-3"
        title="Show aside menu"
      >
        <div
          class="btn btn-icon btn-primary btn-custom w-30px h-30px w-md-40px h-md-40px"
          id="kt_header_menu_mobile_toggle"
        >
          <span class="svg-icon svg-icon-1">
            <inline-svg src="/images/icons/abstract/abs015.svg" />
          </span>
        </div>
      </div>
      <div
        class="header-logo me-md-10 flex-grow-1 flex-lg-grow-0 text-lg-start text-right"
      >
        <router-link to="/">
          <img
            :src="getTenantLogo['src']"
            class="logo-default"
            :style="getTenantLogo['style']"
          />
        </router-link>
      </div>
      <!--end::手机部份-在web端不显示 logo-->

      <!--begin::Wrapper-->
      <div
        class="d-flex align-items-stretch justify-content-between flex-lg-grow-1"
      >
        <!--begin::菜单-->
        <div
          class="d-flex align-items-stretch flex-lg-grow-1"
          id="kt_header_nav"
        >
          <KTMenu></KTMenu>
        </div>
        <!--end::Navbar-->

        <!--begin::头像-->
        <div class="d-flex align-items-stretch flex-shrink-0">
          <KTTopbar></KTTopbar>
        </div>
        <!--end::Topbar-->
      </div>
      <!--end::Wrapper-->
    </div>
    <!--end::Container-->
  </div>
  <!--end::Header-->
</template>

<script lang="ts">
import { defineComponent, computed } from "vue";
import KTTopbar from "@/layouts/client-layout/header/Topbar.vue";
import KTMenu from "@/layouts/client-layout/header/Menu.vue";

import {
  headerWidthFluid,
  headerFixed,
  headerFixedOnMobile,
  headerLeft,
  asideDisplay,
} from "@/core/helpers/config";
import { getTenantLogo } from "@/core/types/TenantTypes";

export default defineComponent({
  name: "KTHeader",
  components: {
    KTTopbar,
    KTMenu,
  },
  setup() {
    const isHeaderSticky = computed(() => {
      if (window.innerWidth > 768) {
        return headerFixed.value;
      } else {
        return headerFixedOnMobile.value;
      }
    });

    return {
      headerWidthFluid,
      headerLeft,
      asideDisplay,
      isHeaderSticky,
      getTenantLogo,
    };
  },
});
</script>
