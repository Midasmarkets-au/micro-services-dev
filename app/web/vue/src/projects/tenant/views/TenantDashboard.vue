<template>
  <div>
    <div class="d-flex flex-wrap gap-6 align-items-start">
      <div class="flex-grow-1" style="min-width: 600px">
        <div class="d-flex flex-wrap gap-6">
          <AutoCreatePermission v-if="$can('TenantAdmin')" />
          <DevicesAndUsers v-if="$can('TenantAdmin')" />
          <DealingOnline v-if="$can('SuperAdmin')" />
        </div>
        <div class="d-flex flex-wrap gap-6 mt-4">
          <EmailCheck v-if="$can('TenantAdmin')" />
        </div>
      </div>
      <div
        class="flex-shrink-0"
        style="min-width: 360px; max-width: 100%"
        v-if="$can('TenantAdmin')"
      >
        <RebateInfo />
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import AutoCreatePermission from "./DashboardComp/AutoCreatePermission/AutoCreatePermission.vue";
import DevicesAndUsers from "./DashboardComp/DevicesAndUsers/DevicesAndUsers.vue";
import DealingOnline from "./DashboardComp/DealingOnline/DealingOnline.vue";
import EmailCheck from "../components/EmailCheck.vue";
import RebateInfo from "./DashboardComp/RebateInfo/RebateInfo.vue";
</script>
