<template>
  <!--begin::Toolbar wrapper-->
  <div class="d-flex align-items-stretch flex-shrink-0">
    <div class="topbar d-flex align-items-stretch flex-shrink-0 gap-3">
      <!--begin::Websockets Notity-->
      <BPWSNotify />
      <!--end::Websockets Notity-->

      <div
        class="align-items-center me-n3 ms-1 ms-lg-3 d-none d-lg-flex"
        v-if="
          $cans(['IB']) && currentAgentAccount?.uid > 0 && projectConfig
            ? projectConfig.ibEnabled
            : false
        "
      >
        <router-link class="btn btn-sm btn-secondary btn-radius" to="/ib"
          >{{ $t("title.ibCenter") }}
        </router-link>
      </div>

      <div
        class="align-items-center me-n3 ms-1 ms-lg-3 d-none d-lg-flex"
        v-if="$cans(['Sales']) && currentSalesAccount?.uid > 0"
      >
        <router-link class="btn btn-sm btn-radius btn-secondary" to="/sales">{{
          $t("title.salesCenter")
        }}</router-link>
      </div>

      <div
        class="align-items-center me-n3 ms-1 ms-lg-3 d-none d-lg-flex"
        v-if="$cans(['Rep']) && currentRepAccount?.uid > 0"
      >
        <router-link class="btn btn-sm btn-radius btn-secondary" to="/rep">{{
          $t("title.repCenter")
        }}</router-link>
      </div>
      <!--begin::User-->
      <div
        class="d-flex align-items-center me-n3 ms-1 ms-lg-3"
        id="kt_header_user_menu_toggle"
      >
        <!--begin::Menu-->

        <div
          class="ms-3 btn btn-icon btn-active-light-primary btn-custom w-30px h-30px w-md-40px h-md-40px symbol symbol-35px symbol-md-40px d-none d-lg-block"
          data-kt-menu-trigger="click"
          data-kt-menu-attach="parent"
          data-kt-menu-placement="bottom-end"
          data-kt-menu-flip="bottom"
        >
          <UserAvatar
            :avatar="user?.avatar"
            :name="user?.name"
            rounded
            side="client"
            size="40px"
            v-if="user"
          />
          <span
            :style="'background-color: ' + siteColor + ';'"
            class="headerSite"
          ></span>
        </div>
        <KTUserMenu></KTUserMenu>
        <!--end::Menu-->
      </div>
      <!--end::User -->
    </div>
  </div>
  <!--end::Toolbar wrapper-->
</template>
<script lang="ts">
export default {
  name: "layout-topbar",
};
</script>

<script lang="ts" setup>
import { computed, ref } from "vue";
import KTUserMenu from "@/layouts/client-layout/header/partials/UserMenu.vue";
import { useStore } from "@/store";
import BPWSNotify from "@/layouts/client-layout/notify/WSNotify.vue";
import { PublicSetting } from "@/core/types/ConfigTypes";

const store = useStore();
const currentAgentAccount = computed(
  () => store.state.AgentModule?.agentAccount
);
const currentSalesAccount = computed(
  () => store.state.SalesModule?.salesAccount
);
const currentRepAccount = computed(() => store.state.RepModule?.repAccount);
const user = store.state.AuthModule.user;
var siteColors = {
  au: "#7a9e7a",
  bvi: "#fa6b6c",
  sea: "#349beb",
  mn: "#b497b4",
  jp: "#f6c23e",
};
const siteColor = ref(siteColors[user.tenancy]);

const projectConfig = computed<PublicSetting>(
  () => store.state.AuthModule.config
);
</script>
<style>
.headerSite {
  position: absolute;
  bottom: 0;
  right: 0;
  width: 3px;
  height: 3px;
  border-radius: 50%;
}
.margin-right {
  margin-right: 0.75rem;
}
</style>
