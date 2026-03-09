<template>
  <div class="sub-menu sub-h2 d-inline-flex" style="white-space: nowrap">
    <router-link to="/supports" class="sub-menu-item active">{{
      $t("title.contactUs")
    }}</router-link>
    <router-link to="/supports/notices" class="sub-menu-item">{{
      $t("title.notices")
    }}</router-link>
    <router-link to="/supports/documents" class="sub-menu-item">{{
      $t("title.documents")
    }}</router-link>
    <router-link
      to="/supports/cases"
      class="sub-menu-item"
      v-if="$cans(['TenantAdmin'])"
      >{{ $t("title.cases") }}</router-link
    >
  </div>

  <BaSupport v-if="region == 'au'" />
  <JpSupport v-else-if="region == 'jp'" />
  <BviSupport v-else />
</template>

<script lang="ts" setup>
import { ref } from "vue";
import BaSupport from "../components/SupportIndex/BaSupport.vue";
import BviSupport from "../components/SupportIndex/BviSupport.vue";
import JpSupport from "../components/SupportIndex/JpSupport.vue";
import { useStore } from "@/store";

const store = useStore();
const user = store.state.AuthModule.user;
const region = ref(user.tenancy);
if (region.value == null || region.value == undefined) {
  region.value = process.env.VUE_APP_SITE;
}
</script>
