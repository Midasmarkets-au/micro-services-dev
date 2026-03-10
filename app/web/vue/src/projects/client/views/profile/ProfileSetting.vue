<template>
  <div class="mb-10" :class="isMobile ? 'px-3' : ''">
    <div>
      <CenterMenu activeMenuItem="profile" />
    </div>

    <div class="row">
      <div class="col-12 col-md-7 mb-3 mb-md-0">
        <div v-if="getTenancy == tenancies.jp">
          <BasicInfomationJp />
        </div>
        <div v-else>
          <BasicInformation />
        </div>
      </div>

      <div class="col-12 col-md-5 d-flex flex-column gap-5">
        <ChangePassword />
        <TwoFaSetting />
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import BasicInformation from "./BasicInfomation.vue";
import TwoFaSetting from "./components/TwoFaSetting.vue";
import ChangePassword from "./ChangePassword.vue";
import CenterMenu from "./components/CenterMenu.vue";
import BasicInfomationJp from "./jp/BasicInfomationJp.vue";
import { getTenancy, tenancies } from "@/core/types/TenantTypes";
import { isMobile } from "@/core/config/WindowConfig";
</script>
<style scoped>
.sub-menu {
  width: 100%;
  white-space: nowrap;
  overflow: auto;
}
</style>
