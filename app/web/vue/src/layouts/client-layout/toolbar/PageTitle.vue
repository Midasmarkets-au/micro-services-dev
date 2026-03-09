<template>
  <!--begin::Page title-->
  <div
    v-if="pageTitleDisplay"
    :class="`page-title d-flex flex-${pageTitleDirection} justify-content-center flex-wrap me-3`"
  >
    <template v-if="pageTitle">
      <!--begin::Title-->
      <h1
        class="page-heading d-flex fw-bold flex-column justify-content-center my-0"
      >
        {{ $t(pageTitle) }}
      </h1>
      <!--end::Title-->

      <span
        v-if="pageTitleDirection === 'row' && pageTitleBreadcrumbDisplay"
        class="h-20px border-gray-200 border-start mx-3"
      ></span>
    </template>
  </div>
  <div v-else class="align-items-stretch"></div>
  <!--end::Page title-->
</template>

<script>
import { defineComponent, computed } from "vue";
import {
  pageTitleDisplay,
  pageTitleBreadcrumbDisplay,
  pageTitleDirection,
} from "@/core/helpers/config";
import { useRoute } from "vue-router";

export default defineComponent({
  name: "layout-page-title",
  components: {},
  setup() {
    const route = useRoute();

    const pageTitle = computed(() => {
      return route.meta.pageTitle;
    });

    const breadcrumbs = computed(() => {
      return route.meta.breadcrumbs;
    });

    return {
      pageTitle,
      breadcrumbs,
      pageTitleDisplay,
      pageTitleBreadcrumbDisplay,
      pageTitleDirection,
    };
  },
});
</script>

<style scoped lang="scss">
.page-heading {
  font-size: 3rem;
  font-family: PingFangSC-Medium;
  font-weight: 500;
  color: #000;
  @media (max-width: 767px) {
    padding: 15px 0;
    font-size: 2rem;
  }
}
</style>
