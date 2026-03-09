<template>
  <div class="ib-menu" :class="{ 'd-inline-flex': !isMobile }">
    <div class="sub-menu d-flex">
      <router-link
        to="/profile"
        class="sub-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.profile }"
        >{{ $t("title.myDetails") }}</router-link
      >
      <!-- <router-link
        to="/profile/inbox"
        class="sub-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.inbox }"
        >{{ $t("title.inbox") }}</router-link
      > -->
      <router-link
        v-if="$can('Client')"
        to="/profile/bank-infos"
        class="sub-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.bankInfo }"
        >{{ $t("title.bankInfo") }}</router-link
      >
      <router-link
        v-if="getTenancy != tenancies.jp"
        to="/profile/file-upload"
        class="sub-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.fileUpload }"
        >{{ $t("title.fileUpload") }}</router-link
      >
      <router-link
        to="/profile/address"
        v-if="$cans(['EventShop'])"
        class="sub-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.address }"
        >{{ $t("title.address") }}</router-link
      >
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import { moibleNavScroller } from "@/core/utils/mobileNavScroller";
import { getTenancy, tenancies } from "@/core/types/TenantTypes";
import { isMobile } from "@/core/config/WindowConfig";
const menuItem = {
  profile: "profile",
  inbox: "inbox",
  bankInfo: "bankInfo",
  fileUpload: "fileUpload",
  address: "address",
};

const props = defineProps<{
  activeMenuItem: any;
}>();

onMounted(() => {
  moibleNavScroller(".sub-menu", ".active");
  moibleNavScroller(".ib-menu", ".active");
});
</script>

<style scoped lang="scss">
.sub-menu {
  width: 100%;
  white-space: nowrap;
}
</style>
