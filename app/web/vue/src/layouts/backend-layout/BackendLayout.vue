<template>
  <!--begin::App-->
  <div class="d-flex flex-column flex-root app-root" id="kt_app_root">
    <!--begin::Page-->
    <div class="app-page flex-column flex-column-fluid" id="kt_app_page">
      <BPHeader />
      <!--begin::Wrapper-->
      <div class="app-wrapper flex-column flex-row-fluid" id="kt_app_wrapper">
        <BPSidebar />
        <!--begin::Main-->
        <div class="app-main flex-column flex-row-fluid" id="kt_app_main">
          <!--begin::Content wrapper-->
          <div class="d-flex flex-column flex-column-fluid">
            <BPToolbar />
            <BPContent></BPContent>
          </div>
          <!--end::Content wrapper-->
          <BPFooter />
        </div>
        <!--end:::Main-->
      </div>
      <!--end::Wrapper-->
    </div>
    <!--end::Page-->
    <BPDrawers />
  </div>
  <!--end::App-->
</template>

<script lang="ts">
import { defineComponent, nextTick, onMounted, watch, inject } from "vue";
import BPHeader from "./header/Header.vue";
import BPSidebar from "./sidebar/Sidebar.vue";
import BPToolbar from "./toolbar/Toolbar.vue";
import BPContent from "./content/Content.vue";
import BPFooter from "./footer/Footer.vue";
import BPDrawers from "./drawers/Drawers.vue";
import { useStore } from "@/store";
import { useRoute, useRouter } from "vue-router";
import { reinitializeComponents } from "@/core/plugins/plugins";
import { MenuComponent } from "@/assets/ts/components";
import { removeModalBackdrop } from "@/core/helpers/dom";
import LayoutService from "@/core/services/BackendLayoutService";

export default defineComponent({
  name: "default-layout",
  components: {
    BPHeader,
    BPSidebar,
    BPContent,
    BPFooter,
    BPDrawers,
    BPToolbar,
  },
  setup() {
    const store = useStore();
    const route = useRoute();
    const router = useRouter();

    onMounted(() => {
      //check if current user is authenticated
      if (!store.getters.isUserAuthenticated) {
        router.push({ name: "sign-in" });
        return;
      }

      if (!store.getters.isUser2fa) {
        router.push({ name: "2fa" });
        return;
      }
      const appConfig = inject("appConfig");
      nextTick(() => {
        reinitializeComponents();
        LayoutService.init(appConfig);
      });
    });

    watch(
      () => route.path,
      () => {
        MenuComponent.hideDropdowns(undefined);
        nextTick(() => {
          reinitializeComponents();
        });
        removeModalBackdrop();
      }
    );
  },
});
</script>
