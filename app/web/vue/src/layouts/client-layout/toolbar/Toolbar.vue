<template>
  <!--begin::Toolbar-->
  <div id="kt_app_toolbar" class="app-toolbar py-3 py-lg-6">
    <!--begin::Toolbar container-->
    <div
      id="kt_app_toolbar_container"
      class="app-container"
      :class="[
        {
          'container-fluid': toolbarWidthFluid,
          'container-xxl': !toolbarWidthFluid,
        },
        isMobile ? '' : 'd-flex flex-stack',
      ]"
    >
      <KTPageTitle />
      <div class="d-flex align-items-center py-3 py-md-1">
        <!-- <IBSelector /> -->
        <!-- <TradeSelector /> -->
        <SalesSelector v-if="$can('Sales')" />
        <IBSelector v-if="$can('IB')" />
        <RepSelector v-if="$can('Rep')" />
      </div>
    </div>
    <!--end::Toolbar container-->
  </div>
  <!--end::Toolbar-->
</template>
<script lang="ts">
export default {
  name: "layout-toolbar",
};
</script>

<script lang="ts" setup>
import { toolbarWidthFluid } from "@/core/helpers/config";
import SalesSelector from "@/projects/client/modules/sales/components/SalesSelector.vue";
import RepSelector from "@/projects/client/modules/rep/components/RepSelector.vue";
// import IBSelector from "@/projects/client/modules/ib/components/IBSelector.vue";
// import TradeSelector from "@/projects/client/modules/reports/components/TradeSelector.vue";
import KTPageTitle from "@/layouts/client-layout/toolbar/PageTitle.vue";
import IBSelector from "@/projects/client/modules/ib/components/IBSelector.vue";
import { isMobile } from "@/core/config/WindowConfig";
defineExpose({
  // showIBSelector: (isShow: boolean) => (isShowIbSelector.value = isShow),
  // showSalesSelector: (isShow: boolean) => (isShowSalesSelector.value = isShow),
});
</script>
